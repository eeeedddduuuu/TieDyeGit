using UnityEngine;
using System.Collections;

public class DyeProductUIManager : MonoBehaviour
{
    // 引用需要控制的UI组件
    [SerializeField] private Transform bucketTransform;
    [SerializeField] private Transform banlanTransform;
    [SerializeField] private Transform limeTransform;
    [SerializeField] private Transform rawmaterialsIntroduceTransform;

    // 可调节的动画参数
    [Header("动画参数")]
    [SerializeField] private float bucketMoveDistance = -400f;
    [SerializeField] private float bucketMoveDuration = 1f;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float delayBetweenElements = 0.5f;

    private void Start()
    {
        // 初始化时确保所有元素不可见（除了bucket）
        InitializeUIElements();

        // 开始播放动画
        StartCoroutine(PlaySceneAnimations());
    }

    private void InitializeUIElements()
    {
        // 设置初始透明度为0，确保在bucket移动结束前不可见
        SetElementAlpha(banlanTransform, 0f);
        SetElementAlpha(limeTransform, 0f);
        SetElementAlpha(rawmaterialsIntroduceTransform, 0f);

        // 可选：禁用交互，避免在不可见时被点击
        SetElementsInteractable(false);
    }

    private void SetElementsInteractable(bool interactable)
    {
        SetComponentInteractable(banlanTransform, interactable);
        SetComponentInteractable(limeTransform, interactable);
        SetComponentInteractable(rawmaterialsIntroduceTransform, interactable);
    }

    private void SetComponentInteractable(Transform elementTransform, bool interactable)
    {
        if (elementTransform == null) return;

        // 对于CanvasGroup组件
        CanvasGroup canvasGroup = elementTransform.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = interactable;
        }

        // 对于Button等UI组件
        UnityEngine.UI.Button button = elementTransform.GetComponent<UnityEngine.UI.Button>();
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    private IEnumerator PlaySceneAnimations()
    {
        // 先执行Bucket的平移动画
        yield return StartCoroutine(MoveBucket());

        // 启用其他元素的交互
        SetElementsInteractable(true);

        // 然后执行其他UI元素的顺序渐显动画
        yield return StartCoroutine(SequenceFadeInElements());
    }

    // Bucket向左平移的动画（距离可调节）
    private IEnumerator MoveBucket()
    {
        if (bucketTransform == null) yield break;

        Vector3 startPosition = bucketTransform.position;
        Vector3 targetPosition = startPosition + new Vector3(bucketMoveDistance, 0f, 0f);

        float elapsedTime = 0f;

        while (elapsedTime < bucketMoveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / bucketMoveDuration);

            // 使用平滑的插值函数
            t = Mathf.SmoothStep(0f, 1f, t);

            bucketTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        // 确保最终位置准确
        bucketTransform.position = targetPosition;
    }

    // 顺序渐显动画
    private IEnumerator SequenceFadeInElements()
    {
        // Banlan渐显
        yield return StartCoroutine(FadeInElement(banlanTransform));

        // 等待延迟
        yield return new WaitForSeconds(delayBetweenElements);

        // Lime渐显
        yield return StartCoroutine(FadeInElement(limeTransform));

        // 等待延迟
        yield return new WaitForSeconds(delayBetweenElements);

        // RawmaterialsIntroduce渐显
        yield return StartCoroutine(FadeInElement(rawmaterialsIntroduceTransform));
    }

    // 单个元素渐显动画
    private IEnumerator FadeInElement(Transform elementTransform)
    {
        if (elementTransform == null) yield break;

        // 获取所有Image和Text组件
        CanvasRenderer[] renderers = elementTransform.GetComponentsInChildren<CanvasRenderer>(true);

        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);

            // 使用平滑的透明度变化
            alpha = Mathf.SmoothStep(0f, 1f, alpha);

            SetElementAlpha(elementTransform, alpha);
            yield return null;
        }

        // 确保最终透明度为1
        SetElementAlpha(elementTransform, 1f);
    }

    // 设置元素及其子元素的透明度
    private void SetElementAlpha(Transform elementTransform, float alpha)
    {
        if (elementTransform == null) return;

        CanvasRenderer[] renderers = elementTransform.GetComponentsInChildren<CanvasRenderer>(true);
        foreach (CanvasRenderer renderer in renderers)
        {
            renderer.SetAlpha(alpha);
        }

        // 同时设置CanvasGroup的alpha（如果有的话）
        CanvasGroup canvasGroup = elementTransform.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }

    // 可选：重置动画的方法
    public void ResetAnimations()
    {
        StopAllCoroutines();
        InitializeUIElements();

        // 重置bucket位置
        if (bucketTransform != null)
        {
            // 这里需要知道bucket的初始位置，您可以在Awake中保存初始位置
            // 或者根据您的需求调整
        }
    }

    // 可选：重新播放动画的方法
    public void ReplayAnimations()
    {
        ResetAnimations();
        StartCoroutine(PlaySceneAnimations());
    }
}