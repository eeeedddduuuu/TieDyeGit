using UnityEngine;
using UnityEngine.EventSystems;

public class TransformGizmoController : MonoBehaviour
{
    [Header("目标引用")]
    public RectTransform targetPattern; // 当前选中的花纹
    public Canvas parentCanvas;

    [Header("UI组件")]
    public RectTransform borderRect;

    // 内部变量
    private Vector2 initialMousePos;
    private Vector3 initialScale;
    private float initialRotation;
    private float initialAngle;
    private float initialDistance;

    void Start()
    {
        // 初始时隐藏
        gameObject.SetActive(false);
        if (parentCanvas == null) parentCanvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        // 每一帧都让框框跟随目标花纹的位置、大小、旋转
        if (targetPattern != null)
        {
            FollowTarget();
        }
    }

    // 供 DesignManager 调用：设置选中目标
    public void SetTarget(DraggablePattern pattern)
    {
        if (pattern == null)
        {
            targetPattern = null;
            gameObject.SetActive(false);
        }
        else
        {
            targetPattern = pattern.GetComponent<RectTransform>();
            gameObject.SetActive(true);
            // 确保Gizmo在最上层
            transform.SetAsLastSibling();
        }
    }

    // 跟随逻辑：让Gizmo在视觉上完全覆盖目标
    // 修改 TransformGizmoController.cs 中的这个方法
    void FollowTarget()
    {
        // 1. 位置和旋转直接同步
        transform.position = targetPattern.position;
        transform.rotation = targetPattern.rotation;

        // 2. 【关键优化】智能尺寸计算
        // 我们不直接复制 scale，因为那样会导致子物体(手柄)也跟着缩放变大。
        // 相反，我们将 Gizmo 的 Scale 锁定为 1，
        // 然后把目标的 Scale 乘到 Gizmo 的 SizeDelta (宽高) 上。

        RectTransform gizmoRect = GetComponent<RectTransform>();

        // 锁定缩放为 1 (这样手柄就不会变形了)
        gizmoRect.localScale = Vector3.one;

        // 计算目标在视觉上的真实宽高
        // 真实宽 = 原始宽 * 缩放X
        float realWidth = targetPattern.rect.width * targetPattern.localScale.x;
        float realHeight = targetPattern.rect.height * targetPattern.localScale.y;

        // 应用到 Gizmo 的尺寸上
        gizmoRect.sizeDelta = new Vector2(realWidth, realHeight);
    }

    // --- 手柄拖拽逻辑 ---

    public void OnHandleBeginDrag(GizmoHandle.HandleType type)
    {
        if (targetPattern == null) return;

        if (type == GizmoHandle.HandleType.Scale)
        {
            initialScale = targetPattern.localScale;
            // 记录鼠标到中心的初始距离
            Vector2 localMouse = GetLocalMousePos();
            initialDistance = localMouse.magnitude;
        }
        else if (type == GizmoHandle.HandleType.Rotate)
        {
            initialRotation = targetPattern.localEulerAngles.z;
            // 记录鼠标相对于中心的初始角度
            Vector2 localMouse = GetLocalMousePos();
            initialAngle = Mathf.Atan2(localMouse.y, localMouse.x) * Mathf.Rad2Deg;
        }
    }

    public void OnHandleDrag(PointerEventData data, GizmoHandle.HandleType type)
    {
        if (targetPattern == null) return;

        Vector2 currentLocalMouse = GetLocalMousePos();

        if (type == GizmoHandle.HandleType.Scale)
        {
            // 简单的均匀缩放算法：比较当前距离和初始距离的比率
            float currentDistance = currentLocalMouse.magnitude;

            // 防止除以0
            if (initialDistance > 0.001f)
            {
                float factor = currentDistance / initialDistance;
                Vector3 newScale = initialScale * factor;

                // 限制最小最大缩放
                newScale = Vector3.Max(Vector3.one * 0.2f, Vector3.Min(Vector3.one * 5f, newScale));

                targetPattern.localScale = newScale;
            }
        }
        else if (type == GizmoHandle.HandleType.Rotate)
        {
            // 计算当前角度
            float currentAngle = Mathf.Atan2(currentLocalMouse.y, currentLocalMouse.x) * Mathf.Rad2Deg;

            // 计算角度差
            float deltaAngle = currentAngle - initialAngle;

            // 应用旋转
            Vector3 newRot = targetPattern.localEulerAngles;
            newRot.z = initialRotation + deltaAngle;
            targetPattern.localEulerAngles = newRot;
        }
    }

    public void OnHandleEndDrag()
    {
        // 可以在这里添加“撤销/重做”系统的记录逻辑
    }

    // 辅助工具：获取鼠标在Gizmo坐标系下的局部位置
    private Vector2 GetLocalMousePos()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform, // 相对于父级Canvas
            Input.mousePosition,
            parentCanvas.worldCamera,
            out localPoint
        );
        // 因为Gizmo本身在旋转，我们需要把这个点转换到Gizmo的局部空间，或者更简单地：
        // 直接计算相对于Gizmo中心（也就是Target中心）的向量
        return localPoint - (Vector2)transform.localPosition;
    }
}