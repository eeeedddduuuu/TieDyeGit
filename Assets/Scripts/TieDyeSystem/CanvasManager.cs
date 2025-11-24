using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    // Canvas引用数组，按照指定顺序排列
    public Canvas[] canvases = new Canvas[4];
    
    // 当前显示的Canvas索引
    private int currentCanvasIndex = 0;
    
    // PreviousStep和NextStep按钮
    public Button previousButton;
    public Button nextButton;
    
    // Canvas初始状态记录（用于恢复原状）
    private bool[] canvasActiveStates;
    private Vector3[] canvasPositions;
    private Vector3[] canvasRotations;
    private Vector3[] canvasScales;
    
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
        
        // 记录每个Canvas的初始状态
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null)
            {
                canvasActiveStates[i] = canvases[i].gameObject.activeSelf;
                canvasPositions[i] = canvases[i].transform.position;
                canvasRotations[i] = canvases[i].transform.rotation.eulerAngles;
                canvasScales[i] = canvases[i].transform.localScale;
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
        }
    }
    
    // 更新Canvas可见性
    private void UpdateCanvasVisibility()
    {
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null)
            {
                bool shouldBeActive = (i == currentCanvasIndex);
                
                // 保存当前激活状态以便将来恢复
                if (!shouldBeActive && canvases[i].gameObject.activeSelf)
                {
                    canvasActiveStates[i] = true;
                }
                
                // 设置Canvas的激活状态
                canvases[i].gameObject.SetActive(shouldBeActive);
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
            // 恢复当前Canvas状态
            ResetCanvasState(currentCanvasIndex);
            
            // 切换到上一个Canvas
            currentCanvasIndex--;
            
            // 恢复目标Canvas状态
            ResetCanvasState(currentCanvasIndex);
            
            // 更新显示和按钮状态
            UpdateCanvasVisibility();
            UpdateButtonStates();
            
            Debug.Log("切换到上一个Canvas: " + canvases[currentCanvasIndex].name);
        }
    }
    
    // 下一步
    public void NextStep()
    {
        if (currentCanvasIndex < canvases.Length - 1)
        {
            // 恢复当前Canvas状态
            ResetCanvasState(currentCanvasIndex);
            
            // 切换到下一个Canvas
            currentCanvasIndex++;
            
            // 恢复目标Canvas状态
            ResetCanvasState(currentCanvasIndex);
            
            // 更新显示和按钮状态
            UpdateCanvasVisibility();
            UpdateButtonStates();
            
            Debug.Log("切换到下一个Canvas: " + canvases[currentCanvasIndex].name);
        }
    }
    
    // 直接跳转到指定索引的Canvas
    public void GoToCanvas(int index)
    {
        if (index >= 0 && index < canvases.Length && index != currentCanvasIndex)
        {
            // 恢复当前Canvas状态
            ResetCanvasState(currentCanvasIndex);
            
            // 切换到指定Canvas
            currentCanvasIndex = index;
            
            // 恢复目标Canvas状态
            ResetCanvasState(currentCanvasIndex);
            
            // 更新显示和按钮状态
            UpdateCanvasVisibility();
            UpdateButtonStates();
            
            Debug.Log("直接跳转到Canvas: " + canvases[currentCanvasIndex].name);
        }
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