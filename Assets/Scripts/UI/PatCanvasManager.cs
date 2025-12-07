using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// PatternerScene画布管理器
/// 用于控制MenuCanvas和二级花纹界面之间的切换
/// </summary>
public class PatCanvasManager : MonoBehaviour
{
    [Header("画布引用")]
    public Canvas menuCanvas; // MenuCanvas引用
    public Canvas patternCanvas; // 二级花纹界面引用
    
    [Header("MenuCanvas按钮")]
    public List<Button> menuButtons = new List<Button>(); // MenuCanvas中的五个按钮
    
    [Header("二级花纹界面")]
    public List<GameObject> patternSubpages = new List<GameObject>(); // 二级花纹界面的五个子元素集合
    public Button backButton; // 二级花纹界面的返回按钮
    
    [Header("动画参数")]
    public float fadeDuration = 0.3f; // 淡入淡出动画时长
    public Ease fadeEase = Ease.Linear; // 淡入淡出缓动函数
    
    // 当前显示的花纹子页面索引
    private int currentPatternIndex = -1;
    
    private void Awake()
    {
        // 确保所有组件都已正确引用
        ValidateReferences();
        
        // 添加按钮点击事件监听
        AddButtonListeners();
        
        // 初始化UI状态
        InitializeUI();
    }
    
    /// <summary>
    /// 验证所有必要的引用
    /// </summary>
    private void ValidateReferences()
    {
        if (menuCanvas == null)
        {
            Debug.LogError("MenuCanvas未引用，请在Inspector面板中设置！");
        }
        
        if (patternCanvas == null)
        {
            Debug.LogError("PatternCanvas未引用，请在Inspector面板中设置！");
        }
        
        if (menuButtons.Count != 5)
        {
            Debug.LogWarning("MenuButtons数量不等于5，请检查设置！");
        }
        
        if (patternSubpages.Count != 5)
        {
            Debug.LogWarning("PatternSubpages数量不等于5，请检查设置！");
        }
        
        if (backButton == null)
        {
            Debug.LogError("BackButton未引用，请在Inspector面板中设置！");
        }
    }
    
    /// <summary>
    /// 添加按钮点击事件监听
    /// </summary>
    private void AddButtonListeners()
    {
        // 为MenuCanvas的五个按钮添加点击事件
        for (int i = 0; i < menuButtons.Count; i++)
        {
            if (menuButtons[i] != null)
            {
                int index = i;
                menuButtons[i].onClick.AddListener(() => OnMenuButtonClick(index));
            }
        }
        
        // 为返回按钮添加点击事件
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClick);
        }
    }
    
    /// <summary>
    /// 初始化UI状态
    /// </summary>
    private void InitializeUI()
    {
        // 确保MenuCanvas可见，PatternCanvas不可见
        ShowCanvas(menuCanvas, true);
        ShowCanvas(patternCanvas, false);
        
        // 隐藏所有花纹子页面
        HideAllPatternSubpages();
    }
    
    /// <summary>
    /// MenuCanvas按钮点击事件
    /// </summary>
    /// <param name="index">点击的按钮索引</param>
    private void OnMenuButtonClick(int index)
    {
        if (index < 0 || index >= patternSubpages.Count)
        {
            Debug.LogWarning("无效的花纹子页面索引: " + index);
            return;
        }
        
        // 隐藏MenuCanvas
        ShowCanvas(menuCanvas, false);
        
        // 显示PatternCanvas
        ShowCanvas(patternCanvas, true);
        
        // 显示对应的花纹子页面
        ShowPatternSubpage(index);
        
        // 更新当前索引
        currentPatternIndex = index;
    }
    
    /// <summary>
    /// 返回按钮点击事件
    /// </summary>
    private void OnBackButtonClick()
    {
        // 隐藏PatternCanvas
        ShowCanvas(patternCanvas, false);
        
        // 显示MenuCanvas
        ShowCanvas(menuCanvas, true);
        
        // 隐藏所有花纹子页面
        HideAllPatternSubpages();
        
        // 重置当前索引
        currentPatternIndex = -1;
    }
    
    /// <summary>
    /// 显示或隐藏画布
    /// </summary>
    /// <param name="canvas">要操作的画布</param>
    /// <param name="show">是否显示</param>
    private void ShowCanvas(Canvas canvas, bool show)
    {
        if (canvas == null)
        {
            return;
        }
        
        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        if (show)
        {
            // 显示画布
            canvas.gameObject.SetActive(true);
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
            // 隐藏画布
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0f, fadeDuration)
                .SetEase(fadeEase)
                .OnComplete(() => 
                {
                    canvas.gameObject.SetActive(false);
                });
        }
    }
    
    /// <summary>
    /// 显示指定的花纹子页面
    /// </summary>
    /// <param name="index">子页面索引</param>
    private void ShowPatternSubpage(int index)
    {
        // 隐藏所有子页面，但跳过当前要显示的子页面
        HideAllPatternSubpagesExcept(index);
        
        if (index >= 0 && index < patternSubpages.Count && patternSubpages[index] != null)
        {
            // 显示指定的子页面
            GameObject subpage = patternSubpages[index];
            CanvasGroup canvasGroup = subpage.GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                canvasGroup = subpage.AddComponent<CanvasGroup>();
            }
            
            subpage.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeDuration)
                .SetEase(fadeEase)
                .OnComplete(() => 
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                });
        }
    }
    
    /// <summary>
    /// 隐藏所有花纹子页面
    /// </summary>
    private void HideAllPatternSubpages()
    {
        HideAllPatternSubpagesExcept(-1);
    }
    
    /// <summary>
    /// 隐藏所有花纹子页面，但跳过指定索引的子页面
    /// </summary>
    /// <param name="exceptIndex">要跳过的子页面索引，-1表示不跳过任何页面</param>
    private void HideAllPatternSubpagesExcept(int exceptIndex)
    {
        for (int i = 0; i < patternSubpages.Count; i++)
        {
            // 如果是要跳过的页面，则继续下一个循环
            if (i == exceptIndex)
            {
                continue;
            }
            
            if (patternSubpages[i] != null)
            {
                GameObject subpage = patternSubpages[i];
                CanvasGroup canvasGroup = subpage.GetComponent<CanvasGroup>();
                
                if (canvasGroup != null)
                {
                    // 取消当前可能正在播放的动画
                    canvasGroup.DOKill(false);
                    
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    canvasGroup.DOFade(0f, fadeDuration)
                        .SetEase(fadeEase)
                        .OnComplete(() => 
                        {
                            subpage.SetActive(false);
                        });
                }
                else
                {
                    subpage.SetActive(false);
                }
            }
        }
    }
}