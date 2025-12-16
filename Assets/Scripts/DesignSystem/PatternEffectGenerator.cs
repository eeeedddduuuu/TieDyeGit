using UnityEngine;
using UnityEngine.UI;

public class PatternEffectGenerator : MonoBehaviour
{
    [Header("渲染设置")]
    public int textureSize = 1024;
    public Color backgroundColor = Color.blue;
    public Color patternColor = Color.white;

    [Header("UI引用")]
    public RawImage previewImage;
    public RenderTexture renderTexture;

    [Header("材质和Shader")]
    public Material effectMaterial;

    private Texture2D generatedTexture;
    private CanvasDesignData currentDesign;

    void Start()
    {
        // 获取设计数据
        if (DesignDataTransfer.CurrentDesignData != null)
        {
            currentDesign = DesignDataTransfer.CurrentDesignData;
            GenerateEffectTexture();
        }
        else
        {
            Debug.LogError("没有找到设计数据！");
            // 尝试从保存数据加载
            DesignDataTransfer.LoadFromSavedData();
            if (DesignDataTransfer.CurrentDesignData != null)
            {
                currentDesign = DesignDataTransfer.CurrentDesignData;
                GenerateEffectTexture();
            }
        }
    }

    // 生成效果纹理
    public void GenerateEffectTexture()
    {
        if (currentDesign == null)
        {
            Debug.LogError("无法生成效果：没有设计数据");
            return;
        }

        // 创建新纹理
        generatedTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        generatedTexture.filterMode = FilterMode.Bilinear;
        generatedTexture.wrapMode = TextureWrapMode.Clamp;

        // 初始化纹理为背景色
        Color[] pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }
        generatedTexture.SetPixels(pixels);

        // 绘制所有花纹
        foreach (var placement in currentDesign.placements)
        {
            DrawPatternOnTexture(generatedTexture, placement);
        }

        generatedTexture.Apply();

        // 更新预览
        if (previewImage != null)
        {
            previewImage.texture = generatedTexture;
        }

        Debug.Log($"效果纹理生成完成: {textureSize}x{textureSize}, {currentDesign.placements.Count} 个花纹");
    }

    // 在纹理上绘制单个花纹
    private void DrawPatternOnTexture(Texture2D targetTexture, PatternPlacement placement)
    {
        // 查找花纹素材
        PatternData patternData = FindPatternDataById(placement.patternId);
        if (patternData == null || patternData.patternTexture == null)
        {
            Debug.LogWarning($"找不到花纹纹理: {placement.patternId}");
            return;
        }

        Texture2D patternTex = patternData.patternTexture;

        // 计算绘制位置和大小
        Vector2 center = new Vector2(targetTexture.width / 2, targetTexture.height / 2);
        Vector2 canvasCenter = currentDesign.canvasSize / 2;

        // 坐标转换：从UI坐标到纹理坐标
        Vector2 texturePos = center + (placement.position - canvasCenter) * (textureSize / currentDesign.canvasSize.x);

        // 计算缩放后的尺寸
        int drawWidth = (int)(patternTex.width * placement.scale.x);
        int drawHeight = (int)(patternTex.height * placement.scale.y);

        // 简单的绘制实现（后续可以优化为支持旋转）
        DrawPatternSimple(targetTexture, patternTex, texturePos, drawWidth, drawHeight);
    }

    // 简单绘制方法（不支持旋转）
    private void DrawPatternSimple(Texture2D target, Texture2D pattern, Vector2 center, int width, int height)
    {
        int startX = (int)(center.x - width / 2);
        int startY = (int)(center.y - height / 2);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 计算源纹理坐标
                int srcX = (int)(x * (pattern.width / (float)width));
                int srcY = (int)(y * (pattern.height / (float)height));

                if (srcX < 0 || srcX >= pattern.width || srcY < 0 || srcY >= pattern.height)
                    continue;

                Color patternColor = pattern.GetPixel(srcX, srcY);

                // 只绘制不透明的像素
                if (patternColor.a > 0.1f)
                {
                    int targetX = startX + x;
                    int targetY = startY + y;

                    if (targetX >= 0 && targetX < target.width && targetY >= 0 && targetY < target.height)
                    {
                        target.SetPixel(targetX, targetY, this.patternColor);
                    }
                }
            }
        }
    }

    // 根据ID查找花纹数据
    private PatternData FindPatternDataById(string patternId)
    {
        // 这里需要获取可用的花纹列表
        // 可以创建一个全局的PatternDatabase，或者通过FindObjectOfType查找DesignManager
        DesignManager designManager = FindObjectOfType<DesignManager>();
        if (designManager != null)
        {
            return designManager.availablePatterns.Find(p => p.patternId == patternId);
        }

        return null;
    }

    // 保存生成的纹理
    public void SaveGeneratedTexture()
    {
        if (generatedTexture != null)
        {
            byte[] pngData = generatedTexture.EncodeToPNG();
            string filePath = Application.persistentDataPath + $"/{currentDesign.designName}_{System.DateTime.Now:yyyyMMddHHmmss}.png";
            System.IO.File.WriteAllBytes(filePath, pngData);
            Debug.Log($"纹理已保存: {filePath}");
        }
    }

    // 应用效果到材质
    public void ApplyToMaterial()
    {
        if (effectMaterial != null && generatedTexture != null)
        {
            effectMaterial.mainTexture = generatedTexture;
            Debug.Log("效果已应用到材质");
        }
    }
}