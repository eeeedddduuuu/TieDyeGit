using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Canvas子元素动画控制器
/// 用于控制Canvas中元素随Canvas出现时的动画效果
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CanvasSubelementAnimation : MonoBehaviour
{
    /// <summary>
    /// 动画类型枚举
    /// </summary>
    public enum AnimationType
    {
        ScaleAndFade,      // 缩放+淡入
        FadeOnly,          // 仅淡入
        ScaleOnly,         // 仅缩放
        SlideAndFade,      // 滑动+淡入
        FullAnimation      // 完整动画(缩放+淡入+滑动)
    }
    
    // 基础动画设置
    [Header("=== 基础动画设置 ===")]
    [Tooltip("动画类型选择")]
    public AnimationType animationType = AnimationType.ScaleAndFade;
    
    [Tooltip("是否自动播放动画")]
    public bool autoPlayOnEnable = true;
    
    [Tooltip("是否使用错开动画")]
    public bool useStaggeredAnimation = true;
    
    [Tooltip("错开动画的延迟时间")]
    [Range(0.01f, 0.5f)]
    public float staggerDelay = 0.05f;
    
    // 动画时间设置
    [Header("=== 动画时间设置 ===")]
    [Tooltip("动画持续时间")]
    [Range(0.1f, 3f)]
    public float animationDuration = 0.5f;
    
    [Tooltip("动画起始延迟时间")]
    [Range(0f, 1f)]
    public float startDelay = 0f;
    
    // 缓动函数设置
    [Header("=== 缓动函数设置 ===")]
    [Tooltip("缩放动画缓动函数")]
    public Ease scaleEase = Ease.OutBack;
    
    [Tooltip("淡入动画缓动函数")]
    public Ease fadeEase = Ease.Linear;
    
    [Tooltip("滑动动画缓动函数")]
    public Ease slideEase = Ease.OutQuad;
    
    // 缩放动画设置
    [Header("=== 缩放动画设置 ===")]
    [Tooltip("初始缩放比例")]
    [Range(0f, 1.5f)]
    public float scaleFrom = 0.8f;
    
    [Tooltip("是否对X、Y轴使用不同的缩放值")]
    public bool useDifferentAxisScales = false;
    
    [Tooltip("X轴初始缩放比例(当useDifferentAxisScales为true时生效)")]
    [Range(0f, 2f)]
    public float scaleFromX = 0.8f;
    
    [Tooltip("Y轴初始缩放比例(当useDifferentAxisScales为true时生效)")]
    [Range(0f, 2f)]
    public float scaleFromY = 0.8f;
    
    // 淡入动画设置
    [Header("=== 淡入动画设置 ===")]
    [Tooltip("初始透明度")]
    [Range(0f, 1f)]
    public float fadeFrom = 0f;
    
    // 滑动动画设置
    [Header("=== 滑动动画设置 ===")]
    [Tooltip("滑动方向")]
    public Vector2 slideDirection = new Vector2(0, 50f); // 默认向上滑入
    
    [Tooltip("滑动距离")]
    [Range(0f, 500f)]
    public float slideDistance = 50f;
    
    // 元素选择设置
    [Header("=== 元素选择设置 ===")]
    [Tooltip("手动指定的需要动画的元素")]
    public List<RectTransform> elementsToAnimate = new List<RectTransform>();
    
    [Tooltip("是否包含所有子元素")]
    public bool includeAllChildElements = false;
    
    [Tooltip("是否递归查找所有子元素")]
    public bool recursiveSearch = true;
    
    [Tooltip("是否排除某些指定元素")]
    public List<RectTransform> excludeElements = new List<RectTransform>();
    
    // 内部状态
    private bool hasPlayedAnimation = false;
    private CanvasGroup canvasGroup;
    private Canvas _canvas;
    private RectTransform _rectTransform;
    
    private void Awake()
    {        
        // 获取并缓存组件引用
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponent<Canvas>();
        
        if (_canvas == null)
        {            
            Debug.LogWarning("警告：CanvasSubelementAnimation脚本未挂载在Canvas组件上。脚本仍然可以工作，但可能无法检测Canvas的渲染状态。", this);
        }
        // 获取或添加CanvasGroup组件
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // 初始化时确保元素状态正确
        InitializeElements();
    }
    
    private void OnEnable()
    {
        // 重置动画状态
        hasPlayedAnimation = false;
        InitializeElements();
        
        // 启动协程检查Canvas的可见性变化
        StartCoroutine(CheckCanvasVisibility());
        
        // 如果启用自动播放，则播放动画
        if (autoPlayOnEnable)
        {
            // 添加一帧延迟，确保Canvas完全启用
            DOVirtual.DelayedCall(0.05f, () => {
                PlayAnimation();
            });
        }
    }
    
    private void OnTransformParentChanged()
    {
        // 当父对象改变时，可能意味着Canvas的层级或可见性发生变化
        ResetAnimation();
        
        if (autoPlayOnEnable && gameObject.activeInHierarchy)
        {
            DOVirtual.DelayedCall(0.1f, () => {
                PlayAnimation();
            });
        }
    }
    
    private void OnCanvasGroupChanged()
    {
        // 监听CanvasGroup的alpha变化，如果alpha从0变为大于0，重新播放动画
        if (canvasGroup != null && canvasGroup.alpha > 0 && !hasPlayedAnimation)
        {
            PlayAnimation();
        }
    }
    
    /// <summary>
    /// 检查Canvas可见性的协程
    /// </summary>
    private System.Collections.IEnumerator CheckCanvasVisibility()
    {
        // 等待几帧确保Canvas和所有组件都已正确初始化
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // 检查Canvas是否可见
        Canvas canvas = GetComponent<Canvas>();
        bool wasVisible = IsCanvasVisible();
        
        // 持续检查可见性变化
        while (true)
        {
            yield return new WaitForSeconds(0.1f); // 每0.1秒检查一次
            
            bool isVisibleNow = IsCanvasVisible();
            
            // 如果Canvas从不可见变为可见，重新播放动画
            if (!wasVisible && isVisibleNow && autoPlayOnEnable)
            {
                ResetAnimation();
                DOVirtual.DelayedCall(0.05f, () => {
                    PlayAnimation();
                });
            }
            
            wasVisible = isVisibleNow;
        }
    }
    
    /// <summary>
    /// 检查Canvas是否可见
    /// </summary>
    private bool IsCanvasVisible()
    {
        // 检查GameObject是否激活
        if (!gameObject.activeInHierarchy)
            return false;
        
        // 检查CanvasGroup的alpha
        if (canvasGroup != null && canvasGroup.alpha <= 0)
            return false;
        
        // 检查Canvas组件
        if (_canvas != null)
        {            
            if (!_canvas.enabled)
                return false;
                
            // 对于Screen Space - Camera模式，简单检查Camera是否启用
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera && _canvas.worldCamera != null)
            {
                return _canvas.worldCamera.enabled;
            }
            
            // 检查所有父级是否都激活
            Transform parent = transform.parent;
            while (parent != null)
            {                
                if (!parent.gameObject.activeSelf)
                    return false;
                parent = parent.parent;
            }
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 初始化所有需要动画的元素
    /// </summary>
    private void InitializeElements()
    {
        // 如果设置为包含所有子元素，则获取所有子元素
        if (includeAllChildElements)
        {
            elementsToAnimate.Clear();
            RectTransform[] allChildren = GetComponentsInChildren<RectTransform>(recursiveSearch);
            
            foreach (RectTransform child in allChildren)
            {
                // 排除自身和在排除列表中的元素
                if (child != transform && !excludeElements.Contains(child))
                {
                    elementsToAnimate.Add(child);
                }
            }
        }
        else
        {
            // 从手动指定的元素列表中移除在排除列表中的元素
            for (int i = elementsToAnimate.Count - 1; i >= 0; i--)
            {
                if (excludeElements.Contains(elementsToAnimate[i]))
                {
                    elementsToAnimate.RemoveAt(i);
                }
            }
        }
        
        // 初始化所有元素状态
        foreach (RectTransform element in elementsToAnimate)
        {
            if (element != null)
            {
                // 保存原始状态
                SaveOriginalState(element);
                
                // 设置初始动画状态
                SetInitialAnimationState(element);
            }
        }
    }
    
    /// <summary>
    /// 保存元素的原始状态
    /// </summary>
    private void SaveOriginalState(RectTransform element)
    {
        // 原始状态会在动画过程中自动恢复，这里只是确保状态正确
    }
    
    /// <summary>
    /// 设置元素的初始动画状态
    /// </summary>
    private void SetInitialAnimationState(RectTransform element)
    {
        if (element != null)
        {
            // 根据动画类型设置初始状态
            switch (animationType)
            {
                case AnimationType.ScaleAndFade:
                case AnimationType.ScaleOnly:
                case AnimationType.FullAnimation:
                    // 设置初始缩放
                    if (useDifferentAxisScales)
                    {
                        element.localScale = new Vector3(scaleFromX, scaleFromY, 1f);
                    }
                    else
                    {
                        element.localScale = Vector3.one * scaleFrom;
                    }
                    break;
                    
                case AnimationType.SlideAndFade:
                case AnimationType.FadeOnly:
                    // 这些动画类型不需要改变初始缩放
                    element.localScale = Vector3.one;
                    break;
            }
            
            // 设置初始位置（用于滑动动画）
            if (animationType == AnimationType.SlideAndFade || animationType == AnimationType.FullAnimation)
            {
                Vector2 normalizedDirection = slideDirection.normalized;
                Vector3 slideOffset = new Vector3(
                    normalizedDirection.x * slideDistance,
                    normalizedDirection.y * slideDistance,
                    0f);
                element.localPosition += slideOffset;
            }
            
            // 为元素添加CanvasGroup组件（如果需要淡入动画）
            if (animationType == AnimationType.ScaleAndFade || 
                animationType == AnimationType.FadeOnly || 
                animationType == AnimationType.SlideAndFade || 
                animationType == AnimationType.FullAnimation)
            {
                CanvasGroup elementCanvasGroup = element.GetComponent<CanvasGroup>();
                if (elementCanvasGroup == null)
                {
                    elementCanvasGroup = element.gameObject.AddComponent<CanvasGroup>();
                    elementCanvasGroup.blocksRaycasts = true;
                    elementCanvasGroup.interactable = true;
                }
                elementCanvasGroup.alpha = fadeFrom;
            }
        }
    }
    
    /// <summary>
    /// 播放动画
    /// </summary>
    public void PlayAnimation()
    {        if (hasPlayedAnimation) return;
        
        hasPlayedAnimation = true;
        
        // 根据是否使用错开动画来决定播放方式
        if (useStaggeredAnimation)
        {
            PlayStaggeredAnimation();
        }
        else
        {
            PlaySimultaneousAnimation();
        }
    }
    
    /// <summary>
    /// 同时播放所有元素的动画
    /// </summary>
    private void PlaySimultaneousAnimation()
    {
        foreach (RectTransform element in elementsToAnimate)
        {
            if (element != null)
            {
                AnimateElement(element, 0f);
            }
        }
    }
    
    /// <summary>
    /// 错开播放所有元素的动画
    /// </summary>
    private void PlayStaggeredAnimation()
    {
        for (int i = 0; i < elementsToAnimate.Count; i++)
        {
            RectTransform element = elementsToAnimate[i];
            if (element != null)
            {
                // 计算延迟时间
                float delay = i * staggerDelay;
                AnimateElement(element, delay);
            }
        }
    }
    
    /// <summary>
    /// 执行单个元素的动画
    /// </summary>
    private void AnimateElement(RectTransform element, float delay)
    {
        if (element == null) return;
        
        // 总延迟时间（起始延迟 + 错开延迟）
        float totalDelay = startDelay + delay;
        
        // 创建动画序列
        Sequence sequence = DOTween.Sequence().SetDelay(totalDelay);
        
        // 根据动画类型添加相应的动画
        switch (animationType)
        {
            case AnimationType.ScaleAndFade:
                // 缩放动画
                sequence.Join(element.DOScale(Vector3.one, animationDuration).SetEase(scaleEase));
                // 淡入动画
                AddFadeAnimation(element, sequence, animationDuration);
                break;
                
            case AnimationType.FadeOnly:
                // 仅淡入动画
                AddFadeAnimation(element, sequence, animationDuration);
                break;
                
            case AnimationType.ScaleOnly:
                // 仅缩放动画
                sequence.Join(element.DOScale(Vector3.one, animationDuration).SetEase(scaleEase));
                break;
                
            case AnimationType.SlideAndFade:
                // 滑动动画
                AddSlideAnimation(element, sequence, animationDuration);
                // 淡入动画
                AddFadeAnimation(element, sequence, animationDuration);
                break;
                
            case AnimationType.FullAnimation:
                // 缩放动画
                sequence.Join(element.DOScale(Vector3.one, animationDuration).SetEase(scaleEase));
                // 滑动动画
                AddSlideAnimation(element, sequence, animationDuration);
                // 淡入动画
                AddFadeAnimation(element, sequence, animationDuration);
                break;
        }
        
        // 动画完成回调
        sequence.OnComplete(() => {
            Debug.Log($"元素动画完成: {element.name}");
        });
    }
    
    /// <summary>
    /// 添加淡入动画到序列
    /// </summary>
    private void AddFadeAnimation(RectTransform element, Sequence sequence, float duration)
    {
        CanvasGroup elementCanvasGroup = element.GetComponent<CanvasGroup>();
        if (elementCanvasGroup == null)
        {
            elementCanvasGroup = element.gameObject.AddComponent<CanvasGroup>();
            elementCanvasGroup.blocksRaycasts = true;
            elementCanvasGroup.interactable = true;
            elementCanvasGroup.alpha = fadeFrom;
        }
        
        sequence.Join(elementCanvasGroup.DOFade(1f, duration).SetEase(fadeEase));
    }
    
    /// <summary>
    /// 添加滑动动画到序列
    /// </summary>
    private void AddSlideAnimation(RectTransform element, Sequence sequence, float duration)
    {
        // 计算目标位置（恢复到初始位置）
        Vector2 normalizedDirection = slideDirection.normalized;
        Vector3 slideOffset = new Vector3(
            -normalizedDirection.x * slideDistance, // 负号表示回到原位
            -normalizedDirection.y * slideDistance,
            0f);
        
        sequence.Join(element.DOLocalMove(element.localPosition + slideOffset, duration).SetEase(slideEase));
    }
    
    /// <summary>
    /// 重置动画状态，可用于重新播放动画
    /// </summary>
    public void ResetAnimation()
    {
        // 停止所有动画
        foreach (RectTransform element in elementsToAnimate)
        {
            if (element != null)
            {
                element.DOKill(true);
                CanvasGroup elementCanvasGroup = element.GetComponent<CanvasGroup>();
                if (elementCanvasGroup != null)
                {
                    elementCanvasGroup.DOKill(true);
                }
            }
        }
        
        // 重置状态
        hasPlayedAnimation = false;
        InitializeElements();
        
        Debug.Log("动画已重置", this);
    }
    
    /// <summary>
    /// 重新播放动画
    /// </summary>
    public void ReplayAnimation()
    {
        if (!IsCanvasVisible())
        {            
            Debug.LogWarning("无法重播动画：Canvas当前不可见", this);
            return;
        }
        
        ResetAnimation();
        PlayAnimation();
    }
    
    /// <summary>
    /// 检查元素列表是否有效
    /// </summary>
    private void OnValidate()
    {
        // 编辑器中验证元素列表
        if (elementsToAnimate.Count > 0)
        {            
            // 清理null引用
            for (int i = elementsToAnimate.Count - 1; i >= 0; i--)
            {
                if (elementsToAnimate[i] == null)
                {
                    elementsToAnimate.RemoveAt(i);
                }
            }
        }
        
        // 确保延迟值不为负
        startDelay = Mathf.Max(0f, startDelay);
        staggerDelay = Mathf.Max(0f, staggerDelay);
        
        // 确保动画时长不为负
        animationDuration = Mathf.Max(0.01f, animationDuration);
        
        // 确保缩放值合理
        scaleFrom = Mathf.Max(0.01f, scaleFrom);
        scaleFromX = Mathf.Max(0.01f, scaleFromX);
        scaleFromY = Mathf.Max(0.01f, scaleFromY);
        
        // 确保滑动距离不为负
        slideDistance = Mathf.Max(0f, slideDistance);
    }
}