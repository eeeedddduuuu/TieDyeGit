using UnityEngine;
using UnityEngine.EventSystems;

public class GizmoHandle : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    // 定义手柄类型
    public enum HandleType { Scale, Rotate }
    public HandleType type;

    // 引用主控制器
    private TransformGizmoController controller;

    void Start()
    {
        controller = GetComponentInParent<TransformGizmoController>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        controller.OnHandleBeginDrag(type);
    }

    public void OnDrag(PointerEventData eventData)
    {
        controller.OnHandleDrag(eventData, type);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        controller.OnHandleEndDrag();
    }
}