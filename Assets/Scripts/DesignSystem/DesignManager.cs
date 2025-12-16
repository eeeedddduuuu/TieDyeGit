using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DesignManager : MonoBehaviour
{
    [Header("UI设置")]
    public Transform patternButtonsContainer; // PatternButtonsContainer
    public RectTransform designArea; // DesignArea - 作为RectTransform引用
    public Button clearButton;
    public Button nextStepButton;

    [Header("工具栏管理")]
    public ToolbarManager toolbarManager;

    [Header("预制体与资源")]
    public GameObject patternPrefab; // 花纹预制体
    public List<PatternData> availablePatterns = new List<PatternData>();

    [Header("当前状态")]
    public DraggablePattern selectedPattern;
    public CanvasDesignData currentDesign = new CanvasDesignData();

    void Start()
    {
        InitializeUI();
        LoadPatternButtons();
    }

    void Update()
    {
        // 处理鼠标滚轮输入
        HandleMouseWheelInput();

        // 处理键盘输入（如删除）
        HandleKeyboardInput();
    }

    // 初始化UI
    private void InitializeUI()
    {
        // 绑定按钮事件
        if (clearButton != null)
            clearButton.onClick.AddListener(ClearDesign);

        if (nextStepButton != null)
            nextStepButton.onClick.AddListener(SaveAndProceed);
    }

    // 处理键盘输入
    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            DeleteSelectedPattern();
        }
    }

    // 加载花纹按钮
    private void LoadPatternButtons()
    {
        if (patternButtonsContainer == null) return;

        // 清除现有按钮
        foreach (Transform child in patternButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        // 为每个花纹数据创建按钮
        foreach (PatternData pattern in availablePatterns)
        {
            CreatePatternButton(pattern);
        }
    }

    // 创建单个花纹按钮
    private void CreatePatternButton(PatternData pattern)
    {
        // 创建按钮对象
        GameObject buttonObj = new GameObject($"Btn_{pattern.patternId}");
        buttonObj.transform.SetParent(patternButtonsContainer, false);

        // 添加UI组件
        Image image = buttonObj.AddComponent<Image>();
        Button button = buttonObj.AddComponent<Button>();

        // 设置按钮图标
        image.sprite = pattern.patternSprite;
        image.preserveAspect = true;

        // 设置按钮大小
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(80, 80);

        // 绑定点击事件
        button.onClick.AddListener(() => OnPatternSelected(pattern));
    }

    // 当花纹被选中时调用
    public void OnPatternSelected(PatternData patternData)
    {
        if (patternPrefab == null || designArea == null)
        {
            Debug.LogError("缺少预制体或设计区域引用！");
            return;
        }

        // 实例化新花纹
        GameObject newPatternObj = Instantiate(patternPrefab, designArea);
        DraggablePattern draggablePattern = newPatternObj.GetComponent<DraggablePattern>();

        if (draggablePattern != null)
        {
            draggablePattern.Initialize(patternData);

            // 设置初始位置（在画布中心的随机偏移）
            RectTransform rectTransform = newPatternObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(
                Random.Range(-100, 100),
                Random.Range(-100, 100)
            );

            // 选中新创建的花纹
            SelectPattern(draggablePattern);
        }
    }

    // 选中花纹
    public void SelectPattern(DraggablePattern pattern)
    {
        // 取消之前的选中
        if (selectedPattern != null)
        {
            selectedPattern.SetSelected(false);
        }

        selectedPattern = pattern;

        // 设置新选中
        if (selectedPattern != null)
        {
            selectedPattern.SetSelected(true);
            selectedPattern.transform.SetAsLastSibling();

            // 更新工具栏状态
            if (toolbarManager != null)
            {
                toolbarManager.UpdateToolbarState(true);
            }
        }
        else
        {
            // 没有选中任何花纹时更新工具栏
            if (toolbarManager != null)
            {
                toolbarManager.UpdateToolbarState(false);
            }
        }
    }

    // 设置当前交互模式
    public void SetCurrentMode(DraggablePattern.InteractionMode mode)
    {
        if (selectedPattern != null)
        {
            selectedPattern.SetInteractionMode(mode);
        }
    }

    // 删除选中的花纹
    public void DeleteSelectedPattern()
    {
        if (selectedPattern != null)
        {
            Destroy(selectedPattern.gameObject);
            SelectPattern(null); // 清空选中
        }
    }

    // 处理鼠标滚轮输入
    private void HandleMouseWheelInput()
    {
        if (selectedPattern == null) return;

        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollDelta) > 0.01f)
        {
            if (selectedPattern.currentMode == DraggablePattern.InteractionMode.Scale)
            {
                selectedPattern.ScalePattern(scrollDelta);
            }
            else if (selectedPattern.currentMode == DraggablePattern.InteractionMode.Rotate)
            {
                selectedPattern.RotatePattern(scrollDelta);
            }
        }
    }

    // 清空设计
    public void ClearDesign()
    {
        if (designArea == null) return;

        foreach (Transform child in designArea)
        {
            DraggablePattern pattern = child.GetComponent<DraggablePattern>();
            if (pattern != null)
            {
                Destroy(child.gameObject);
            }
        }

        currentDesign.placements.Clear();
        SelectPattern(null); // 清空选中
    }

    public void SaveAndProceed()
    {
        // 1. 先从 UI 里的 DraggablePattern 收集数据到 currentDesign
        CollectDesignData();

        // 2. 然后保存 (使用我们上面修改过的新方法)
        SaveDesignData();

        // 3. 最后跳转场景
        Debug.Log("设计数据保存成功");

    }

    // 收集设计数据
    private void CollectDesignData()
    {
        currentDesign.placements.Clear();

        foreach (Transform child in designArea)
        {
            DraggablePattern pattern = child.GetComponent<DraggablePattern>();
            if (pattern != null)
            {
                currentDesign.placements.Add(pattern.GetPlacementData());
            }
        }
    }

    // 修改 DesignManager.cs 中的这个方法
    private void SaveDesignData()
    {
        // 1. 确保将画布的尺寸更新到设计数据中
        // (之前是保存在 sessionData 外层，现在要存进核心数据里)
        if (designArea != null)
        {
            currentDesign.canvasSize = designArea.rect.size;
        }

        // 2. 使用 DesignSaveManager 进行标准保存
        // 这样存入 PlayerPrefs 的就是纯净的 CanvasDesignData JSON，而不是 SessionData 包装壳
        DesignSaveManager.SaveDesign(currentDesign);

        // 3. 【关键】同时更新内存传输数据
        // 这样场景跳转时可以直接从内存读，不需要读硬盘，更快且更稳
        DesignDataTransfer.SetDesignDataForNextScene(currentDesign);

        Debug.Log($"设计已保存并准备传输! 画布尺寸: {currentDesign.canvasSize}, 花纹数量: {currentDesign.placements.Count}");
    }

    // 设计会话数据类
    [System.Serializable]
    public class DesignSessionData
    {
        public CanvasDesignData designData;
        public Vector2 canvasSize;      // 画布尺寸
        public Vector2 canvasPosition;  // 画布位置
    }
}