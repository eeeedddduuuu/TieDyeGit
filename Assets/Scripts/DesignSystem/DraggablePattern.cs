using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggablePattern : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("组件引用")]
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Image image;

    [Header("拖动设置")]
    public bool isDragging = false;
    private Vector2 dragOffset;

    [Header("缩放设置")]
    public float minScale = 0.5f;
    public float maxScale = 3f;
    public float scaleSpeed = 0.1f;
    private Vector3 originalScale;

    [Header("旋转设置")]
    public float rotationSpeed = 2f;

    [Header("模式控制")]
    public InteractionMode currentMode = InteractionMode.Drag;

    public enum InteractionMode
    {
        Drag,
        Scale,
        Rotate
    }

    // 花纹数据
    public PatternData patternData;

    // 选中视觉效果
    private Outline selectionOutline;
    private bool isSelected = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 创建选中轮廓
        CreateSelectionOutline();
    }

    // 创建选中轮廓（更轻量）
    private void CreateSelectionOutline()
    {
        // 直接在花纹上添加Outline组件，但默认禁用
        selectionOutline = gameObject.AddComponent<Outline>();
        selectionOutline.effectColor = new Color(1, 0.92f, 0.016f, 1f); // 金黄色
        selectionOutline.effectDistance = new Vector2(3, -3);
        selectionOutline.enabled = false;
    }

    // 初始化花纹
    public void Initialize(PatternData data)
    {
        patternData = data;
        if (image != null && data.patternSprite != null)
        {
            image.sprite = data.patternSprite;
            image.SetNativeSize();
        }

        // 设置默认大小并保存原始尺寸
        originalScale = Vector3.one * 0.8f;
        rectTransform.localScale = originalScale;
    }

    // 开始拖动
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentMode != InteractionMode.Drag) return;

        isDragging = true;
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out dragOffset
        );

        dragOffset = rectTransform.anchoredPosition - dragOffset;

        // 置顶显示
        rectTransform.SetAsLastSibling();
    }

    // 拖动中
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || currentMode != InteractionMode.Drag) return;

        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition))
        {
            rectTransform.anchoredPosition = localPointerPosition + dragOffset;
        }
    }

    // 结束拖动
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    // 点击处理
    public void OnPointerClick(PointerEventData eventData)
    {
        // 选中这个花纹
        SelectThisPattern();
    }

    // 选中这个花纹
    public void SelectThisPattern()
    {
        // 通知DesignManager选中了这个花纹
        DesignManager designManager = FindObjectOfType<DesignManager>();
        if (designManager != null)
        {
            designManager.SelectPattern(this);
        }

        // 置顶显示
        rectTransform.SetAsLastSibling();
    }

    // 设置选中状态
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        // 使用轮廓效果而不是黄色方块
        if (selectionOutline != null)
        {
            selectionOutline.enabled = selected;
        }

        // 轻微的尺寸变化作为视觉反馈
        if (selected)
        {
            rectTransform.localScale = originalScale * 1.02f;
        }
        else
        {
            rectTransform.localScale = originalScale;
        }
    }

    // 设置交互模式
    public void SetInteractionMode(InteractionMode mode)
    {
        currentMode = mode;
    }

    // 缩放花纹
    public void ScalePattern(float scaleDelta)
    {
        if (currentMode != InteractionMode.Scale) return;

        Vector3 newScale = rectTransform.localScale + Vector3.one * scaleDelta * scaleSpeed;
        newScale = Vector3.Max(Vector3.one * minScale, Vector3.Min(Vector3.one * maxScale, newScale));
        rectTransform.localScale = newScale;

        // 更新原始尺寸参考
        if (isSelected)
        {
            originalScale = newScale / 1.02f;
        }
        else
        {
            originalScale = newScale;
        }
    }

    // 旋转花纹
    public void RotatePattern(float rotationDelta)
    {
        if (currentMode != InteractionMode.Rotate) return;

        Vector3 newRotation = rectTransform.localEulerAngles;
        newRotation.z += rotationDelta * rotationSpeed;
        rectTransform.localEulerAngles = newRotation;
    }

    // 获取花纹数据（用于保存）
    public PatternPlacement GetPlacementData()
    {
        return new PatternPlacement
        {
            patternId = patternData?.patternId ?? "",
            position = rectTransform.anchoredPosition,
            scale = rectTransform.localScale,
            rotation = rectTransform.localEulerAngles.z
        };
    }
}