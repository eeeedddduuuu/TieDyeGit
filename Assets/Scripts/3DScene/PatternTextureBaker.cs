using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatternTextureBaker : MonoBehaviour
{
    [Header("核心组件引用")]
    public RectTransform offScreenCanvasRect;
    public Camera bakingCamera;
    public GameObject clothObject;

    [Header("资源配置")]
    public GameObject patternImagePrefab;
    public List<PatternData> allPatternConfig;

    [Header("扎染风格设置")]
    public Color tieDyeBackgroundColor = new Color(0.1f, 0.3f, 0.8f, 1f);
    public Color patternTintColor = Color.white;

    [Header("烘焙设置")]
    public float resolutionScale = 2.0f;
    public bool useNativeSize = true;
    public Vector2 manualBaseSize = new Vector2(300, 300);

    private RenderTexture bakedTexture;

    void Start()
    {
        // 改用协程：等待一帧再拍照，确保UI已经排版完成
        StartCoroutine(ProcessDesignAndBake());
    }
    // --- 新增：供外部调用的改色方法 ---
    public void UpdateBackgroundColor(Color newColor)
    {
        // 1. 更新内存中的颜色变量
        tieDyeBackgroundColor = newColor;

        // 2. 找到画布上的背景层并改色
        if (offScreenCanvasRect != null)
        {
            Transform bgTrans = offScreenCanvasRect.Find("BackgroundLayer");
            if (bgTrans != null)
            {
                Image bgImg = bgTrans.GetComponent<Image>();
                if (bgImg != null)
                {
                    bgImg.color = newColor;
                }
            }
        }

        // 3. 立即重新拍照应用
        // 注意：如果你之前的 GetDesignData 逻辑比较耗时，
        // 这里可以直接复用 canvasSize 而不需要重新 GetDesignData
        if (DesignDataTransfer.CurrentDesignData != null)
        {
            BakeAndApplyTexture(DesignDataTransfer.CurrentDesignData.canvasSize);
        }
        else
        {
            // 如果没有数据，就用当前画布的尺寸兜底
            BakeAndApplyTexture(offScreenCanvasRect.sizeDelta);
        }
    }

    IEnumerator ProcessDesignAndBake()
    {
        // 1. 获取数据
        CanvasDesignData dataToRender = GetDesignData();
        if (dataToRender == null) yield break;

        // 2. 重建花纹
        SetupCanvasSize(dataToRender.canvasSize);
        ReconstructPatternOnCanvas(dataToRender);

        // 3. 【关键】强制刷新 Canvas，确保刚刚生成的物体都摆好了
        Canvas.ForceUpdateCanvases();

        // 4. 【关键】等待这一帧结束，确保画面渲染就绪
        yield return new WaitForEndOfFrame();

        // 5. 自动把摄像机瞬移到画布正前方，并调整大小
        AutoAlignCamera();

        // 6. 拍照并应用
        BakeAndApplyTexture(dataToRender.canvasSize);
    }

    // --- 自动对齐摄像机（解决拍不到的问题） ---
    void AutoAlignCamera()
    {
        if (offScreenCanvasRect == null || bakingCamera == null) return;

        Vector3 centerPos = offScreenCanvasRect.position;

        // 【修改】把 -10 改成 -50，确保离得足够远能拍到全景
        bakingCamera.transform.position = centerPos + new Vector3(0, 0, -50);

        bakingCamera.transform.rotation = Quaternion.identity;
        bakingCamera.orthographic = true;

        float canvasHeight = offScreenCanvasRect.rect.height * offScreenCanvasRect.lossyScale.y;
        bakingCamera.orthographicSize = canvasHeight / 2f;

        bakingCamera.nearClipPlane = 0.1f;
        bakingCamera.farClipPlane = 1000f; // 【修改】看远一点
    }

    CanvasDesignData GetDesignData()
    {
        if (DesignDataTransfer.CurrentDesignData != null) return DesignDataTransfer.CurrentDesignData;
        var savedData = DesignSaveManager.LoadDesign();
        if (savedData != null && savedData.placements.Count > 0) return savedData;

        // 生成测试数据
        CanvasDesignData testData = new CanvasDesignData();
        testData.canvasSize = new Vector2(800, 600);
        if (allPatternConfig.Count > 0)
        {
            testData.placements.Add(new PatternPlacement
            {
                patternId = allPatternConfig[0].patternId,
                position = Vector2.zero,
                scale = Vector2.one * 3,
                rotation = 45
            });
        }
        return testData;
    }

    void SetupCanvasSize(Vector2 size)
    {
        if (offScreenCanvasRect != null) offScreenCanvasRect.sizeDelta = size;
    }

    void ReconstructPatternOnCanvas(CanvasDesignData data)
    {
        // 1. 清理旧物体
        foreach (Transform child in offScreenCanvasRect) Destroy(child.gameObject);

        // 2. 创建背景层
        GameObject bgObj = Instantiate(patternImagePrefab, offScreenCanvasRect);
        bgObj.name = "BackgroundLayer";

        // 【关键修复】强制将新生成的背景改到和 Canvas 一样的层级 (比如 UI 层)
        bgObj.layer = offScreenCanvasRect.gameObject.layer;

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        Image bgImg = bgObj.GetComponent<Image>();
        bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero; bgRect.anchoredPosition = Vector2.zero;
        bgImg.sprite = null;
        bgImg.color = tieDyeBackgroundColor;

        // 3. 生成花纹层
        foreach (var placement in data.placements)
        {
            GameObject newObj = Instantiate(patternImagePrefab, offScreenCanvasRect);
            newObj.name = "Pattern_" + placement.patternId;
            newObj.layer = offScreenCanvasRect.gameObject.layer; // 保持层级一致

            Image img = newObj.GetComponent<Image>();
            RectTransform rect = newObj.GetComponent<RectTransform>();

            PatternData pData = allPatternConfig.Find(p => p.patternId == placement.patternId);

            // 临时变量，用于记录图片的基础宽高比
            Vector2 finalSize = manualBaseSize;

            if (pData != null)
            {
                // 优先用白图，没有则用黑图
                Sprite targetSprite = (pData.whiteTextureSprite != null) ? pData.whiteTextureSprite : pData.patternSprite;
                img.sprite = targetSprite;
                img.color = patternTintColor;

                // 【核心修改】尺寸计算逻辑
                if (useNativeSize && targetSprite != null)
                {
                    // 方案A: 使用图片的真实像素大小 (比如图片是 512x512，这里就设为 512x512)
                    img.SetNativeSize();
                    finalSize = rect.sizeDelta; // 记录下 NativeSize 后的尺寸
                }
                else
                {
                    // 方案B: 使用手动指定的大小，但保持长宽比
                    img.preserveAspect = true;
                    if (targetSprite != null)
                    {
                        // 根据图片比例自动调整宽高，防止拉伸
                        float aspect = targetSprite.rect.width / targetSprite.rect.height;
                        if (aspect >= 1) // 宽图
                            finalSize = new Vector2(manualBaseSize.x, manualBaseSize.x / aspect);
                        else // 长图
                            finalSize = new Vector2(manualBaseSize.y * aspect, manualBaseSize.y);
                    }
                }
            }

            // 应用位置和旋转
            rect.anchoredPosition = placement.position;
            rect.localRotation = Quaternion.Euler(0, 0, placement.rotation);

            // 【核心修改】应用缩放
            // 这里的 Scale 是你在设计场景里用鼠标滚轮缩放的倍数 (比如 1.5倍)
            // 最终大小 = 基础大小 * 缩放倍数
            rect.localScale = new Vector3(placement.scale.x, placement.scale.y, 1f);

            // 应用基础大小 (替代之前写死的 80, 80)
            // 如果上面使用了 SetNativeSize，这里如果不重设，缩放可能会叠加出问题，所以我们要显式控制一下
            if (!useNativeSize)
            {
                rect.sizeDelta = finalSize;
            }
            // 如果用了 NativeSize，Instantiate 出来的 Image 已经自动有了正确 sizeDelta，不需要再赋值，除非被 Scale 影响
        }
    }

    void BakeAndApplyTexture(Vector2 canvasSize)
    {
        if (bakedTexture != null) bakedTexture.Release();

        int width = (int)(canvasSize.x * resolutionScale);
        int height = (int)(canvasSize.y * resolutionScale);

        bakedTexture = new RenderTexture(width, height, 0);
        bakedTexture.name = "BakedTieDyeTexture";

        bakingCamera.targetTexture = bakedTexture;
        bakingCamera.enabled = true; // 开启
        bakingCamera.Render();       // 拍照
        bakingCamera.enabled = false;
        bakingCamera.targetTexture = null;

        // 【修改】动态寻找当前存活的渲染器 (SkinnedMeshRenderer)
        if (clothObject != null)
        {
            Renderer currentRenderer = clothObject.GetComponent<Renderer>();

            if (currentRenderer != null)
            {
                Material mat = currentRenderer.material;

                // 暴力赋值，确保万无一失
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", bakedTexture);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", bakedTexture);

                // 确保颜色是白色，防止变暗
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);

                Debug.Log($"纹理已应用到 {currentRenderer.GetType().Name}");
            }
            else
            {
                Debug.LogError("在布料物体上找不到 Renderer 组件！");
            }
        }
    }

        void OnDestroy()
    {
        if (bakedTexture != null) bakedTexture.Release();
    }
}