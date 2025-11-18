using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DesignManager : MonoBehaviour
{
    [Header("UI引用")]
    public Transform patternButtonsContainer; // PatternButtonsContainer
    public RectTransform designArea; // DesignArea - 改为RectTransform类型
    public Button clearButton;
    public Button nextStepButton;

    [Header("工具栏引用")]
    public ToolbarManager toolbarManager;

    [Header("预制体和素材")]
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

        // 处理键盘输入（删除键）
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

    // 加载花纹按钮（保持不变）
    private void LoadPatternButtons()
    {
        if (patternButtonsContainer == null) return;

        // 清空现有按钮
        foreach (Transform child in patternButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        // 为每个花纹创建按钮
        foreach (PatternData pattern in availablePatterns)
        {
            CreatePatternButton(pattern);
        }
    }

    // 创建花纹按钮（保持不变）
    private void CreatePatternButton(PatternData pattern)
    {
        // 创建按钮对象
        GameObject buttonObj = new GameObject($"Btn_{pattern.patternId}");
        buttonObj.transform.SetParent(patternButtonsContainer, false);

        // 添加UI组件
        Image image = buttonObj.AddComponent<Image>();
        Button button = buttonObj.AddComponent<Button>();

        // 设置按钮外观
        image.sprite = pattern.patternSprite;
        image.preserveAspect = true;

        // 设置按钮大小
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(80, 80);

        // 绑定点击事件
        button.onClick.AddListener(() => OnPatternSelected(pattern));
    }

    // 当花纹被选择时调用
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

            // 设置初始位置（在画布中心附近随机偏移）
            RectTransform rectTransform = newPatternObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(
                Random.Range(-100, 100),
                Random.Range(-100, 100)
            );

            // 选中新创建的花纹
            SelectPattern(draggablePattern);
        }
    }

    // 选择花纹（更新版本）
    public void SelectPattern(DraggablePattern pattern)
    {
        // 取消之前的选择
        if (selectedPattern != null)
        {
            selectedPattern.SetSelected(false);
        }

        selectedPattern = pattern;

        // 设置新选择
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

    // 设置当前模式
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
            SelectPattern(null); // 清除选择
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

    // 清除设计（更新版本）
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
        SelectPattern(null); // 清除选择
    }

    // 保存并进入下一步（保持不变）
    public void SaveAndProceed()
    {
        // 收集所有花纹数据
        CollectDesignData();

        // 保存数据
        SaveDesignData();

        // 进入下一步
        Debug.Log("设计保存完成，准备进入下一步...");
        Debug.Log($"共保存了 {currentDesign.placements.Count} 个花纹");

        // 跳转到扎染场景
       // UnityEngine.SceneManagement.SceneManager.LoadScene("TieDyeScene");
    }

    // 收集设计数据（保持不变）
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

    private void SaveDesignData()
    {
        // 收集设计区域信息
        Vector2 designAreaSize = designArea.rect.size;
        Vector2 designAreaPosition = designArea.anchoredPosition;

        // 创建包含设计区域信息的数据结构
        DesignSessionData sessionData = new DesignSessionData
        {
            designData = currentDesign,
            canvasSize = designAreaSize,
            canvasPosition = designAreaPosition
        };

        string jsonData = JsonUtility.ToJson(sessionData);
        PlayerPrefs.SetString("CurrentDesign", jsonData);
        PlayerPrefs.Save();

        Debug.Log("设计数据已保存，设计区域尺寸: " + designAreaSize);
    }

    // 新增设计会话数据类
    [System.Serializable]
    public class DesignSessionData
    {
        public CanvasDesignData designData;
        public Vector2 canvasSize;      // 设计区域尺寸
        public Vector2 canvasPosition;  // 设计区域位置
    }
}