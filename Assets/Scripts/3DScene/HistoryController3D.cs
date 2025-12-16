using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class HistoryController3D : MonoBehaviour
{
    [Header("核心组件")]
    public PatternTextureBaker baker;     // 烘焙器
    public GameObject historyPanel;       // 整个历史记录UI面板(用来开关)
    public Transform contentContainer;    // ScrollView的Content物体
    public GameObject historyItemPrefab;  // 列表项的预制体(下面会做)

    [Header("UI引用")]
    public Button openHistoryBtn;         // 打开历史的按钮
    public Button closeHistoryBtn;        // 关闭历史的按钮
    public Button saveCurrentBtn;         // 保存当前状态的按钮

    void Start()
    {
        // 绑定按钮事件
        if (openHistoryBtn) openHistoryBtn.onClick.AddListener(ShowHistory);
        if (closeHistoryBtn) closeHistoryBtn.onClick.AddListener(HideHistory);
        if (saveCurrentBtn) saveCurrentBtn.onClick.AddListener(SaveCurrentToHistory);

        // 默认隐藏面板
        if (historyPanel) historyPanel.SetActive(false);
    }

    // --- 1. 保存当前状态到历史 ---
    public void SaveCurrentToHistory()
    {
        // 获取当前正在显示的数据
        CanvasDesignData currentData = DesignDataTransfer.CurrentDesignData;

        if (currentData != null)
        {
            // 更新一下当前的时间
            currentData.creationTime = DateTime.Now;
            // 更新当前的颜色 (从Baker获取)
            currentData.savedBackgroundColor = baker.tieDyeBackgroundColor;

            // 保存到硬盘历史记录
            DesignSaveManager.SaveToHistory(currentData);

            Debug.Log("已保存当前设计到历史记录！");

            // 如果面板开着，刷新一下列表
            if (historyPanel.activeSelf) RefreshList();
        }
    }

    // --- 2. 显示历史列表 ---
    void ShowHistory()
    {
        historyPanel.SetActive(true);
        RefreshList();
    }

    void HideHistory()
    {
        historyPanel.SetActive(false);
    }

    // --- 3. 刷新列表逻辑 ---
    void RefreshList()
    {
        // 清空旧按钮
        foreach (Transform child in contentContainer) Destroy(child.gameObject);

        // 加载历史数据
        List<CanvasDesignData> history = DesignSaveManager.LoadHistory();

        // 生成新按钮
        foreach (var data in history)
        {
            GameObject btnObj = Instantiate(historyItemPrefab, contentContainer);

            // 设置文字 (显示时间)
            Text btnText = btnObj.GetComponentInChildren<Text>();
            if (btnText) btnText.text = data.creationTime.ToString("MM-dd HH:mm:ss");

            // 绑定点击事件
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnHistoryItemSelected(data));
        }
    }

    // --- 4. 核心：点击历史记录后的回放逻辑 ---
    void OnHistoryItemSelected(CanvasDesignData oldData)
    {
        Debug.Log("正在恢复历史记录...");

        // 1. 更新全局数据 (这一步保留，为了以防万一)
        DesignDataTransfer.CurrentDesignData = oldData;

        // 2. 【关键修改】调用 Baker 的一键恢复接口
        // 这一行代码会同时搞定：背景色、清理旧花纹、生成新花纹、拍照
        baker.ApplyHistoryData(oldData);
    }
}