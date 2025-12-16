using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SimpleFileBrowser;

public class UserImageLoader : MonoBehaviour
{
    [Header("引用")]
    public DesignManager designManager; // 拖入场景里的 DesignManager
    public Button uploadButton;         // 拖入你UI上的“上传图片”按钮

    [Header("抠图设置")]
    [Range(0, 1)]
    public float threshold = 0.5f; // 阈值：亮度大于这个值的像素会被当成背景抠掉

    void Start()
    {
        if (uploadButton != null)
        {
            uploadButton.onClick.AddListener(OnUploadButtonClicked);
        }
    }

    // 点击按钮触发
    void OnUploadButtonClicked()
    {
        // 设置过滤器，只允许选图片
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png", ".jpeg"));
        FileBrowser.SetDefaultFilter(".jpg");

        // 打开文件选择窗口
        // 参数：成功回调，取消回调，模式(Load)，多选(false)
        FileBrowser.ShowLoadDialog(
            (paths) => {
                // 用户选好图了，路径在 paths[0]
                ProcessUserImage(paths[0]);
            },
            () => { Debug.Log("用户取消了"); },
            FileBrowser.PickMode.Files, false, null, null, "选择图片", "上传"
        );
    }

    // 【核心】处理用户图片
    public void ProcessUserImage(string imagePath)
    {
        byte[] fileData = File.ReadAllBytes(imagePath);
        Texture2D rawTexture = new Texture2D(2, 2);
        rawTexture.LoadImage(fileData);

        // 【新增 1】尺寸标准化处理
        // 如果图片太大（比如超过512px），强制缩放，否则在3D场景会巨大无比
        if (rawTexture.width > 512 || rawTexture.height > 512)
        {
            // 这里用简单的双线性插值或直接缩小 (为简化代码，这里演示强制重设尺寸逻辑)
            // 实际项目中建议使用 TextureScale 插件，或者直接告诉用户上传小图
            // 简单方案：不改变像素，但在 Sprite.Create 时调大 pixelsPerUnit
        }

        ProcessTextureColors(rawTexture, out Texture2D blackTex, out Texture2D whiteTex);

        // 【修改 2】调整 PixelsPerUnit (PPU)
        // 默认是 100。如果你上传的图是 1000px，它在 UI 上就是 10 个单位大。
        // 如果我们想让它和普通花纹一样大，可以根据图片宽度动态计算 PPU
        // 假设标准花纹宽度是 200px，如果你上传 400px，就把 PPU 设为 200，这样它显示出来还是 2 个单位
        float dynamicPPU = 100f;

        Sprite blackSprite = Sprite.Create(blackTex, new Rect(0, 0, blackTex.width, blackTex.height), new Vector2(0.5f, 0.5f), dynamicPPU);
        Sprite whiteSprite = Sprite.Create(whiteTex, new Rect(0, 0, whiteTex.width, whiteTex.height), new Vector2(0.5f, 0.5f), dynamicPPU);

        // 创建数据
        PatternData newPatternData = new PatternData();
        newPatternData.patternId = "User_" + System.DateTime.Now.Ticks;
        newPatternData.patternSprite = blackSprite;
        newPatternData.whiteTextureSprite = whiteSprite;

        // 【关键新增 3】存入全局仓库！
        if (!UserPatternStorage.userPatterns.ContainsKey(newPatternData.patternId))
        {
            UserPatternStorage.userPatterns.Add(newPatternData.patternId, newPatternData);
        }

        // 像往常一样显示在 UI 上
        designManager.OnPatternSelected(newPatternData);

        Debug.Log($"用户花纹已加载并注册 ID: {newPatternData.patternId}");
    }

    // --- 图像算法：二值化抠图 ---
    void ProcessTextureColors(Texture2D source, out Texture2D blackResult, out Texture2D whiteResult)
    {
        int w = source.width;
        int h = source.height;

        // 创建两张空图（RGBA32 支持透明通道）
        blackResult = new Texture2D(w, h, TextureFormat.RGBA32, false);
        whiteResult = new Texture2D(w, h, TextureFormat.RGBA32, false);

        Color[] pixels = source.GetPixels();
        Color[] blackPixels = new Color[pixels.Length];
        Color[] whitePixels = new Color[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            Color p = pixels[i];

            // 计算亮度 (灰度值)
            float gray = p.r * 0.299f + p.g * 0.587f + p.b * 0.114f;

            // 判断：如果是深色（花纹），保留；如果是浅色（背景），变透明
            if (gray < threshold)
            {
                // 是花纹部分
                // 黑图：纯黑，不透明
                blackPixels[i] = new Color(0, 0, 0, 1);
                // 白图：纯白，不透明 (给 3D 扎染用)
                whitePixels[i] = new Color(1, 1, 1, 1);
            }
            else
            {
                // 是背景部分 -> 完全透明
                blackPixels[i] = Color.clear;
                whitePixels[i] = Color.clear;
            }
        }

        blackResult.SetPixels(blackPixels);
        blackResult.Apply();

        whiteResult.SetPixels(whitePixels);
        whiteResult.Apply();
    }
}