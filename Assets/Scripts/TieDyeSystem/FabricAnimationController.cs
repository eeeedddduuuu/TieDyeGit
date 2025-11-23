using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FabricAnimationController : MonoBehaviour
{
    // 存储所有布料元素及其原始状态
    // 优化性能的标记
    private bool isInitialized;

    // 存储所有布料元素及其原始状态
    private struct FabricElementState
    {
        public RectTransform rectTransform;
        public Vector3 originalPosition;
        public Vector3 originalScale;
        public RectTransform introduceRect;
        public Vector3 introduceOriginalScale;
        public Vector3 introduceOriginalPosition;
    }

    private FabricElementState[] fabricElements;
    private int lastClickedIndex = -1;
    private bool isAnimating = false;

        // 动画参数（可在Inspector中调节）
    [Header("动画参数")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float introduceAnimationDuration = 0.5f;
    [SerializeField] private float introduceShowDuration = 1.0f; // Introduce显示持续时间
        [SerializeField] private Vector3 centerScale = new Vector3(1.5f, 1.5f, 1.5f);
    [SerializeField] private Vector3 originalScale = new Vector3(1.0f, 1.0f, 1.0f);
    [SerializeField] private Vector3 minimizedScale = new Vector3(0.6f, 0.6f, 0.6f); // 缩小后的比例
    
    // 左侧排列位置参数
    [Header("左侧排列参数")]
    [SerializeField] private Vector3 leftColumnStartPosition = new Vector3(-300f, 150f, 0f); // 左侧排列起始位置
    [SerializeField] private float verticalSpacing = 80f; // 垂直间距
// 动画参数（可在Inspector中调节）
    [Header("动画参数")]
    // 动画计数和状态追踪1.0f);

    // 屏幕中间位置（相对于FabricCanvas的位置）
    [Header("位置参数")]
    [SerializeField] private Vector3 centerPosition = Vector3.zero;

    void Start()
    {
        InitializeFabricElements();
    }

    private void InitializeFabricElements()
    {
        // 获取FabricCanvas的所有直接子元素
        Transform[] children = GetComponentsInChildren<Transform>();
        int childCount = 0;

        // 计算实际有多少个子元素（排除自身）
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != transform)
                childCount++;
        }

        // 初始化数组
        fabricElements = new FabricElementState[childCount];
        int index = 0;

        // 存储每个元素的原始状态并添加点击事件
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == transform) continue;

            GameObject fabricElement = children[i].gameObject;
            Button button = fabricElement.GetComponent<Button>();
            RectTransform rectTransform = fabricElement.GetComponent<RectTransform>();

            if (button != null && rectTransform != null)
            {
                // 查找Introduce子元素
                RectTransform introduceRect = null;
                Transform introTransform = fabricElement.transform.Find("Introduce");
                if (introTransform != null)
                {
                    introduceRect = introTransform.GetComponent<RectTransform>();
                }

                // 存储状态
                fabricElements[index] = new FabricElementState
                {
                    rectTransform = rectTransform,
                    originalPosition = rectTransform.anchoredPosition3D,
                    originalScale = rectTransform.localScale,
                    introduceRect = introduceRect,
                    introduceOriginalScale = introduceRect != null ? introduceRect.localScale : Vector3.one,
                    introduceOriginalPosition = introduceRect != null ? introduceRect.localPosition : Vector3.zero
                };

                // 初始时将Introduce缩放设为0
                if (introduceRect != null)
                {
                    introduceRect.localScale = Vector3.zero;
                }

                // 保存当前索引用于点击事件
                int elementIndex = index;
                button.onClick.AddListener(() => OnFabricElementClicked(elementIndex));

                index++;
            }
        }
    }

    private void OnFabricElementClicked(int clickedIndex)
    {
        if (isAnimating || clickedIndex < 0 || clickedIndex >= fabricElements.Length)
            return;

        isAnimating = true;
        
        // 处理其他元素：缩小并排列到左侧
        int minimizedIndex = 0;
        for (int i = 0; i < fabricElements.Length; i++)
        {
            if (i != clickedIndex)
            {
                // 计算左侧排列位置
                Vector3 targetPosition = leftColumnStartPosition - new Vector3(0f, minimizedIndex * verticalSpacing, 0f);
                AnimateElementToMinimizedState(i, targetPosition);
                minimizedIndex++;
            }
        }
        
        // 动画处理当前点击的元素
        AnimateSelectedElement(clickedIndex);
        
        // 追踪动画完成状态
        DOVirtual.DelayedCall(animationDuration + 0.1f, () => 
        {
            isAnimating = false;
        });

        lastClickedIndex = clickedIndex;
    }

private void AnimateSelectedElement(int index)
    {
        FabricElementState element = fabricElements[index];

        // 移动并放大元素到屏幕中间
        element.rectTransform.DOAnchorPos3D(centerPosition, animationDuration)
            .SetEase(Ease.OutQuad);

        element.rectTransform.DOScale(centerScale, animationDuration)
            .SetEase(Ease.OutQuad);

        // 显示Introduce文本并设置向上移动
        if (element.introduceRect != null)
        {
            // 保存原始位置
            Vector3 originalLocalPosition = element.introduceRect.localPosition;
            
            // 向上移动一点（例如20个单位）
            Vector3 targetLocalPosition = new Vector3(originalLocalPosition.x, originalLocalPosition.y + 20f, originalLocalPosition.z);
            
            // 先将Introduce缩小为0
            element.introduceRect.localScale = Vector3.zero;
            
            // 同时执行缩放和移动动画
            element.introduceRect.DOScale(element.introduceOriginalScale, introduceAnimationDuration)
                .SetEase(Ease.OutBack);
            
            element.introduceRect.DOLocalMove(targetLocalPosition, introduceAnimationDuration)
                .SetEase(Ease.OutQuad);
        }
    }

private void AnimateElementToMinimizedState(int index, Vector3 targetPosition)
    {
        FabricElementState element = fabricElements[index];

        // 移动到左侧排列位置并缩小
        element.rectTransform.DOAnchorPos3D(targetPosition, animationDuration)
            .SetEase(Ease.OutQuad);

        element.rectTransform.DOScale(minimizedScale, animationDuration)
            .SetEase(Ease.OutQuad);

        // 隐藏Introduce文本并恢复原始位置
        if (element.introduceRect != null)
        {
            // 立即隐藏Introduce
            element.introduceRect.DOScale(Vector3.zero, introduceAnimationDuration)
                .SetEase(Ease.InBack);
            
            // 恢复Introduce的原始位置
            element.introduceRect.DOLocalMove(element.introduceOriginalPosition, introduceAnimationDuration)
                .SetEase(Ease.OutQuad);
        }
    }


private void ResetElementToOriginal(int index, bool isCurrentlySelected = false)
    {
        FabricElementState element = fabricElements[index];

        // 恢复原始位置和大小
        element.rectTransform.DOAnchorPos3D(element.originalPosition, animationDuration)
            .SetEase(Ease.OutQuad);

        element.rectTransform.DOScale(element.originalScale, animationDuration)
            .SetEase(Ease.OutQuad);

        // 处理Introduce文本
        if (element.introduceRect != null)
        {
            // 如果是当前选中的元素，保持其可见并向上移动
            if (isCurrentlySelected)
            {
                // 向上移动一点（例如20个单位）
                Vector3 targetLocalPosition = new Vector3(0f, 20f, 0f);
                
                element.introduceRect.DOScale(element.introduceOriginalScale, introduceAnimationDuration)
                    .SetEase(Ease.OutBack);
                
                element.introduceRect.DOLocalMove(targetLocalPosition, introduceAnimationDuration)
                    .SetEase(Ease.OutQuad);
            }
            else
            {
                // 隐藏Introduce文本并恢复原始位置
                element.introduceRect.DOScale(Vector3.zero, introduceAnimationDuration)
                    .SetEase(Ease.InBack);
                
                element.introduceRect.DOLocalMove(element.introduceOriginalPosition, introduceAnimationDuration)
                    .SetEase(Ease.OutQuad);
            }
        }
    }
}
