using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// Product场景画布管理器
/// 用于控制Tag按钮与对应分类界面的切换
/// </summary>
public class ProCanvasManager : MonoBehaviour
{
    [Header("Tag按钮")]
    public List<Button> tagButtons = new List<Button>(); // Tag条的五个按钮
    
    [Header("分类界面")]
    public List<GameObject> categoryPages = new List<GameObject>(); // 对应的五个分类界面
    
    [Header("动画参数")]
    public float fadeDuration = 0.3f; // 淡入淡出动画时长
    public Ease fadeEase = Ease.Linear; // 淡入淡出缓动函数
    
    // 当前显示的分类界面索引
    private int currentPageIndex = 0;
    
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
        if (tagButtons.Count != 5)
        {
            Debug.LogWarning("TagButtons数量不等于5，请检查设置！");
        }
        
        if (categoryPages.Count != 5)
        {
            Debug.LogWarning("CategoryPages数量不等于5，请检查设置！");
        }
        
        // 确保按钮和页面数量匹配
        if (tagButtons.Count != categoryPages.Count)
        {
            Debug.LogError("TagButtons和CategoryPages数量不匹配，请检查设置！");
        }
    }
    
    /// <summary>
    /// 添加按钮点击事件监听
    /// </summary>
    private void AddButtonListeners()
    {
        // 为每个Tag按钮添加点击事件
        for (int i = 0; i < tagButtons.Count; i++)
        {
            if (tagButtons[i] != null)
            {
                int index = i;
                tagButtons[i].onClick.AddListener(() => OnTagButtonClick(index));
            }
        }
    }
    
    /// <summary>
    /// 初始化UI状态
    /// </summary>
    private void InitializeUI()
    {
        // 显示第一个分类界面，隐藏其他界面
        ShowCategoryPage(0);
    }
    
    /// <summary>
    /// Tag按钮点击事件
    /// </summary>
    /// <param name="index">点击的按钮索引</param>
    private void OnTagButtonClick(int index)
    {
        if (index < 0 || index >= categoryPages.Count || index == currentPageIndex)
        {
            return;
        }
        
        // 显示对应的分类界面，隐藏其他界面
        ShowCategoryPage(index);
        
        // 更新当前索引
        currentPageIndex = index;
    }
    
    /// <summary>
    /// 显示指定的分类界面，隐藏其他界面
    /// </summary>
    /// <param name="index">要显示的界面索引</param>
    private void ShowCategoryPage(int index)
    {
        for (int i = 0; i < categoryPages.Count; i++)
        {
            if (categoryPages[i] != null)
            {
                GameObject page = categoryPages[i];
                CanvasGroup canvasGroup = page.GetComponent<CanvasGroup>();
                
                if (canvasGroup == null)
                {
                    canvasGroup = page.AddComponent<CanvasGroup>();
                }
                
                // 取消当前可能正在播放的动画
                canvasGroup.DOKill(false);
                
                if (i == index)
                {
                    // 显示当前页面
                    page.SetActive(true);
                    canvasGroup.alpha = 0f;
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
                    // 隐藏其他页面
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    canvasGroup.DOFade(0f, fadeDuration)
                        .SetEase(fadeEase)
                        .OnComplete(() => 
                        {
                            page.SetActive(false);
                        });
                }
            }
        }
    }
}