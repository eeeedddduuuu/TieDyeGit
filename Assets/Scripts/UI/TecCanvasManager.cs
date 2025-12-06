using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// TechCanvas管理器
/// 用于控制TechCanvas中一级和二级子元素的显示和切换
/// </summary>
public class TecCanvasManager : MonoBehaviour
{
    // 一级子元素列表（带Button组件）
    private List<Transform> firstLevelElements = new List<Transform>();
    
    // 二级子元素字典：键为一级子元素索引，值为对应的二级子元素列表
    private Dictionary<int, List<Transform>> secondLevelElements = new Dictionary<int, List<Transform>>();
    
    // 当前显示的二级子元素索引
    private int currentSecondLevelIndex = 0;
    
    // 动画参数
    [Header("动画参数")]
    public float fadeDuration = 0.3f; // 淡入淡出动画时长
    public float scaleDuration = 0.3f; // 缩放动画时长
    public Ease fadeEase = Ease.Linear; // 淡入淡出缓动函数
    public Ease scaleEase = Ease.OutBack; // 缩放缓动函数
    
    private void Awake()
    {
        // 获取所有一级子元素（带Button组件的直接子元素）
        CollectFirstLevelElements();
        
        // 获取所有二级子元素
        CollectSecondLevelElements();
        
        // 初始化UI状态
        InitializeUI();
    }
    
    /// <summary>
    /// 收集所有一级子元素（带Button组件的直接子元素）
    /// </summary>
    private void CollectFirstLevelElements()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Button button = child.GetComponent<Button>();
            
            // 如果子元素有Button组件，则视为一级子元素
            if (button != null)
            {
                firstLevelElements.Add(child);
                
                // 添加按钮点击事件监听
                int index = firstLevelElements.Count - 1;
                button.onClick.AddListener(() => OnFirstLevelElementClick(index));
            }
        }
        
        Debug.Log("收集到一级子元素数量：" + firstLevelElements.Count);
    }
    
    /// <summary>
    /// 收集所有二级子元素
    /// </summary>
    private void CollectSecondLevelElements()
    {
        for (int i = 0; i < firstLevelElements.Count; i++)
        {
            Transform firstLevelElement = firstLevelElements[i];
            List<Transform> secondLevelList = new List<Transform>();
            
            // 遍历一级子元素的所有子元素，视为二级子元素
            for (int j = 0; j < firstLevelElement.childCount; j++)
            {
                Transform child = firstLevelElement.GetChild(j);
                secondLevelList.Add(child);
            }
            
            secondLevelElements.Add(i, secondLevelList);
            Debug.Log("一级子元素 " + i + " 收集到二级子元素数量：" + secondLevelList.Count);
        }
    }
    
    /// <summary>
    /// 初始化UI状态
    /// </summary>
    private void InitializeUI()
    {
        // 确保所有一级子元素可见
        foreach (Transform firstLevelElement in firstLevelElements)
        {
            firstLevelElement.gameObject.SetActive(true);
        }
        
        // 隐藏所有二级子元素
        foreach (var pair in secondLevelElements)
        {
            HideSecondLevelElements(pair.Key);
        }
        
        // 默认显示第一个一级子元素的二级子元素
        ShowSecondLevelElements(0);
    }
    
    /// <summary>
    /// 一级子元素点击事件
    /// </summary>
    /// <param name="index">点击的一级子元素索引</param>
    private void OnFirstLevelElementClick(int index)
    {
        if (index == currentSecondLevelIndex)
        {
            // 如果点击的是当前显示的一级子元素，不做任何操作
            return;
        }
        
        // 隐藏当前显示的二级子元素
        HideSecondLevelElements(currentSecondLevelIndex);
        
        // 显示点击的一级子元素的二级子元素
        ShowSecondLevelElements(index);
        
        // 更新当前显示索引
        currentSecondLevelIndex = index;
    }
    
    /// <summary>
    /// 显示指定一级子元素的二级子元素
    /// </summary>
    /// <param name="firstLevelIndex">一级子元素索引</param>
    private void ShowSecondLevelElements(int firstLevelIndex)
    {
        if (!secondLevelElements.ContainsKey(firstLevelIndex))
        {
            Debug.LogWarning("没有找到索引为 " + firstLevelIndex + " 的二级子元素列表");
            return;
        }
        
        List<Transform> elements = secondLevelElements[firstLevelIndex];
        
        // 显示所有二级子元素并播放进入动画
        for (int i = 0; i < elements.Count; i++)
        {
            Transform element = elements[i];
            element.gameObject.SetActive(true);
            
            // 获取或添加CanvasGroup组件
            CanvasGroup canvasGroup = element.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = element.gameObject.AddComponent<CanvasGroup>();
            }
            
            // 重置缩放和透明度
            element.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
            
            // 使用DOTween播放进入动画
            float delay = i * 0.05f; // 错开动画延迟
            
            element.DOScale(Vector3.one, scaleDuration)
                .SetEase(scaleEase)
                .SetDelay(delay);
            
            canvasGroup.DOFade(1f, fadeDuration)
                .SetEase(fadeEase)
                .SetDelay(delay);
        }
    }
    
    /// <summary>
    /// 隐藏指定一级子元素的二级子元素
    /// </summary>
    /// <param name="firstLevelIndex">一级子元素索引</param>
    private void HideSecondLevelElements(int firstLevelIndex)
    {
        if (!secondLevelElements.ContainsKey(firstLevelIndex))
        {
            Debug.LogWarning("没有找到索引为 " + firstLevelIndex + " 的二级子元素列表");
            return;
        }
        
        List<Transform> elements = secondLevelElements[firstLevelIndex];
        
        // 播放退出动画并隐藏二级子元素
        for (int i = 0; i < elements.Count; i++)
        {
            Transform element = elements[i];
            
            // 获取CanvasGroup组件
            CanvasGroup canvasGroup = element.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                // 如果没有CanvasGroup组件，直接隐藏
                element.gameObject.SetActive(false);
                continue;
            }
            
            // 使用DOTween播放退出动画
            float delay = i * 0.03f; // 错开动画延迟
            
            element.DOScale(Vector3.zero, scaleDuration)
                .SetEase(scaleEase)
                .SetDelay(delay);
            
            canvasGroup.DOFade(0f, fadeDuration)
                .SetEase(fadeEase)
                .SetDelay(delay)
                .OnComplete(() => element.gameObject.SetActive(false));
        }
    }
}