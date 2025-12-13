using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ImageScaleAnimation : MonoBehaviour
{
    [Header("动画设置")]
    public float startDelay = 1f; // 开始延迟
    public float animationDuration = 0.5f; // 动画持续时间
    public Ease easeType = Ease.OutBack; // 缓动类型

    private Image targetImage;
    private Vector3 originalScale;

    void Start()
    {
        // 获取Image组件
        targetImage = GetComponent<Image>();

        // 保存原始缩放
        originalScale = transform.localScale;

        // 初始状态：完全透明且缩放为0
        transform.localScale = Vector3.zero;

        // 延迟后开始动画
        Invoke("StartScaleAnimation", startDelay);
    }

    void StartScaleAnimation()
    {
        // 使用DOTween实现缩放动画
        transform.DOScale(originalScale, animationDuration)
                 .SetEase(easeType)
                 .OnComplete(() => {
                     // 动画完成后的回调
                     Debug.Log("图片放大动画完成！");
                 });
    }

    // 可选：公共方法用于在其他地方触发动画
    public void PlayAnimation()
    {
        transform.localScale = Vector3.zero;
        StartScaleAnimation();
    }

    // 可选：重置动画
    public void ResetAnimation()
    {
        transform.localScale = Vector3.zero;
    }
}