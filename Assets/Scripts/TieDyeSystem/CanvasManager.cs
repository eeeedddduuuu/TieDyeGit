using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CanvasManager : MonoBehaviour
{
    // Canvas引用数组，按照指定顺序排列
    public Canvas[] canvases = new Canvas[4];
    
    // 当前显示的Canvas索引
    private int currentCanvasIndex = 0;
    
    // PreviousStep和NextStep按钮
    public Button previousButton;
    public Button nextButton;
    
    // 动画参数
    [Header("动画参数")]
    public float fadeDuration = 0.3f; // 淡入淡出动画时长
    public float slideDuration = 0.3f; // 滑动动画时长
    public Ease fadeEase = Ease.Linear; // 淡入淡出缓动函数
    public Ease slideEase = Ease.OutQuad; // 滑动缓动函数
    public float slideDistance = 100f; // 滑动距离
    
    // Canvas初始状态记录（用于恢复原状）
    private bool[] canvasActiveStates;
    private Vector3[] canvasPositions;
    private Vector3[] canvasRotations;
    private Vector3[] canvasScales;
    private CanvasGroup[] canvasGroups; // 存储每个Canvas的CanvasGroup组件
    
    void Start()
    {
        // 初始化状态记录数组
        InitializeCanvasStates();
        
        // 设置初始Canvas显示状态
        UpdateCanvasVisibility();
        
        // 添加按钮点击事件监听
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousStep);
        
        if (nextButton != null)
            nextButton.onClick.AddListener(NextStep);
        
        // 更新按钮状态
        UpdateButtonStates();
    }
    
    private void InitializeCanvasStates()
    {
        canvasActiveStates = new bool[canvases.Length];
        canvasPositions = new Vector3[canvases.Length];
        canvasRotations = new Vector3[canvases.Length];
        canvasScales = new Vector3[canvases.Length];
        canvasGroups = new CanvasGroup[canvases.Length];
        
        // 记录每个Canvas的初始状态
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null)
            {
                canvasActiveStates[i] = canvases[i].gameObject.activeSelf;
                canvasPositions[i] = canvases[i].transform.position;
                canvasRotations[i] = canvases[i].transform.rotation.eulerAngles;
                canvasScales[i] = canvases[i].transform.localScale;
                
                // 为Canvas添加CanvasGroup组件（如果没有）
                canvasGroups[i] = canvases[i].GetComponent<CanvasGroup>();
                if (canvasGroups[i] == null)
                {
                    canvasGroups[i] = canvases[i].gameObject.AddComponent<CanvasGroup>();
                }
                
                // 初始状态设置
                if (i == currentCanvasIndex)
                {
                    canvasGroups[i].alpha = 1f;
                    canvasGroups[i].interactable = true;
                    canvasGroups[i].blocksRaycasts = true;
                }
                else
                {
                    canvasGroups[i].alpha = 0f;
                    canvasGroups[i].interactable = false;
                    canvasGroups[i].blocksRaycasts = false;
                }
            }
        }
    }
    
    // 恢复Canvas到初始状态
    private void ResetCanvasState(int index)
    {
        if (index >= 0 && index < canvases.Length && canvases[index] != null)
        {
            // 仅恢复位置、旋转和缩放，不改变激活状态（由UpdateCanvasVisibility控制）
            canvases[index].transform.position = canvasPositions[index];
            canvases[index].transform.rotation = Quaternion.Euler(canvasRotations[index]);
            canvases[index].transform.localScale = canvasScales[index];
            
            // 如果CanvasGroup存在，确保它的初始状态正确
            if (canvasGroups[index] != null)
            {
                if (index == currentCanvasIndex)
                {
                    canvasGroups[index].alpha = 1f;
                    canvasGroups[index].interactable = true;
                    canvasGroups[index].blocksRaycasts = true;
                }
                else
                {
                    canvasGroups[index].alpha = 0f;
                    canvasGroups[index].interactable = false;
                    canvasGroups[index].blocksRaycasts = false;
                }
            }
        }
    }
    
    // 更新Canvas可见性 - 不使用，而是通过CanvasGroup控制透明度和交互性
    private void UpdateCanvasVisibility()
    {
        // 确保所有Canvas都处于激活状态（通过CanvasGroup控制可见性）
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null)
            {
                // 保存当前激活状态以便将来恢复
                if (canvases[i].gameObject.activeSelf)
                {
                    canvasActiveStates[i] = true;
                }
                
                // 确保Canvas始终处于激活状态
                canvases[i].gameObject.SetActive(true);
            }
        }
    }
    
    // 更新按钮状态（首尾页面的按钮可用性）
    private void UpdateButtonStates()
    {
        if (previousButton != null)
            previousButton.interactable = (currentCanvasIndex > 0);
        
        if (nextButton != null)
            nextButton.interactable = (currentCanvasIndex < canvases.Length - 1);
    }
    
    // 上一步
    public void PreviousStep()
    {
        if (currentCanvasIndex > 0)
        {
            int nextIndex = currentCanvasIndex - 1;
            TransitionToCanvas(nextIndex, Direction.Left);
            currentCanvasIndex = nextIndex;
            UpdateButtonStates();
            Debug.Log("切换到上一个Canvas: " + canvases[currentCanvasIndex].name);
        }
    }
    
    // 下一步
    public void NextStep()
    {
        if (currentCanvasIndex < canvases.Length - 1)
        {
            int nextIndex = currentCanvasIndex + 1;
            TransitionToCanvas(nextIndex, Direction.Right);
            currentCanvasIndex = nextIndex;
            UpdateButtonStates();
            Debug.Log("切换到下一个Canvas: " + canvases[currentCanvasIndex].name);
        }
    }
    
    // 直接跳转到指定索引的Canvas
    public void GoToCanvas(int index)
    {
        if (index >= 0 && index < canvases.Length && index != currentCanvasIndex)
        {
            // 根据索引决定动画方向
            Direction direction = (index > currentCanvasIndex) ? Direction.Right : Direction.Left;
            TransitionToCanvas(index, direction);
            currentCanvasIndex = index;
            UpdateButtonStates();
            Debug.Log("直接跳转到Canvas: " + canvases[currentCanvasIndex].name);
        }
    }
    
    // 动画方向枚举
    private enum Direction
    {
        Left,
        Right
    }
    
    // 使用DOTween进行Canvas切换动画
    private void TransitionToCanvas(int targetIndex, Direction direction)
    {
        if (currentCanvasIndex < 0 || currentCanvasIndex >= canvases.Length || 
            targetIndex < 0 || targetIndex >= canvases.Length)
            return;
            
        Canvas currentCanvas = canvases[currentCanvasIndex];
        Canvas targetCanvas = canvases[targetIndex];
        CanvasGroup currentGroup = canvasGroups[currentCanvasIndex];
        CanvasGroup targetGroup = canvasGroups[targetIndex];
        
        if (currentCanvas == null || targetCanvas == null)
            return;
        
        // 确保目标Canvas可见
        targetCanvas.gameObject.SetActive(true);
        
        // 重置目标Canvas的位置
        targetCanvas.transform.position = canvasPositions[targetIndex];
        targetCanvas.transform.rotation = Quaternion.Euler(canvasRotations[targetIndex]);
        targetCanvas.transform.localScale = canvasScales[targetIndex];
        
        // 计算目标Canvas的初始位置（屏幕外）
        Vector3 targetStartPosition = targetCanvas.transform.position;
        float directionValue = (direction == Direction.Left) ? -1 : 1;
        targetStartPosition.x += slideDistance * directionValue;
        targetCanvas.transform.position = targetStartPosition;
        
        // 设置目标CanvasGroup初始状态
        targetGroup.alpha = 0f;
        targetGroup.interactable = true;
        targetGroup.blocksRaycasts = true;
        
        // 创建DOTween序列
        DOTween.Sequence()
            // 淡出当前Canvas，同时淡入目标Canvas
            .Append(currentGroup.DOFade(0f, fadeDuration).SetEase(fadeEase))
            .Join(targetGroup.DOFade(1f, fadeDuration).SetEase(fadeEase))
            // 滑动动画：当前Canvas向相反方向移动，目标Canvas向原位移动
            .Join(currentCanvas.transform.DOMoveX(
                currentCanvas.transform.position.x - slideDistance * directionValue,
                slideDuration).SetEase(slideEase))
            .Join(targetCanvas.transform.DOMoveX(
                canvasPositions[targetIndex].x,
                slideDuration).SetEase(slideEase))
            // 动画完成后的回调
            .OnComplete(() => {
                // 重置当前Canvas的位置和状态
                currentCanvas.transform.position = canvasPositions[currentCanvasIndex];
                currentGroup.interactable = false;
                currentGroup.blocksRaycasts = false;
                
                Debug.Log("Canvas切换动画完成: " + targetCanvas.name);
            });
    }
    
    // 获取当前Canvas索引
    public int GetCurrentCanvasIndex()
    {
        return currentCanvasIndex;
    }
    
    // 获取当前Canvas
    public Canvas GetCurrentCanvas()
    {
        if (currentCanvasIndex >= 0 && currentCanvasIndex < canvases.Length)
            return canvases[currentCanvasIndex];
        return null;
    }
}