using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CanvasDesignData
{
    public string designName = "未命名设计";
    public DateTime creationTime;
    public List<PatternPlacement> placements = new List<PatternPlacement>();

    // 设计画布信息
    public Vector2 canvasSize = new Vector2(800, 600);

    public CanvasDesignData()
    {
        creationTime = DateTime.Now;
    }

    // 深拷贝方法
    public CanvasDesignData Clone()
    {
        CanvasDesignData clone = new CanvasDesignData();
        clone.designName = this.designName;
        clone.creationTime = this.creationTime;
        clone.canvasSize = this.canvasSize;

        foreach (var placement in this.placements)
        {
            clone.placements.Add(placement.Clone());
        }

        return clone;
    }
}

[Serializable]
public class PatternPlacement
{
    public string patternId;
    public Vector2 position;
    public Vector2 scale = Vector2.one;
    public float rotation;

    // 辅助方法：深拷贝
    public PatternPlacement Clone()
    {
        return new PatternPlacement
        {
            patternId = this.patternId,
            position = this.position,
            scale = this.scale,
            rotation = this.rotation
        };
    }

    // 转换为矩阵（用于Shader计算）
    public Matrix4x4 GetTransformationMatrix()
    {
        return Matrix4x4.TRS(
            new Vector3(position.x, position.y, 0),
            Quaternion.Euler(0, 0, rotation),
            new Vector3(scale.x, scale.y, 1)
        );
    }
}

// 设计保存管理器
public static class DesignSaveManager
{
    private const string DESIGN_SAVE_KEY = "CurrentDesign";
    private const string DESIGN_HISTORY_KEY = "DesignHistory";
    private const int MAX_HISTORY_COUNT = 10;

    // 保存当前设计
    public static void SaveDesign(CanvasDesignData designData)
    {
        string jsonData = JsonUtility.ToJson(designData);
        PlayerPrefs.SetString(DESIGN_SAVE_KEY, jsonData);
        PlayerPrefs.Save();

        Debug.Log($"设计已保存: {designData.designName}, 包含 {designData.placements.Count} 个花纹");
    }

    // 加载当前设计
    public static CanvasDesignData LoadDesign()
    {
        if (PlayerPrefs.HasKey(DESIGN_SAVE_KEY))
        {
            string jsonData = PlayerPrefs.GetString(DESIGN_SAVE_KEY);
            CanvasDesignData designData = JsonUtility.FromJson<CanvasDesignData>(jsonData);
            Debug.Log($"设计已加载: {designData.designName}");
            return designData;
        }

        Debug.Log("没有找到保存的设计，返回新建设计");
        return new CanvasDesignData();
    }

    // 保存到历史记录
    public static void SaveToHistory(CanvasDesignData designData)
    {
        List<CanvasDesignData> history = LoadHistory();

        // 添加到历史开头
        history.Insert(0, designData.Clone());

        // 限制历史记录数量
        if (history.Count > MAX_HISTORY_COUNT)
        {
            history = history.GetRange(0, MAX_HISTORY_COUNT);
        }

        // 保存历史记录
        string historyJson = JsonUtility.ToJson(new DesignHistoryWrapper { designs = history });
        PlayerPrefs.SetString(DESIGN_HISTORY_KEY, historyJson);
        PlayerPrefs.Save();
    }

    // 加载历史记录
    public static List<CanvasDesignData> LoadHistory()
    {
        if (PlayerPrefs.HasKey(DESIGN_HISTORY_KEY))
        {
            string historyJson = PlayerPrefs.GetString(DESIGN_HISTORY_KEY);
            DesignHistoryWrapper wrapper = JsonUtility.FromJson<DesignHistoryWrapper>(historyJson);
            return wrapper.designs;
        }

        return new List<CanvasDesignData>();
    }

    // 清除当前设计
    public static void ClearCurrentDesign()
    {
        PlayerPrefs.DeleteKey(DESIGN_SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("当前设计已清除");
    }
}

// 包装类用于序列化列表
[Serializable]
public class DesignHistoryWrapper
{
    public List<CanvasDesignData> designs = new List<CanvasDesignData>();
}