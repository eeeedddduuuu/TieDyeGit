using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class AnimatedElement
{
    [Tooltip("要进行动画的UI元素")]
    public RectTransform targetElement;
    
    [Tooltip("此元素的动画延迟时间（秒）")]
    [Range(0f, 2f)]
    public float delay = 0f;
    
    [Tooltip("是否使用全局的持续时间和缓动类型")]
    public bool useGlobalSettings = true;
    
    [Tooltip("此元素的自定义动画持续时间（秒）")]
    [Range(0.1f, 3f)]
    public float customDuration = 1f;
    
    [Tooltip("此元素的自定义缓动类型")]
    public Ease customEaseType = Ease.OutQuad;
    
    // 存储原始位置
    [HideInInspector]
    public Vector2 originalAnchoredPosition;
    
    // 存储屏幕外位置
    [HideInInspector]
    public Vector2 offScreenPosition;
}

/// <summary>
    /// UI左侧滑入动画管理器
    /// 可以放置在场景中的管理器对象上，用于控制多个UI元素的滑入动画
    /// </summary>
    public class CanvasLeftSlide : MonoBehaviour
    {
        [Header("全局动画设置")]
        [Tooltip("默认动画持续时间（秒）")]
        [Range(0.1f, 3f)]
        public float globalAnimationDuration = 1f;
        
        [Tooltip("全局动画缓动类型")]
        public Ease globalEaseType = Ease.OutQuad;
        
        [Tooltip("初始位置的偏移量，相对于屏幕宽度的倍数")]
        [Range(1f, 3f)]
        public float initialOffsetMultiplier = 1.5f;
        
        [Tooltip("是否在启用时自动播放所有元素的动画")]
        public bool playOnEnable = true;
        
        [Tooltip("是否启用动画队列，按顺序播放元素动画")]
        public bool useAnimationQueue = false;
        
        [Tooltip("队列中元素之间的间隔时间（秒）")]
        [Range(0.05f, 0.5f)]
        public float queueSpacing = 0.1f;
    
    [Header("动画元素列表")]
    [Tooltip("要进行动画的UI元素列表")]
    public List<AnimatedElement> animatedElements = new List<AnimatedElement>();
    
    // 存储DOTween序列，用于管理动画队列
    private Sequence animationSequence;
    
    // 编辑器验证方法，确保Inspector设置有效
    private void OnValidate()
    {
        // 确保队列间隔时间合理
        queueSpacing = Mathf.Max(0.05f, queueSpacing);
        
        // 验证动画持续时间
        globalAnimationDuration = Mathf.Clamp(globalAnimationDuration, 0.1f, 3f);
        
        // 验证元素列表中的空引用
        for (int i = 0; i < animatedElements.Count; i++)
        {
            if (animatedElements[i].targetElement == null)
            {
                Debug.LogWarning($"[{name}] CanvasLeftSlide: 索引{i}的动画元素为null，请检查!");
            }
        }
    }
    
    private void Awake()
    {
        Debug.Log($"[{name}] CanvasLeftSlide: 初始化管理器");
        InitializeElements();
    }
    
    private void OnEnable()
    {
        if (playOnEnable)
        {
            Debug.Log($"[{name}] CanvasLeftSlide: 启用时自动播放动画");
            PlayAllSlideInAnimations();
        }
    }
    
    private void InitializeElements()
    {
        Debug.Log($"[{name}] CanvasLeftSlide: 初始化{animatedElements.Count}个动画元素");
        
        foreach (var element in animatedElements)
        {
            if (element.targetElement != null)
            {
                // 保存原始位置
                element.originalAnchoredPosition = element.targetElement.anchoredPosition;
                
                // 计算屏幕外的位置（从左侧滑入）
                Canvas canvas = element.targetElement.GetComponentInParent<Canvas>();
                if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    // 如果是Overlay模式，使用Screen.width
                    float offScreenX = -Screen.width * initialOffsetMultiplier;
                    element.offScreenPosition = new Vector2(offScreenX, element.originalAnchoredPosition.y);
                    Debug.Log($"[{name}] CanvasLeftSlide: 初始化元素 {element.targetElement.name} (Overlay模式)，目标位置: {element.originalAnchoredPosition}");
                }
                else
                {
                    // 其他情况下，使用RectTransform的宽度作为参考
                    float offScreenX = -element.targetElement.rect.width * initialOffsetMultiplier;
                    element.offScreenPosition = new Vector2(element.originalAnchoredPosition.x + offScreenX, element.originalAnchoredPosition.y);
                    Debug.Log($"[{name}] CanvasLeftSlide: 初始化元素 {element.targetElement.name}，目标位置: {element.originalAnchoredPosition}");
                }
                
                // 初始化时设置到屏幕外位置
                element.targetElement.anchoredPosition = element.offScreenPosition;
            }
            else
            {
                Debug.LogWarning($"[{name}] CanvasLeftSlide: AnimatedElement中的targetElement为null，请检查！");
            }
        }
    }
    
    /// <summary>
    /// 播放所有元素的滑入动画
    /// </summary>
    public void PlayAllSlideInAnimations()
    {
        Debug.Log($"[{name}] CanvasLeftSlide: 播放所有元素滑入动画，模式: {(useAnimationQueue ? "队列" : "并行")}");
        
        if (useAnimationQueue)
        {
            PlayAnimationsInQueue();
        }
        else
        {
            PlayAnimationsSimultaneously();
        }
    }
    
    /// <summary>
    /// 同时播放所有元素的动画
    /// </summary>
    private void PlayAnimationsSimultaneously()
    {
        Debug.Log($"[{name}] CanvasLeftSlide: 同时播放{animatedElements.Count}个元素动画");
        
        foreach (var element in animatedElements)
        {
            PlayElementAnimation(element);
        }
    }
    
    /// <summary>
    /// 按顺序播放元素动画队列
    /// </summary>
    private void PlayAnimationsInQueue()
    {
        Debug.Log($"[{name}] CanvasLeftSlide: 创建动画队列，包含{animatedElements.Count}个元素，间隔: {queueSpacing}s");
        
        // 清除之前的序列
        if (animationSequence != null)
        {
            animationSequence.Kill();
        }
        
        // 创建新的序列
        animationSequence = DOTween.Sequence();
        
        float currentDelay = 0f;
        
        foreach (var element in animatedElements)
        {
            if (element.targetElement != null)
            {
                // 添加元素动画到序列中
                float duration = element.useGlobalSettings ? globalAnimationDuration : element.customDuration;
                Ease easeType = element.useGlobalSettings ? globalEaseType : element.customEaseType;
                
                // 添加延迟（元素自身的延迟 + 队列间隔）
                currentDelay += element.delay;
                
                // 保存局部变量，避免闭包问题
                AnimatedElement currentElement = element;
                float currentDuration = duration;
                Ease currentEaseType = easeType;
                
                animationSequence.AppendCallback(() =>
                {
                    Debug.Log($"[{name}] CanvasLeftSlide: 队列播放元素 {currentElement.targetElement.name} 动画");
                    
                    // 先设置到屏幕外位置
                    currentElement.targetElement.anchoredPosition = currentElement.offScreenPosition;
                    
                    // 执行滑入动画
                    currentElement.targetElement.DOAnchorPos(currentElement.originalAnchoredPosition, currentDuration)
                        .SetEase(currentEaseType)
                        .SetUpdate(true) // 确保在Time.timeScale为0时也能播放
                        .SetLink(currentElement.targetElement.gameObject) // 链接到对象，当对象销毁时自动停止动画
                        .OnComplete(() => {
                            Debug.Log($"[{name}] CanvasLeftSlide: 元素 {currentElement.targetElement.name} 动画完成");
                        });
                });
                
                // 添加队列间隔
                animationSequence.AppendInterval(duration + queueSpacing);
            }
        }
        
        // 添加序列完成回调
        animationSequence.OnComplete(() => {
            Debug.Log($"[{name}] CanvasLeftSlide: 动画队列播放完成");
        });
        
        // 开始序列动画
        animationSequence.Play();
    }
    
    /// <summary>
    /// 播放单个元素的动画
    /// </summary>
    /// <param name="element">要播放动画的元素</param>
    private void PlayElementAnimation(AnimatedElement element)
    {
        if (element.targetElement == null)
        {
            Debug.LogError($"[{name}] CanvasLeftSlide: 尝试播放动画的元素为null！");
            return;
        }
        
        float duration = element.useGlobalSettings ? globalAnimationDuration : element.customDuration;
        Ease easeType = element.useGlobalSettings ? globalEaseType : element.customEaseType;
        
        Debug.Log($"[{name}] CanvasLeftSlide: 播放元素 {element.targetElement.name} 动画 - 时长: {duration}s, 缓动: {easeType}, 延迟: {element.delay}s");
        
        // 先设置到屏幕外位置
        element.targetElement.anchoredPosition = element.offScreenPosition;
        
        // 执行滑入动画，带延迟
        element.targetElement.DOAnchorPos(element.originalAnchoredPosition, duration)
            .SetEase(easeType)
            .SetDelay(element.delay)
            .SetUpdate(true) // 确保在Time.timeScale为0时也能播放
            .SetLink(element.targetElement.gameObject)
            .OnComplete(() => {
                Debug.Log($"[{name}] CanvasLeftSlide: 元素 {element.targetElement.name} 动画完成");
            });
    }
    
    /// <summary>
    /// 重置所有元素到初始位置（屏幕外）
    /// </summary>
    public void ResetAllElements()
    {
        Debug.Log($"[{name}] CanvasLeftSlide: 重置所有元素到初始位置");
        
        // 停止所有动画
        if (animationSequence != null)
        {
            animationSequence.Kill();
        }
        
        foreach (var element in animatedElements)
        {
            if (element.targetElement != null)
            {
                element.targetElement.anchoredPosition = element.offScreenPosition;
                Debug.Log($"[{name}] CanvasLeftSlide: 元素 {element.targetElement.name} 已重置");
            }
        }
    }
    
    /// <summary>
    /// 立即显示所有元素（无动画）
    /// </summary>
    public void ShowAllElementsImmediately()
    {
        Debug.Log($"[{name}] CanvasLeftSlide: 立即显示所有元素");
        
        // 停止所有动画
        if (animationSequence != null)
        {
            animationSequence.Kill();
        }
        
        foreach (var element in animatedElements)
        {
            if (element.targetElement != null)
            {
                element.targetElement.anchoredPosition = element.originalAnchoredPosition;
                Debug.Log($"[{name}] CanvasLeftSlide: 元素 {element.targetElement.name} 已显示");
            }
        }
    }
    
    /// <summary>
    /// 添加新的动画元素
    /// </summary>
    /// <param name="rectTransform">要添加的UI元素的RectTransform</param>
    public void AddElement(RectTransform rectTransform)
    {
        Debug.Log($"[{name}] CanvasLeftSlide: 添加新元素 {rectTransform?.name}");
        
        AnimatedElement newElement = new AnimatedElement
        {
            targetElement = rectTransform
        };
        
        animatedElements.Add(newElement);
        
        // 初始化新元素
        if (rectTransform != null)
        {            // 保存原始位置
            newElement.originalAnchoredPosition = rectTransform.anchoredPosition;
            
            // 计算屏幕外的位置
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                float offScreenX = -Screen.width * initialOffsetMultiplier;
                newElement.offScreenPosition = new Vector2(offScreenX, newElement.originalAnchoredPosition.y);
                Debug.Log($"[{name}] CanvasLeftSlide: 初始化新元素 {rectTransform.name} (Overlay模式)");
            }
            else
            {
                float offScreenX = -rectTransform.rect.width * initialOffsetMultiplier;
                newElement.offScreenPosition = new Vector2(newElement.originalAnchoredPosition.x + offScreenX, newElement.originalAnchoredPosition.y);
                Debug.Log($"[{name}] CanvasLeftSlide: 初始化新元素 {rectTransform.name}");
            }
            
            // 设置到屏幕外位置
            rectTransform.anchoredPosition = newElement.offScreenPosition;
        }
    }
    
    /// <summary>
    /// 移除指定的动画元素
    /// </summary>
    /// <param name="rectTransform">要移除的UI元素的RectTransform</param>
    public void RemoveElement(RectTransform rectTransform)
    {        Debug.Log($"[{name}] CanvasLeftSlide: 尝试移除元素 {rectTransform?.name}");
        
        for (int i = animatedElements.Count - 1; i >= 0; i--)
        {
            if (animatedElements[i].targetElement == rectTransform)
            {
                animatedElements.RemoveAt(i);
                Debug.Log($"[{name}] CanvasLeftSlide: 元素已成功移除");
                break;
            }
        }
    }
    
    /// <summary>
    /// 停止所有正在播放的动画
    /// </summary>
    public void StopAllAnimations()
    {        Debug.Log($"[{name}] CanvasLeftSlide: 停止所有动画");
        
        if (animationSequence != null)
        {
            animationSequence.Kill();
        }
        
        foreach (var element in animatedElements)
        {
            if (element.targetElement != null)
            {
                element.targetElement.DOKill();
                Debug.Log($"[{name}] CanvasLeftSlide: 已停止元素 {element.targetElement.name} 的动画");
            }
        }
    }
}