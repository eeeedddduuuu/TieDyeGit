using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    // 单例模式
    private static SceneTransitionManager _instance;
    public static SceneTransitionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SceneTransitionManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("SceneTransitionManager");
                    _instance = obj.AddComponent<SceneTransitionManager>();
                    DontDestroyOnLoad(obj);
                    _instance.Initialize();
                }
            }
            return _instance;
        }
    }
    
    [Header("过渡效果设置")]
    public TransitionType transitionType = TransitionType.Fade;
    public float transitionDuration = 0.5f;
    public Color transitionColor = Color.black;
    
    [Header("特殊过渡效果参数")]
    public float wipeSpeed = 2f;
    public float scaleDuration = 0.4f;
    public float rotateDuration = 0.6f;
    public int pixelSize = 8;
    
    [Header("扎染效果参数")]
    public bool useTieDyeEffect = true;
    public AnimationCurve tieDyeIntensityCurve;
    public Color tieDyeColor1 = new Color(0.8f, 0.2f, 0.2f, 0.8f); // 红色系
    public Color tieDyeColor2 = new Color(0.2f, 0.4f, 0.8f, 0.8f); // 蓝色系
    public float tieDyeSpeed = 1.0f;
    
    // 过渡UI组件
    private Image transitionImage;
    private Canvas transitionCanvas;
    private Material transitionMaterial;
    
    // 状态标志
    private bool isTransitioning = false;
    private bool initialized = false;
    
    // 过渡类型枚举
    public enum TransitionType
    {
        Fade,           // 淡入淡出
        Wipe,           // 擦拭效果
        Slide,          // 滑动效果
        Scale,          // 缩放效果
        Rotate,         // 旋转效果
        Pixelize,       // 像素化效果
        TieDyeWipe      // 扎染风格擦拭
    }
    
    // 初始化
    private void Initialize()
    {
        if (initialized)
            return;
        
        // 创建过渡Canvas
        transitionCanvas = gameObject.AddComponent<Canvas>();
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = int.MaxValue; // 确保在最顶层
        
        // 创建过渡Image
        GameObject imageObj = new GameObject("TransitionImage");
        imageObj.transform.SetParent(transitionCanvas.transform, false);
        
        transitionImage = imageObj.AddComponent<Image>();
        transitionImage.rectTransform.anchorMin = Vector2.zero;
        transitionImage.rectTransform.anchorMax = Vector2.one;
        transitionImage.rectTransform.offsetMin = Vector2.zero;
        transitionImage.rectTransform.offsetMax = Vector2.zero;
        transitionImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, 0f);
        
        // 初始化扎染效果曲线
        if (tieDyeIntensityCurve.length == 0)
        {
            tieDyeIntensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        }
        
        initialized = true;
    }
    
    // 加载场景（带动画过渡）
    public void LoadScene(string sceneName, TransitionType customType = TransitionType.Fade)
    {
        if (isTransitioning || !SceneExists(sceneName))
        {
            Debug.LogWarning($"场景 '{sceneName}' 不存在或过渡正在进行中");
            return;
        }
        
        Initialize();
        StartCoroutine(TransitionToScene(sceneName, customType));
    }
    
    // 过渡到新场景的协程
    private IEnumerator TransitionToScene(string sceneName, TransitionType customType)
    {
        isTransitioning = true;
        
        // 使用自定义过渡类型或默认类型
        TransitionType typeToUse = customType == TransitionType.Fade ? transitionType : customType;
        
        // 淡出当前场景
        yield return StartCoroutine(PlayTransitionOut(typeToUse));
        
        // 异步加载新场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        // 等待场景加载到90%
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        // 激活场景
        asyncLoad.allowSceneActivation = true;
        
        // 等待一帧确保场景完全加载
        yield return null;
        
        // 淡入新场景
        yield return StartCoroutine(PlayTransitionIn(typeToUse));
        
        isTransitioning = false;
    }
    
    // 淡出过渡
    private IEnumerator PlayTransitionOut(TransitionType type)
    {
        float elapsedTime = 0f;
        
        switch (type)
        {
            case TransitionType.Fade:
                while (elapsedTime < transitionDuration)
                {
                    float alpha = Mathf.Lerp(0f, 1f, elapsedTime / transitionDuration);
                    transitionImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, alpha);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                transitionImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, 1f);
                break;
                
            case TransitionType.Wipe:
                // 从左到右擦拭
                RectTransform rt = transitionImage.rectTransform;
                float startWidth = rt.sizeDelta.x;
                rt.sizeDelta = new Vector2(0, rt.sizeDelta.y);
                
                while (elapsedTime < transitionDuration)
                {
                    float width = Mathf.Lerp(0f, startWidth * 2, elapsedTime / transitionDuration);
                    rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                rt.sizeDelta = new Vector2(startWidth * 2, rt.sizeDelta.y);
                break;
                
            case TransitionType.TieDyeWipe:
                // 扎染风格的擦拭效果
                if (useTieDyeEffect)
                {
                    float tieDyeTime = 0f;
                    
                    while (tieDyeTime < transitionDuration)
                    {
                        float progress = tieDyeTime / transitionDuration;
                        float intensity = tieDyeIntensityCurve.Evaluate(progress);
                        
                        // 创建扎染效果的混合颜色
                        float blendFactor = Mathf.Sin(tieDyeTime * tieDyeSpeed) * 0.5f + 0.5f;
                        Color blendColor = Color.Lerp(tieDyeColor1, tieDyeColor2, blendFactor);
                        blendColor.a = Mathf.Lerp(0f, 1f, progress);
                        
                        transitionImage.color = blendColor;
                        tieDyeTime += Time.deltaTime;
                        yield return null;
                    }
                }
                else
                {
                    // 如果不使用扎染效果，使用普通淡入
                    while (elapsedTime < transitionDuration)
                    {
                        float alpha = Mathf.Lerp(0f, 1f, elapsedTime / transitionDuration);
                        transitionImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, alpha);
                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }
                }
                transitionImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, 1f);
                break;
                
            // 其他过渡效果的实现...
            default:
                // 默认使用淡入淡出
                while (elapsedTime < transitionDuration)
                {
                    float alpha = Mathf.Lerp(0f, 1f, elapsedTime / transitionDuration);
                    transitionImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, alpha);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                transitionImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, 1f);
                break;
        }
    }
    
    // 淡入过渡
    private IEnumerator PlayTransitionIn(TransitionType type)
    {
        float elapsedTime = 0f;
        
        switch (type)
        {
            case TransitionType.Fade:
                while (elapsedTime < transitionDuration)
                {
                    float alpha = Mathf.Lerp(1f, 0f, elapsedTime / transitionDuration);
                    transitionImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, alpha);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                transitionImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, 0f);
                break;
                
            case TransitionType.Wipe:
                // 从右到左擦拭
                RectTransform rt = transitionImage.rectTransform;
                float endWidth = 0;
                float startWidth = rt.sizeDelta.x;
                
                while (elapsedTime < transitionDuration)
                {
                    float width = Mathf.Lerp(startWidth, endWidth, elapsedTime / transitionDuration);
                    rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                rt.sizeDelta = new Vector2(endWidth, rt.sizeDelta.y);
                break;
                
            case TransitionType.TieDyeWipe:
                // 扎染风格的擦拭效果
                if (useTieDyeEffect)
                {
                    float tieDyeTime = 0f;
                    
                    while (tieDyeTime < transitionDuration)
                    {
                        float progress = 1 - (tieDyeTime / transitionDuration);
                        float intensity = tieDyeIntensityCurve.Evaluate(progress);
                        
                        // 创建扎染效果的混合颜色
                        float blendFactor = Mathf.Sin((transitionDuration - tieDyeTime) * tieDyeSpeed) * 0.5f + 0.5f;
                        Color blendColor = Color.Lerp(tieDyeColor1, tieDyeColor2, blendFactor);
                        blendColor.a = Mathf.Lerp(1f, 0f, tieDyeTime / transitionDuration);
                        
                        transitionImage.color = blendColor;
                        tieDyeTime += Time.deltaTime;
                        yield return null;
                    }
                }
                else
                {
                    // 如果不使用扎染效果，使用普通淡出
                    while (elapsedTime < transitionDuration)
                    {
                        float alpha = Mathf.Lerp(1f, 0f, elapsedTime / transitionDuration);
                        transitionImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, alpha);
                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }
                }
                transitionImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, 0f);
                break;
                
            // 其他过渡效果的实现...
            default:
                // 默认使用淡入淡出
                while (elapsedTime < transitionDuration)
                {
                    float alpha = Mathf.Lerp(1f, 0f, elapsedTime / transitionDuration);
                    transitionImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, alpha);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                transitionImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, 0f);
                break;
        }
    }
    
    // 检查场景是否存在
    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
            {
                return true;
            }
        }
        return false;
    }
    
    // 设置过渡类型
    public void SetTransitionType(TransitionType type)
    {
        transitionType = type;
    }
    
    // 设置过渡持续时间
    public void SetTransitionDuration(float duration)
    {
        if (duration > 0)
        {
            transitionDuration = duration;
        }
    }
}