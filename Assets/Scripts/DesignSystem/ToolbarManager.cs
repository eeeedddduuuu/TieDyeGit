using UnityEngine;
using UnityEngine.UI;

public class ToolbarManager : MonoBehaviour
{
    [Header("工具栏按钮")]
    public Button moveButton;
    public Button rotateButton;
    public Button scaleButton;
    public Button deleteButton;

    [Header("按钮颜色")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.green;
    public Color disabledColor = Color.gray;

    private DesignManager designManager;

    void Start()
    {
        designManager = FindObjectOfType<DesignManager>();

        // 绑定按钮事件
        if (moveButton != null)
            moveButton.onClick.AddListener(OnMoveButtonClick);

        if (rotateButton != null)
            rotateButton.onClick.AddListener(OnRotateButtonClick);

        if (scaleButton != null)
            scaleButton.onClick.AddListener(OnScaleButtonClick);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteButtonClick);

        // 初始状态
        UpdateToolbarState(false);
    }

    // 移动按钮点击
    private void OnMoveButtonClick()
    {
        if (designManager != null && designManager.selectedPattern != null)
        {
            designManager.SetCurrentMode(DraggablePattern.InteractionMode.Drag);
            UpdateButtonColors(DraggablePattern.InteractionMode.Drag);
        }
    }

    // 旋转按钮点击
    private void OnRotateButtonClick()
    {
        if (designManager != null && designManager.selectedPattern != null)
        {
            designManager.SetCurrentMode(DraggablePattern.InteractionMode.Rotate);
            UpdateButtonColors(DraggablePattern.InteractionMode.Rotate);
        }
    }

    // 缩放按钮点击
    private void OnScaleButtonClick()
    {
        if (designManager != null && designManager.selectedPattern != null)
        {
            designManager.SetCurrentMode(DraggablePattern.InteractionMode.Scale);
            UpdateButtonColors(DraggablePattern.InteractionMode.Scale);
        }
    }

    // 删除按钮点击
    private void OnDeleteButtonClick()
    {
        if (designManager != null)
        {
            designManager.DeleteSelectedPattern();
        }
    }

    // 更新按钮颜色
    private void UpdateButtonColors(DraggablePattern.InteractionMode mode)
    {
        // 重置所有按钮颜色
        moveButton.image.color = normalColor;
        rotateButton.image.color = normalColor;
        scaleButton.image.color = normalColor;

        // 设置选中按钮颜色
        switch (mode)
        {
            case DraggablePattern.InteractionMode.Drag:
                moveButton.image.color = selectedColor;
                break;
            case DraggablePattern.InteractionMode.Rotate:
                rotateButton.image.color = selectedColor;
                break;
            case DraggablePattern.InteractionMode.Scale:
                scaleButton.image.color = selectedColor;
                break;
        }
    }

    // 更新工具栏状态
    public void UpdateToolbarState(bool hasSelection)
    {
        // 设置按钮交互状态
        moveButton.interactable = hasSelection;
        rotateButton.interactable = hasSelection;
        scaleButton.interactable = hasSelection;
        deleteButton.interactable = hasSelection;

        if (!hasSelection)
        {
            // 没有选中时重置按钮颜色
            moveButton.image.color = disabledColor;
            rotateButton.image.color = disabledColor;
            scaleButton.image.color = disabledColor;
            deleteButton.image.color = disabledColor;
        }
        else
        {
            // 有选中时根据当前模式设置颜色
            if (designManager != null && designManager.selectedPattern != null)
            {
                UpdateButtonColors(designManager.selectedPattern.currentMode);
            }
        }
    }
}