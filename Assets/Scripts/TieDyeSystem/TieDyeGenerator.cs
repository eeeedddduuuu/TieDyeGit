using UnityEngine;
using System.Collections.Generic;

public class TieDyeGenerator : MonoBehaviour
{
    [Header("材质和纹理")]
    public Material tieDyeMaterial;
    public List<PatternLineData> patternLineTextures = new List<PatternLineData>();

    [Header("布料纹理增强")]
    public Texture2D clothBaseTexture; // 基础布料纹理
    public Texture2D clothNormalMap;   // 布料法线贴图
    public Texture2D clothRoughnessMap; // 布料粗糙度贴图
    public float clothTextureBlend = 0.3f; // 布料纹理混合强度

    [Header("生成设置")]
    public int textureSize = 1024;
    public Color baseColor = Color.blue;
    public Color dyeColor = Color.white;

    [Header("布料质感增强")]
    [Range(0f, 1f)] public float fabricSoftness = 0.7f;
    [Range(0f, 1f)] public float fabricRoughness = 0.8f;
    [Range(0f, 1f)] public float fabricTextureDetail = 0.6f;
    public bool enableFabricWeave = true;
    public Vector2 weaveScale = new Vector2(4f, 4f);

    [Header("颜色混合增强")]
    [Range(0f, 1f)] public float colorBleeding = 0.3f;
    [Range(0f, 1f)] public float dyeAbsorption = 0.6f;

    [Header("映射设置")]
    public bool usePreciseMapping = true;
    public float patternScaleFactor = 1.0f;

    [Header("布料物理")]
    public ClothPhysicsManager clothPhysicsManager;

    [Header("调试选项")]
    public bool debugMode = true;

    // 私有变量
    private Texture2D finalTieDyeTexture;
    private Texture2D fabricWeaveTexture;
    private DesignSessionData loadedSessionData;

    void Start()
    {
        Debug.Log("TieDyeGenerator 开始运行 - 增强布料质感模式");

        GenerateFabricWeaveTexture();
        LoadDesignData();
        GenerateTieDyeTexture();
        ApplyToMaterial();

        Debug.Log("扎染纹理生成完成！");
    }

    // 生成布料编织纹理
    void GenerateFabricWeaveTexture()
    {
        if (!enableFabricWeave) return;

        int weaveSize = 256;
        fabricWeaveTexture = new Texture2D(weaveSize, weaveSize);
        fabricWeaveTexture.filterMode = FilterMode.Bilinear;
        fabricWeaveTexture.wrapMode = TextureWrapMode.Repeat;

        for (int x = 0; x < weaveSize; x++)
        {
            for (int y = 0; y < weaveSize; y++)
            {
                // 创建编织图案
                float u = (float)x / weaveSize * weaveScale.x;
                float v = (float)y / weaveSize * weaveScale.y;

                float weave = Mathf.Sin(u * Mathf.PI * 2f) * Mathf.Sin(v * Mathf.PI * 2f);
                weave = Mathf.Abs(weave);

                // 添加一些随机变化
                float noise = Mathf.PerlinNoise(u * 8f, v * 8f) * 0.2f;
                weave = Mathf.Clamp01(weave + noise);

                Color weaveColor = new Color(weave, weave, weave, 1f);
                fabricWeaveTexture.SetPixel(x, y, weaveColor);
            }
        }

        fabricWeaveTexture.Apply();
    }

    // 加载设计数据
    void LoadDesignData()
    {
        string jsonData = PlayerPrefs.GetString("CurrentDesign", "");
        if (!string.IsNullOrEmpty(jsonData))
        {
            try
            {
                loadedSessionData = JsonUtility.FromJson<DesignSessionData>(jsonData);
                Debug.Log($"成功加载设计数据，包含 {loadedSessionData.designData.placements.Count} 个花纹");
                Debug.Log($"设计区域尺寸: {loadedSessionData.canvasSize}");

                if (debugMode)
                {
                    foreach (var placement in loadedSessionData.designData.placements)
                    {
                        Debug.Log($"花纹ID: {placement.patternId}, 位置: {placement.position}, 缩放: {placement.scale}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"解析设计数据失败: {e.Message}");
                LoadLegacyDesignData(jsonData);
            }
        }
        else
        {
            Debug.LogWarning("没有找到设计数据！");
            loadedSessionData = new DesignSessionData
            {
                designData = new CanvasDesignData(),
                canvasSize = new Vector2(400, 400)
            };
        }
    }

    // 兼容旧格式的设计数据
    void LoadLegacyDesignData(string jsonData)
    {
        Debug.Log("尝试加载旧格式设计数据");
        loadedSessionData = new DesignSessionData
        {
            designData = JsonUtility.FromJson<CanvasDesignData>(jsonData),
            canvasSize = new Vector2(400, 400)
        };
    }

    // 生成扎染纹理
    void GenerateTieDyeTexture()
    {
        finalTieDyeTexture = new Texture2D(textureSize, textureSize);
        finalTieDyeTexture.filterMode = FilterMode.Bilinear;
        finalTieDyeTexture.wrapMode = TextureWrapMode.Repeat;

        // 填充基础颜色并添加布料质感
        Color[] basePixels = new Color[textureSize * textureSize];
        for (int i = 0; i < basePixels.Length; i++)
        {
            int x = i % textureSize;
            int y = i / textureSize;

            Color pixelColor = ApplyFabricTexture(baseColor, x, y);
            basePixels[i] = pixelColor;
        }
        finalTieDyeTexture.SetPixels(basePixels);

        // 绘制每个花纹
        foreach (var placement in loadedSessionData.designData.placements)
        {
            DrawPatternOnTexture(placement);
        }

        // 应用颜色渗出效果
        if (colorBleeding > 0.01f)
        {
            ApplyColorBleeding();
        }

        // 应用布料软化效果
        ApplyFabricSoftening();

        finalTieDyeTexture.Apply();
        Debug.Log($"扎染纹理生成完成，尺寸: {textureSize}x{textureSize}");
    }

    // 应用布料纹理
    Color ApplyFabricTexture(Color baseColor, int x, int y)
    {
        Color result = baseColor;

        // 应用基础布料纹理
        if (clothBaseTexture != null)
        {
            float u = (float)x / textureSize;
            float v = (float)y / textureSize;

            Color clothColor = clothBaseTexture.GetPixelBilinear(u * 4f, v * 4f);
            result = Color.Lerp(baseColor, clothColor * baseColor, clothTextureBlend);
        }

        // 应用编织纹理
        if (enableFabricWeave && fabricWeaveTexture != null)
        {
            float u = (float)x / textureSize * weaveScale.x;
            float v = (float)y / textureSize * weaveScale.y;

            Color weaveColor = fabricWeaveTexture.GetPixelBilinear(u, v);
            float weaveInfluence = Mathf.Lerp(0.1f, 0.3f, fabricTextureDetail);
            result *= Color.Lerp(Color.white, weaveColor, weaveInfluence);
        }

        return result;
    }

    // 应用颜色渗出效果
    void ApplyColorBleeding()
    {
        Texture2D tempTexture = new Texture2D(textureSize, textureSize);
        tempTexture.SetPixels(finalTieDyeTexture.GetPixels());

        for (int x = 1; x < textureSize - 1; x++)
        {
            for (int y = 1; y < textureSize - 1; y++)
            {
                if (Random.value > colorBleeding) continue;

                Color currentColor = tempTexture.GetPixel(x, y);
                if (currentColor.r + currentColor.g + currentColor.b < 2.5f) continue;

                // 随机选择一个相邻像素
                int dx = Random.Range(-2, 3);
                int dy = Random.Range(-2, 3);

                int targetX = Mathf.Clamp(x + dx, 0, textureSize - 1);
                int targetY = Mathf.Clamp(y + dy, 0, textureSize - 1);

                Color targetColor = tempTexture.GetPixel(targetX, targetY);
                Color blendedColor = Color.Lerp(targetColor, currentColor, 0.3f * dyeAbsorption);

                finalTieDyeTexture.SetPixel(targetX, targetY, blendedColor);
            }
        }

        DestroyImmediate(tempTexture);
    }

    // 应用布料软化效果
    void ApplyFabricSoftening()
    {
        if (fabricSoftness < 0.01f) return;

        Color[] pixels = finalTieDyeTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            // 轻微的颜色混合和柔化
            pixels[i] = Color.Lerp(pixels[i], new Color(0.5f, 0.5f, 0.5f), fabricSoftness * 0.1f);

            // 降低饱和度模拟布料吸收
            float intensity = (pixels[i].r + pixels[i].g + pixels[i].b) / 3f;
            pixels[i] = Color.Lerp(pixels[i], new Color(intensity, intensity, intensity), fabricSoftness * 0.2f);
        }
        finalTieDyeTexture.SetPixels(pixels);
    }

    // 在纹理上绘制单个花纹
    void DrawPatternOnTexture(PatternPlacement placement)
    {
        Texture2D lineTexture = GetLineTextureForPattern(placement.patternId);
        if (lineTexture == null)
        {
            Debug.LogWarning($"找不到花纹 {placement.patternId} 的扎染线条纹理");
            return;
        }

        Vector2 texturePos;
        Vector2 drawSize;

        if (usePreciseMapping)
        {
            texturePos = ConvertToPreciseTexturePosition(placement.position);
            drawSize = CalculatePreciseDrawSize(placement.scale, lineTexture);
        }
        else
        {
            texturePos = ConvertCanvasToTexturePosition(placement.position);
            drawSize = new Vector2(200f * placement.scale.x, 200f * placement.scale.y);
        }

        if (debugMode)
        {
            Debug.Log($"精确映射 - 位置: {texturePos}, 尺寸: {drawSize}");
        }

        DrawTextureWithRotation(finalTieDyeTexture, lineTexture, texturePos, drawSize, placement.rotation);
    }

    // 精确位置映射
    Vector2 ConvertToPreciseTexturePosition(Vector2 designPosition)
    {
        Vector2 canvasSize = loadedSessionData.canvasSize;
        float u = (designPosition.x + canvasSize.x / 2) / canvasSize.x;
        float v = (designPosition.y + canvasSize.y / 2) / canvasSize.y;
        u = Mathf.Clamp01(u);
        v = Mathf.Clamp01(v);
        return new Vector2(u * textureSize, v * textureSize);
    }

    // 精确尺寸计算
    Vector2 CalculatePreciseDrawSize(Vector2 designScale, Texture2D sourceTexture)
    {
        Vector2 canvasSize = loadedSessionData.canvasSize;
        float designPixelWidth = sourceTexture.width * designScale.x;
        float designPixelHeight = sourceTexture.height * designScale.y;
        float widthRatio = designPixelWidth / canvasSize.x;
        float heightRatio = designPixelHeight / canvasSize.y;
        float textureWidth = widthRatio * textureSize * patternScaleFactor;
        float textureHeight = heightRatio * textureSize * patternScaleFactor;
        return new Vector2(textureWidth, textureHeight);
    }

    // 旧的坐标转换方法
    Vector2 ConvertCanvasToTexturePosition(Vector2 canvasPosition)
    {
        float canvasWidth = 400f;
        float canvasHeight = 400f;
        float u = (canvasPosition.x + canvasWidth / 2) / canvasWidth;
        float v = (canvasPosition.y + canvasHeight / 2) / canvasHeight;
        u = Mathf.Clamp01(u);
        v = Mathf.Clamp01(v);
        return new Vector2(u * textureSize, v * textureSize);
    }

    // 获取花纹对应的扎染线条纹理
    Texture2D GetLineTextureForPattern(string patternId)
    {
        foreach (var patternLine in patternLineTextures)
        {
            if (patternLine.patternId == patternId && patternLine.lineTexture != null)
            {
                return patternLine.lineTexture;
            }
        }

        Debug.LogWarning($"为花纹 {patternId} 创建测试纹理");
        return CreateSimpleTestTexture();
    }

    // 创建简单的测试纹理
    Texture2D CreateSimpleTestTexture()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2, size / 2));
                float maxDistance = size / 2;
                float alpha = 1f - Mathf.Clamp01(distance / maxDistance);
                if (distance < 5f) alpha = 1f;
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }

        texture.Apply();
        return texture;
    }

    // 带旋转的纹理绘制
    void DrawTextureWithRotation(Texture2D target, Texture2D source, Vector2 position, Vector2 size, float rotation)
    {
        try
        {
            Color[] sourcePixels = source.GetPixels();
            int sourceWidth = source.width;
            int sourceHeight = source.height;

            Vector2 center = position;

            int startX = Mathf.Max(0, Mathf.RoundToInt(center.x - size.x / 2));
            int endX = Mathf.Min(target.width, Mathf.RoundToInt(center.x + size.x / 2));
            int startY = Mathf.Max(0, Mathf.RoundToInt(center.y - size.y / 2));
            int endY = Mathf.Min(target.height, Mathf.RoundToInt(center.y + size.y / 2));

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    Vector2 localPos = new Vector2(x - center.x, y - center.y);
                    Vector2 sourceUV = localPos;

                    if (Mathf.Abs(rotation) > 0.1f)
                    {
                        sourceUV = RotatePoint(localPos, -rotation);
                    }

                    float u = (sourceUV.x / size.x) + 0.5f;
                    float v = (sourceUV.y / size.y) + 0.5f;

                    if (u >= 0 && u <= 1 && v >= 0 && v <= 1)
                    {
                        int sourceX = Mathf.FloorToInt(u * sourceWidth);
                        int sourceY = Mathf.FloorToInt(v * sourceHeight);
                        sourceX = Mathf.Clamp(sourceX, 0, sourceWidth - 1);
                        sourceY = Mathf.Clamp(sourceY, 0, sourceHeight - 1);

                        Color sourceColor = sourcePixels[sourceY * sourceWidth + sourceX];

                        if (sourceColor.a > 0.1f)
                        {
                            Color currentColor = target.GetPixel(x, y);

                            // 增强布料感：根据布料吸收度调整混合
                            float blendFactor = sourceColor.a * (1f - dyeAbsorption * 0.3f);
                            Color newColor = Color.Lerp(currentColor, dyeColor, blendFactor);

                            // 添加纹理细节
                            newColor = ApplyDyeTextureDetail(newColor, x, y);

                            target.SetPixel(x, y, newColor);
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"绘制纹理时出错: {e.Message}");
        }
    }

    // 应用染料纹理细节
    Color ApplyDyeTextureDetail(Color dyeColor, int x, int y)
    {
        if (fabricTextureDetail > 0.01f)
        {
            // 添加微小的噪点模拟布料纤维
            float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.1f;
            dyeColor.r = Mathf.Clamp01(dyeColor.r + noise * fabricTextureDetail);
            dyeColor.g = Mathf.Clamp01(dyeColor.g + noise * fabricTextureDetail);
            dyeColor.b = Mathf.Clamp01(dyeColor.b + noise * fabricTextureDetail);
        }
        return dyeColor;
    }

    // 旋转点坐标
    Vector2 RotatePoint(Vector2 point, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            point.x * cos - point.y * sin,
            point.x * sin + point.y * cos
        );
    }

    // 应用生成的纹理到材质
    void ApplyToMaterial()
    {
        if (finalTieDyeTexture != null)
        {
            // 关键：设置纹理的包装模式
            finalTieDyeTexture.wrapMode = TextureWrapMode.Repeat;
            finalTieDyeTexture.filterMode = FilterMode.Bilinear;

            if (clothPhysicsManager != null)
            {
                clothPhysicsManager.ApplyTieDyeTexture(finalTieDyeTexture);
                EnsureMaterialSettings();
            }
            else if (tieDyeMaterial != null)
            {
                tieDyeMaterial.mainTexture = finalTieDyeTexture;
                tieDyeMaterial.mainTextureScale = Vector2.one;
                tieDyeMaterial.mainTextureOffset = Vector2.zero;

                // 应用布料专用设置
                ApplyFabricMaterialSettings(tieDyeMaterial);
                Debug.Log("纹理已应用到材质");
            }
            else
            {
                Debug.LogError("无法应用纹理：缺少布料管理器或材质引用");
            }
        }
        else
        {
            Debug.LogError("无法应用纹理：纹理为空");
        }
    }

    // 应用布料材质设置
    void ApplyFabricMaterialSettings(Material material)
    {
        // 设置布料质感参数
        material.SetFloat("_Glossiness", 1f - fabricRoughness); // 粗糙度
        material.SetFloat("_Metallic", 0f); // 无金属感

        // 应用法线贴图
        if (clothNormalMap != null)
        {
            material.SetTexture("_BumpMap", clothNormalMap);
            material.EnableKeyword("_NORMALMAP");
        }

        // 应用粗糙度贴图
        if (clothRoughnessMap != null)
        {
            material.SetTexture("_SpecGlossMap", clothRoughnessMap);
        }
    }

    // 确保材质设置正确
    void EnsureMaterialSettings()
    {
        if (clothPhysicsManager != null && clothPhysicsManager.clothModel != null)
        {
            MeshRenderer renderer = clothPhysicsManager.clothModel.GetComponent<MeshRenderer>();
            if (renderer != null && renderer.material != null)
            {
                Material mat = renderer.material;
                mat.mainTextureScale = Vector2.one;
                mat.mainTextureOffset = Vector2.zero;
                ApplyFabricMaterialSettings(mat);
            }
        }
    }

    // 公共方法：重新生成纹理
    public void RegenerateTexture()
    {
        GenerateFabricWeaveTexture();
        GenerateTieDyeTexture();
        ApplyToMaterial();
    }

    // 公共方法：调整布料质感参数
    public void SetFabricParameters(float softness, float roughness, float textureDetail)
    {
        fabricSoftness = softness;
        fabricRoughness = roughness;
        fabricTextureDetail = textureDetail;
        RegenerateTexture();
    }

    // 公共方法：调整颜色混合参数
    public void SetColorBlendParameters(float bleeding, float absorption)
    {
        colorBleeding = bleeding;
        dyeAbsorption = absorption;
        RegenerateTexture();
    }

    // 公共方法：调整缩放因子
    public void SetPatternScaleFactor(float factor)
    {
        patternScaleFactor = factor;
        RegenerateTexture();
    }

    // 公共方法：更改基础颜色
    public void ChangeBaseColor(Color newColor)
    {
        baseColor = newColor;
        RegenerateTexture();
    }

    // 公共方法：更改染料颜色
    public void ChangeDyeColor(Color newColor)
    {
        dyeColor = newColor;
        RegenerateTexture();
    }
}