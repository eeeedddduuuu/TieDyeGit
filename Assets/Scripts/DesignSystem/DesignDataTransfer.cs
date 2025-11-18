using UnityEngine;

public static class DesignDataTransfer
{
    // 当前设计数据（场景间传递）
    public static CanvasDesignData CurrentDesignData { get; set; }

    // 设计画布尺寸
    public static Vector2 CanvasSize { get; set; } = new Vector2(800, 600);

    // 是否从保存数据加载
    public static bool ShouldLoadFromSave { get; set; } = false;

    // 传递设计数据到下一个场景
    public static void SetDesignDataForNextScene(CanvasDesignData designData)
    {
        CurrentDesignData = designData.Clone();
        ShouldLoadFromSave = false;
        Debug.Log($"设计数据已准备传递到下一个场景: {designData.placements.Count} 个花纹");
    }

    // 从保存数据加载
    public static void LoadFromSavedData()
    {
        CurrentDesignData = DesignSaveManager.LoadDesign();
        ShouldLoadFromSave = true;
    }

    // 清除传递的数据
    public static void ClearTransferData()
    {
        CurrentDesignData = null;
        ShouldLoadFromSave = false;
    }
}