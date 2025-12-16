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
    // 【新增】专门用来装花纹的容器，防止清理时把背景图也删了
    public Transform patternContainer;

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
        if (DesignDataTransfer.CurrentDesignData != null)
        {
            // 把当前数据传进去
            ReconstructPatternOnCanvas(DesignDataTransfer.CurrentDesignData);

            // ... 颜色设置代码 ...

            BakeAndApplyTexture(DesignDataTransfer.CurrentDesignData.canvasSize);
        }
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

    // 2. 【修改】重构花纹逻辑 (确保先清空)
    // 注意：请将原有的 ReconstructPatternOnCanvas 修改为接受参数的版本
    private void ReconstructPatternOnCanvas(CanvasDesignData data)
    {
        // --- 关键步骤：先清空画布上现有的所有花纹 ---
        if (patternContainer != null)
        {
            foreach (Transform child in patternContainer)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogError("PatternContainer 为空，请检查 Inspector 赋值！");
            return;
        }

        // --- 接下来是生成新花纹 (和你原本的 Start 里的逻辑类似) ---
        foreach (var placement in data.placements)
        {
            // 1. 实例化 Image 预制体
            GameObject newObj = Instantiate(patternImagePrefab, patternContainer);
            RectTransform rect = newObj.GetComponent<RectTransform>();
            Image img = newObj.GetComponent<Image>();

            // 2. 还原位置、旋转、缩放
            rect.anchoredPosition = placement.position;
            rect.localEulerAngles = new Vector3(0, 0, placement.rotation);
            rect.localScale = placement.scale;

            // 3. 还原图片内容 (查找 ID)
            // ---------------------------------------------------------
            // 这是一个混合查找逻辑：先查系统库，再查用户上传库
            // ---------------------------------------------------------
            PatternData pData = allPatternConfig.Find(p => p.patternId == placement.patternId);

            // 如果系统库没找到，去用户库找 (防止上传的图片变白块)
            if (pData == null && UserPatternStorage.userPatterns.ContainsKey(placement.patternId))
            {
                pData = UserPatternStorage.userPatterns[placement.patternId];
            }

            if (pData != null)
            {
                // 注意：3D场景我们要用 whiteTextureSprite (黑底白花)
                if (pData.whiteTextureSprite != null)
                    img.sprite = pData.whiteTextureSprite;
                else if (pData.patternSprite != null)
                    img.sprite = pData.patternSprite; // 保底

                // 针对用户上传图片的尺寸修正 (防止巨大化)
                if (placement.patternId.StartsWith("User_"))
                {
                    img.SetNativeSize();
                    if (rect.sizeDelta.x > 300) // 限制最大宽度
                    {
                        float ratio = rect.sizeDelta.y / rect.sizeDelta.x;
                        rect.sizeDelta = new Vector2(300, 300 * ratio);
                    }
                }
                else
                {
                    img.SetNativeSize(); // 普通花纹也重置一下大小
                }
            }
        }
    }

    public void BakeAndApplyTexture(Vector2 canvasSize)
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
    // 1. 【新增】供外部调用的总接口：应用历史数据
    public void ApplyHistoryData(CanvasDesignData historyData)
    {
        // A. 更新背景色
        UpdateBackgroundColor(historyData.savedBackgroundColor);

        // B. 重构花纹 (传入数据)
        ReconstructPatternOnCanvas(historyData);

        // C. 重新拍照
        BakeAndApplyTexture(historyData.canvasSize);
    }

    void OnDestroy()
    {
        if (bakedTexture != null) bakedTexture.Release();
    }
}