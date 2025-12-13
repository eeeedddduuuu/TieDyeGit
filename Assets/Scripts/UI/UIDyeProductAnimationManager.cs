using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// DyeProduct场景UI动画管理器
/// 控制场景中各UI组件的动画效果
/// </summary>
public class UIDyeProductAnimationManager : MonoBehaviour
{
    [Header("UI组件引用")]
    [Tooltip("水桶组件")]
    public RectTransform bucketTransform;
    [Tooltip("板蓝根组件")]
    public RectTransform banlanTransform;
    [Tooltip("石灰组件")]
    public RectTransform limeTransform;
    [Tooltip("原材料介绍组件")]
    public RectTransform rawmaterialsIntroduceTransform;

    [Header("动画参数")]
    [Tooltip("水桶平移距离（单位）")]
    public float bucketTranslateDistance = -400f;
    [Tooltip("水桶平移时长（秒）")]
    public float bucketTranslateDuration = 1f;
    [Tooltip("渐显动画时长（秒）")]
    public float fadeInDuration = 1f;
    [Tooltip("组件间渐显延迟（秒）")]
    public float fadeInDelayBetweenComponents = 0.5f;

    [Header("动画配置")]
    [Tooltip("是否自动播放动画")]
    public bool autoPlay = true;
    [Tooltip("动画缓动函数")]
    public Ease moveEase = Ease.OutQuad;
    [Tooltip("渐显缓动函数")]
    public Ease fadeEase = Ease.OutQuad;

    private Image[] banlanImages;
    private Image[] limeImages;
    private Image[] rawmaterialsImages;

    private void Start()
    {
        // 初始化组件引用
        InitializeComponents();

        // 设置初始状态
        SetInitialState();

        // 自动播放动画
        if (autoPlay)
        {
            PlayAnimations();
        }
    }

    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void InitializeComponents()
    {
        // 获取各组件的Image组件用于控制透明度
        if (banlanTransform != null)
        {
            banlanImages = banlanTransform.GetComponentsInChildren<Image>(true);
        }

        if (limeTransform != null)
        {
            limeImages = limeTransform.GetComponentsInChildren<Image>(true);
        }

        if (rawmaterialsIntroduceTransform != null)
        {
            rawmaterialsImages = rawmaterialsIntroduceTransform.GetComponentsInChildren<Image>(true);
        }
    }

    /// <summary>
    /// 设置组件初始状态
    /// </summary>
    private void SetInitialState()
    {
        // 确保所有渐显组件初始时透明且不可见
        SetComponentsVisibility(banlanImages, false);
        SetComponentsVisibility(limeImages, false);
        SetComponentsVisibility(rawmaterialsImages, false);
    }

    /// <summary>
    /// 播放所有动画
    /// </summary>
    public void PlayAnimations()
    {
        // 播放水桶平移动画
        PlayBucketAnimation();

        // 计算延迟时间，确保水桶移动结束前其他组件不可见
        float startDelay = bucketTranslateDuration;

        // 按顺序播放其他组件的渐显动画
        PlayBanlanAnimation(startDelay);
        PlayLimeAnimation(startDelay + fadeInDelayBetweenComponents);
        PlayRawmaterialsAnimation(startDelay + fadeInDelayBetweenComponents * 2);
    }

    /// <summary>
    /// 播放水桶平移动画
    /// </summary>
    private void PlayBucketAnimation()
    {
        if (bucketTransform != null)
        {
            Vector3 targetPosition = bucketTransform.localPosition + new Vector3(bucketTranslateDistance, 0, 0);
            bucketTransform.DOLocalMove(targetPosition, bucketTranslateDuration)
                .SetEase(moveEase);
        }
    }

    /// <summary>
    /// 播放板蓝根渐显动画
    /// </summary>
    private void PlayBanlanAnimation(float delay)
    {
        if (banlanTransform != null)
        {
            // 先设置为可见
            SetComponentsVisibility(banlanImages, true);
            
            // 使用DOTween进行渐显和位移动画
            banlanTransform.DOAnchorPosX(0, fadeInDuration)
                .SetDelay(delay)
                .SetEase(fadeEase);

            // 为所有子图片组件添加渐显效果
            foreach (var image in banlanImages)
            {
                image.DOFade(1, fadeInDuration)
                    .SetDelay(delay)
                    .SetEase(fadeEase);
            }
        }
    }

    /// <summary>
    /// 播放石灰渐显动画
    /// </summary>
    private void PlayLimeAnimation(float delay)
    {
        if (limeTransform != null)
        {
            // 先设置为可见
            SetComponentsVisibility(limeImages, true);
            
            // 使用DOTween进行渐显和位移动画
            limeTransform.DOAnchorPosX(0, fadeInDuration)
                .SetDelay(delay)
                .SetEase(fadeEase);

            // 为所有子图片组件添加渐显效果
            foreach (var image in limeImages)
            {
                image.DOFade(1, fadeInDuration)
                    .SetDelay(delay)
                    .SetEase(fadeEase);
            }
        }
    }

    /// <summary>
    /// 播放原材料介绍渐显动画
    /// </summary>
    private void PlayRawmaterialsAnimation(float delay)
    {
        if (rawmaterialsIntroduceTransform != null)
        {
            // 先设置为可见
            SetComponentsVisibility(rawmaterialsImages, true);
            
            // 使用DOTween进行渐显和位移动画
            rawmaterialsIntroduceTransform.DOAnchorPosX(0, fadeInDuration)
                .SetDelay(delay)
                .SetEase(fadeEase);

            // 为所有子图片组件添加渐显效果
            foreach (var image in rawmaterialsImages)
            {
                image.DOFade(1, fadeInDuration)
                    .SetDelay(delay)
                    .SetEase(fadeEase);
            }
        }
    }

    /// <summary>
    /// 设置组件可见性
    /// </summary>
    private void SetComponentsVisibility(Image[] images, bool visible)
    {
        if (images == null)
            return;

        foreach (var image in images)
        {
            if (image != null)
            {
                image.gameObject.SetActive(visible);
                image.color = new Color(image.color.r, image.color.g, image.color.b, visible ? 0 : image.color.a);
            }
        }
    }

    /// <summary>
    /// 重置所有动画状态
    /// </summary>
    public void ResetAnimations()
    {
        // 取消所有正在进行的动画
        DOTween.KillAll();

        // 重置所有组件到初始状态
        ResetTransforms();
        SetInitialState();
    }

    /// <summary>
    /// 重置所有变换组件
    /// </summary>
    private void ResetTransforms()
    {
        // 这里可以根据需要添加重置逻辑
        // 目前假设组件在Inspector中已经设置好了初始位置
    }
}
