using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// SewControl脚本用于控制ClothCanvas中的tool按钮点击事件
/// 实现mian子元素渐隐消失，ClothGroup子元素渐显出现的效果
/// </summary>
public class SewControl : MonoBehaviour
{
    [Header("ClothCanvas引用")]
    public GameObject clothCanvas; // ClothCanvas游戏对象引用
    
    [Header("按钮引用")]
    public Button toolButton; // Tool按钮引用
    
    [Header("子元素引用")]
    public GameObject mianElement; // mian子元素引用
    public GameObject clothGroupElement; // ClothGroup子元素引用
    
    [Header("动画参数")]
    public float fadeDuration = 0.5f; // 渐变动画持续时间
    public Ease fadeEase = Ease.Linear; // 渐变动画缓动函数
    
    private void Start()
    {
        // 如果未指定clothCanvas，则默认使用当前游戏对象的Canvas组件
        if (clothCanvas == null)
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                clothCanvas = canvas.gameObject;
            }
        }
        
        // 如果未指定toolButton，尝试在clothCanvas中查找
        if (toolButton == null && clothCanvas != null)
        {
            toolButton = clothCanvas.transform.Find("tool")?.GetComponent<Button>();
        }
        
        // 如果未指定mianElement，尝试在clothCanvas中查找
        if (mianElement == null && clothCanvas != null)
        {
            mianElement = clothCanvas.transform.Find("mian")?.gameObject;
        }
        
        // 如果未指定clothGroupElement，尝试在clothCanvas中查找
        if (clothGroupElement == null && clothCanvas != null)
        {
            clothGroupElement = clothCanvas.transform.Find("ClothGroup")?.gameObject;
        }
        
        // 添加按钮点击事件监听
        if (toolButton != null)
        {
            toolButton.onClick.AddListener(OnToolButtonClick);
        }
        
        // 初始化状态：显示mian元素，隐藏ClothGroup元素
        if (mianElement != null)
        {
            SetElementState(mianElement, true, 1f);
        }
        
        if (clothGroupElement != null)
        {
            SetElementState(clothGroupElement, false, 0f);
        }
    }
    
    /// <summary>
    /// 设置元素的初始状态
    /// </summary>
    /// <param name="element">要设置的元素</param>
    /// <param name="visible">是否可见</param>
    /// <param name="alpha">透明度值</param>
    private void SetElementState(GameObject element, bool visible, float alpha)
    {
        if (element == null)
            return;
        
        element.SetActive(visible);
        
        CanvasGroup canvasGroup = element.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = element.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = alpha;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }
    
    /// <summary>
    /// Tool按钮点击事件处理
    /// </summary>
    private void OnToolButtonClick()
    {
        // 实现mian子元素渐隐消失
        if (mianElement != null)
        {
            FadeElement(mianElement, false);
        }
        
        // 实现ClothGroup子元素渐显出现
        if (clothGroupElement != null)
        {
            FadeElement(clothGroupElement, true);
        }
    }
    
    /// <summary>
    /// 渐变显示或隐藏元素
    /// </summary>
    /// <param name="element">要渐变的元素</param>
    /// <param name="show">是否显示元素</param>
    private void FadeElement(GameObject element, bool show)
    {
        if (element == null)
            return;
        
        CanvasGroup canvasGroup = element.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = element.AddComponent<CanvasGroup>();
        }
        
        if (show)
        {
            // 显示元素
            element.SetActive(true);
            canvasGroup.DOFade(1f, fadeDuration)
                .SetEase(fadeEase)
                .OnComplete(() => 
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                });
        }
        else
        {
            // 隐藏元素
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0f, fadeDuration)
                .SetEase(fadeEase)
                .OnComplete(() => 
                {
                    element.SetActive(false);
                });
        }
    }
}