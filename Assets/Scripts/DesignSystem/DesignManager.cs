using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DesignManager : MonoBehaviour
{
    [Header("UI����")]
    public Transform patternButtonsContainer; // PatternButtonsContainer
    public RectTransform designArea; // DesignArea - ��ΪRectTransform����
    public Button clearButton;
    public Button nextStepButton;

    [Header("����������")]
    public ToolbarManager toolbarManager;

    [Header("Ԥ������ز�")]
    public GameObject patternPrefab; // ����Ԥ����
    public List<PatternData> availablePatterns = new List<PatternData>();

    [Header("��ǰ״̬")]
    public DraggablePattern selectedPattern;
    public CanvasDesignData currentDesign = new CanvasDesignData();
    private bool isDesignLocked = false; // 设计是否被锁定

    void Start()
    {
        InitializeUI();
        LoadPatternButtons();
    }

    void Update()
    {
        // ��������������
        HandleMouseWheelInput();

        // �����������루ɾ������
        HandleKeyboardInput();
    }

    // ��ʼ��UI
    private void InitializeUI()
    {
        // �󶨰�ť�¼�
        if (clearButton != null)
            clearButton.onClick.AddListener(ClearDesign);

        if (nextStepButton != null)
            nextStepButton.onClick.AddListener(SaveAndProceed);
    }

    // ������������
    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            DeleteSelectedPattern();
        }
    }

    // ���ػ��ư�ť�����ֲ��䣩
    private void LoadPatternButtons()
    {
        if (patternButtonsContainer == null) return;

        // ������а�ť
        foreach (Transform child in patternButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        // Ϊÿ�����ƴ�����ť
        foreach (PatternData pattern in availablePatterns)
        {
            CreatePatternButton(pattern);
        }
    }

    // �������ư�ť�����ֲ��䣩
    private void CreatePatternButton(PatternData pattern)
    {
        // ������ť����
        GameObject buttonObj = new GameObject($"Btn_{pattern.patternId}");
        buttonObj.transform.SetParent(patternButtonsContainer, false);

        // ����UI���
        Image image = buttonObj.AddComponent<Image>();
        Button button = buttonObj.AddComponent<Button>();

        // ���ð�ť���
        image.sprite = pattern.patternSprite;
        image.preserveAspect = true;

        // ���ð�ť��С
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(80, 80);

        // �󶨵���¼�
        button.onClick.AddListener(() => OnPatternSelected(pattern));
    }

    // �����Ʊ�ѡ��ʱ����
    public void OnPatternSelected(PatternData patternData)
    {
        if (patternPrefab == null || designArea == null)
        {
            Debug.LogError("ȱ��Ԥ���������������ã�");
            return;
        }

        // ʵ�����»���
        GameObject newPatternObj = Instantiate(patternPrefab, designArea);
        DraggablePattern draggablePattern = newPatternObj.GetComponent<DraggablePattern>();

        if (draggablePattern != null)
        {
            draggablePattern.Initialize(patternData);

            // ���ó�ʼλ�ã��ڻ������ĸ������ƫ�ƣ�
            RectTransform rectTransform = newPatternObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(
                Random.Range(-100, 100),
                Random.Range(-100, 100)
            );

            // ѡ���´����Ļ���
            SelectPattern(draggablePattern);
        }
    }

    // ѡ���ƣ����°汾��
    public void SelectPattern(DraggablePattern pattern)
    {
        // ȡ��֮ǰ��ѡ��
        if (selectedPattern != null)
        {
            selectedPattern.SetSelected(false);
        }

        selectedPattern = pattern;

        // ������ѡ��
        if (selectedPattern != null)
        {
            selectedPattern.SetSelected(true);
            selectedPattern.transform.SetAsLastSibling();

            // ���¹�����״̬
            if (toolbarManager != null)
            {
                toolbarManager.UpdateToolbarState(true);
            }
        }
        else
        {
            // û��ѡ���κλ���ʱ���¹�����
            if (toolbarManager != null)
            {
                toolbarManager.UpdateToolbarState(false);
            }
        }
    }

    // ���õ�ǰģʽ
    public void SetCurrentMode(DraggablePattern.InteractionMode mode)
    {
        if (selectedPattern != null)
        {
            selectedPattern.SetInteractionMode(mode);
        }
    }

    // ɾ��ѡ�еĻ���
    public void DeleteSelectedPattern()
    {
        if (selectedPattern != null)
        {
            Destroy(selectedPattern.gameObject);
            SelectPattern(null); // ���ѡ��
        }
    }

    // ��������������
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

    // �����ƣ����°汾��
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
        SelectPattern(null); // ���ѡ��
    }

    // ���沢������һ�������ֲ��䣩
    public void SaveAndProceed()
    {
        // 锁定设计区花纹，使其不可再变动
        LockDesign();
        
        // 让clearButton缩放消失
        HideClearButton();

        // �ռ����л�������
        CollectDesignData();

        // ��������
        SaveDesignData();

        // ������һ��
        Debug.Log("��Ʊ�����ɣ�׼��������һ��...");
        Debug.Log($"�������� {currentDesign.placements.Count} ������");

        // ��ת����Ⱦ����
       // UnityEngine.SceneManagement.SceneManager.LoadScene("TieDyeScene");
    }
    
    // 锁定设计区花纹，使其不可再变动
    private void LockDesign()
    {
        isDesignLocked = true;
        
        if (designArea != null)
        {
            foreach (Transform child in designArea)
            {
                DraggablePattern pattern = child.GetComponent<DraggablePattern>();
                if (pattern != null)
                {
                    // 禁用交互
                    CanvasGroup canvasGroup = pattern.GetComponent<CanvasGroup>();
                    if (canvasGroup != null)
                    {
                        canvasGroup.blocksRaycasts = false;
                    }
                    
                    // 取消选中状态
                    pattern.SetSelected(false);
                }
            }
        }
        
        // 取消当前选中的花纹
        SelectPattern(null);
    }
    
    // 隐藏ClearButton通过缩放动画
    private void HideClearButton()
    {
        if (clearButton != null)
        {
            RectTransform clearButtonRect = clearButton.GetComponent<RectTransform>();
            if (clearButtonRect != null)
            {
                // 使用DOTween实现缩放消失动画
                clearButtonRect.DOScale(Vector3.zero, 0.3f).OnComplete(() => {
                    clearButton.gameObject.SetActive(false);
                });
            }
        }
    }

    // �ռ�������ݣ����ֲ��䣩
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
        // �ռ����������Ϣ
        Vector2 designAreaSize = designArea.rect.size;
        Vector2 designAreaPosition = designArea.anchoredPosition;

        // �����������������Ϣ�����ݽṹ
        DesignSessionData sessionData = new DesignSessionData
        {
            designData = currentDesign,
            canvasSize = designAreaSize,
            canvasPosition = designAreaPosition
        };

        string jsonData = JsonUtility.ToJson(sessionData);
        PlayerPrefs.SetString("CurrentDesign", jsonData);
        PlayerPrefs.Save();

        Debug.Log("��������ѱ��棬�������ߴ�: " + designAreaSize);
    }

    // ������ƻỰ������
    [System.Serializable]
    public class DesignSessionData
    {
        public CanvasDesignData designData;
        public Vector2 canvasSize;      // �������ߴ�
        public Vector2 canvasPosition;  // �������λ��
    }
}