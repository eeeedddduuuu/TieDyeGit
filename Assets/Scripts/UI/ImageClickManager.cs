using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class AdvancedImageClickManager : MonoBehaviour
{
    [Header("第一次点击动画设置")]
    public float firstMoveDistance = 200f; // 第一次点击下移距离
    public float firstScaleMultiplier = 1.5f; // 第一次点击放大倍数
    public float firstAnimationDuration = 1.2f; // 第一次动画持续时间

    [Header("第二次点击动画设置")]
    public float secondMoveDistance = 200f; // 第二次点击右移距离（可调节）
    public float secondScaleMultiplier = 1.8f; // 第二次点击放大倍数（可调节）
    public float secondAnimationDuration = 0.7f; // 第二次动画持续时间

    [Header("通用动画设置")]
    public float disappearDuration = 0.8f; // 消失动画持续时间
    public Ease scaleEase = Ease.OutBack;
    public Ease moveEase = Ease.OutCubic;

    [Header("UI元素引用")]
    public Transform otherElementsParent; // 其他需要消失的元素的父对象
    public List<Image> clickableImages = new List<Image>(); // 可点击的图片列表
    public Button backArrow; // 返回箭头按钮

    // 状态管理
    private Vector3[] originalPositions;
    private Vector3[] originalScales;
    private Image currentlySelected;
    private int clickCount = 0; // 点击次数计数
    private Vector3 firstClickPosition; // 第一次点击后的位置
    private Vector3 firstClickScale; // 第一次点击后的缩放

    // 历史记录，用于返回功能
    private Stack<UIState> stateHistory = new Stack<UIState>();

    void Start()
    {
        // 保存所有图片的原始位置和缩放
        SaveOriginalTransforms();

        // 为每个图片添加点击事件
        for (int i = 0; i < clickableImages.Count; i++)
        {
            int index = i;
            clickableImages[i].GetComponent<Button>().onClick.AddListener(() => OnImageClicked(index));
        }

        // 设置返回箭头点击事件
        if (backArrow != null)
        {
            backArrow.onClick.AddListener(OnBackArrowClicked);
            backArrow.gameObject.SetActive(false); // 初始隐藏返回箭头
        }

        // 保存初始状态
        SaveCurrentState("初始状态");
    }

    void SaveOriginalTransforms()
    {
        originalPositions = new Vector3[clickableImages.Count];
        originalScales = new Vector3[clickableImages.Count];

        for (int i = 0; i < clickableImages.Count; i++)
        {
            originalPositions[i] = clickableImages[i].transform.localPosition;
            originalScales[i] = clickableImages[i].transform.localScale;
        }
    }

    void OnImageClicked(int imageIndex)
    {
        Image clickedImage = clickableImages[imageIndex];

        // 如果点击的是不同的图片，重置状态
        if (currentlySelected != clickedImage)
        {
            ResetToInitialState();
            currentlySelected = clickedImage;
            clickCount = 1;
            SaveCurrentState($"第一次点击图片 {imageIndex}");
            PlayFirstClickAnimation(clickedImage, imageIndex);
        }
        else
        {
            // 点击同一图片
            clickCount++;

            if (clickCount == 2)
            {
                SaveCurrentState($"第二次点击图片 {imageIndex}");
                PlaySecondClickAnimation(clickedImage, imageIndex);
            }
        }
    }

    void PlayFirstClickAnimation(Image image, int index)
    {
        image.transform.DOKill();

        // 计算第一次点击后的目标位置和缩放
        firstClickPosition = originalPositions[index] + Vector3.down * firstMoveDistance;
        firstClickScale = originalScales[index] * firstScaleMultiplier;

        Sequence sequence = DOTween.Sequence();
        sequence.Join(image.transform.DOLocalMove(firstClickPosition, firstAnimationDuration).SetEase(moveEase));
        sequence.Join(image.transform.DOScale(firstClickScale, firstAnimationDuration).SetEase(scaleEase));

        sequence.OnComplete(() => {
            Debug.Log($"第一次点击完成：下移 {firstMoveDistance} 像素，放大 {firstScaleMultiplier} 倍");

            // 显示返回箭头
            if (backArrow != null)
            {
                backArrow.gameObject.SetActive(true);
                PlayBackArrowEntranceAnimation();
            }
        });

        // 其他元素消失动画
        PlayDisappearAnimation();
    }

    void PlaySecondClickAnimation(Image image, int index)
    {
        image.transform.DOKill();

        // 计算第二次点击后的目标位置和缩放（向右移动并放大）
        Vector3 targetPosition = firstClickPosition + Vector3.right * secondMoveDistance; // 可调节：向右移动的距离
        Vector3 targetScale = firstClickScale * secondScaleMultiplier; // 可调节：进一步放大的倍数

        Sequence sequence = DOTween.Sequence();
        sequence.Join(image.transform.DOLocalMove(targetPosition, secondAnimationDuration).SetEase(moveEase));
        sequence.Join(image.transform.DOScale(targetScale, secondAnimationDuration).SetEase(scaleEase));

        sequence.OnComplete(() => {
            Debug.Log($"第二次点击完成：右移 {secondMoveDistance} 像素，再次放大 {secondScaleMultiplier} 倍");
        });
    }

    void PlayDisappearAnimation()
    {
        if (otherElementsParent == null) return;

        List<Transform> childrenToDisappear = new List<Transform>();
        foreach (Transform child in otherElementsParent)
        {
            childrenToDisappear.Add(child);
        }

        for (int i = 0; i < childrenToDisappear.Count; i++)
        {
            Transform child = childrenToDisappear[i];
            float delay = i * 0.1f;

            Sequence disappearSequence = DOTween.Sequence();
            disappearSequence.AppendInterval(delay);
            disappearSequence.Append(child.DOLocalMoveX(child.localPosition.x + 500f, disappearDuration));

            // 确保有CanvasGroup组件
            CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = child.gameObject.AddComponent<CanvasGroup>();
            }
            disappearSequence.Join(canvasGroup.DOFade(0f, disappearDuration));

            disappearSequence.OnComplete(() => child.gameObject.SetActive(false));
        }
    }

    void PlayBackArrowEntranceAnimation()
    {
        if (backArrow == null) return;

        backArrow.transform.DOKill();

        // 设置初始状态（缩小并透明）
        backArrow.transform.localScale = Vector3.zero;
        CanvasGroup arrowCanvasGroup = backArrow.GetComponent<CanvasGroup>();
        if (arrowCanvasGroup == null)
        {
            arrowCanvasGroup = backArrow.gameObject.AddComponent<CanvasGroup>();
        }
        arrowCanvasGroup.alpha = 0f;

        // 执行入场动画
        Sequence sequence = DOTween.Sequence();
        sequence.Append(backArrow.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        sequence.Join(arrowCanvasGroup.DOFade(1f, 0.5f));
    }

    void OnBackArrowClicked()
    {
        if (stateHistory.Count > 1)
        {
            // 移除当前状态
            stateHistory.Pop();

            // 恢复到上一个状态
            UIState previousState = stateHistory.Peek();
            RestoreState(previousState);

            Debug.Log("返回上一个界面");
        }
        else
        {
            ResetToInitialState();
        }
    }

    void SaveCurrentState(string description)
    {
        UIState state = new UIState
        {
            description = description,
            selectedImageIndex = GetImageIndex(currentlySelected),
            clickCount = clickCount,
            imagePositions = new Vector3[clickableImages.Count],
            imageScales = new Vector3[clickableImages.Count],
            otherElementsActive = new bool[otherElementsParent.childCount]
        };

        // 保存图片状态
        for (int i = 0; i < clickableImages.Count; i++)
        {
            state.imagePositions[i] = clickableImages[i].transform.localPosition;
            state.imageScales[i] = clickableImages[i].transform.localScale;
        }

        // 保存其他元素状态
        for (int i = 0; i < otherElementsParent.childCount; i++)
        {
            state.otherElementsActive[i] = otherElementsParent.GetChild(i).gameObject.activeSelf;
        }

        stateHistory.Push(state);
    }

    void RestoreState(UIState state)
    {
        // 恢复图片状态
        for (int i = 0; i < clickableImages.Count; i++)
        {
            clickableImages[i].transform.DOKill();
            clickableImages[i].transform.localPosition = state.imagePositions[i];
            clickableImages[i].transform.localScale = state.imageScales[i];
        }

        // 恢复其他元素状态
        for (int i = 0; i < otherElementsParent.childCount; i++)
        {
            Transform child = otherElementsParent.GetChild(i);
            child.gameObject.SetActive(state.otherElementsActive[i]);

            if (state.otherElementsActive[i])
            {
                child.DOKill();
                child.localPosition = Vector3.zero;
                CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
            }
        }

        // 恢复当前选中图片和点击计数
        currentlySelected = (state.selectedImageIndex >= 0) ?
            clickableImages[state.selectedImageIndex] : null;
        clickCount = state.clickCount;

        // 根据状态决定是否显示返回箭头
        if (backArrow != null)
        {
            backArrow.gameObject.SetActive(state.clickCount > 0);
            if (state.clickCount > 0)
            {
                PlayBackArrowEntranceAnimation();
            }
        }
    }

    void ResetToInitialState()
    {
        // 重置所有可点击图片
        for (int i = 0; i < clickableImages.Count; i++)
        {
            clickableImages[i].transform.DOKill();
            clickableImages[i].transform.localPosition = originalPositions[i];
            clickableImages[i].transform.localScale = originalScales[i];
        }

        // 重置其他元素
        ResetOtherElements();

        // 重置状态
        currentlySelected = null;
        clickCount = 0;

        // 隐藏返回箭头
        if (backArrow != null)
        {
            backArrow.gameObject.SetActive(false);
        }

        // 清空历史记录并保存初始状态
        stateHistory.Clear();
        SaveCurrentState("初始状态");
    }

    void ResetOtherElements()
    {
        if (otherElementsParent == null) return;

        foreach (Transform child in otherElementsParent)
        {
            child.DOKill();
            child.gameObject.SetActive(true);
            child.localPosition = Vector3.zero;

            CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
    }

    int GetImageIndex(Image image)
    {
        for (int i = 0; i < clickableImages.Count; i++)
        {
            if (clickableImages[i] == image)
                return i;
        }
        return -1;
    }

    // 状态记录类
    [System.Serializable]
    public class UIState
    {
        public string description;
        public int selectedImageIndex;
        public int clickCount;
        public Vector3[] imagePositions;
        public Vector3[] imageScales;
        public bool[] otherElementsActive;
    }
}