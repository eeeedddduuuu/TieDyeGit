using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

/// <summary>
/// 设计界面动画控制器
/// 控制PatternSelectionPanel、Toolbar、DesignArea和IntroduceArea的平移动画
/// 支持第一次和第二次点击nextstepbutton的不同动画效果
/// </summary>
public class DesignUIAnimation : MonoBehaviour
{
    [Header("组件引用")]
    [Tooltip("要向左移动的PatternSelectionPanel")]
    public RectTransform patternSelectionPanel;
    
    [Tooltip("要向右移动的Toolbar")]
    public RectTransform toolbar;
    
    [Tooltip("要向左移动的DesignArea")]
    public RectTransform designArea;
    
    [Tooltip("要向左移动的IntroduceArea")]
    public RectTransform introduceArea;
    
    [Tooltip("第一次点击时出现的IntroduceText1")]
    public RectTransform introduceText1;
    
    [Tooltip("第二次点击时出现的IntroduceText2")]
    public RectTransform introduceText2;
    
    [Tooltip("第二次点击时向下移动出现的Plastic组件")]
    public RectTransform plasticComponent;
    
    [Header("动画设置")]
    [Tooltip("动画持续时间（秒）")]
    public float animationDuration = 1.0f;
    
    [Tooltip("PatternSelectionPanel向左移动的单位数")]
    public float panelMoveDistance = 50f;
    
    [Tooltip("Toolbar向右移动的单位数")]
    public float toolbarMoveDistance = 200f;
    
    [Tooltip("DesignArea向左移动的单位数")]
    public float designAreaMoveDistance = 50f;
    
    [Tooltip("IntroduceArea向左移动的单位数")]
    public float introduceAreaMoveDistance = 200f;
    
    [Tooltip("IntroduceText缩放动画的持续时间")]
    public float introduceTextScaleDuration = 0.5f;
    
    [Tooltip("Plastic组件向下移动的单位数")]
    public float plasticMoveDistance = 400f;
    
    [Tooltip("Plastic组件移动动画的持续时间")]
    public float plasticMoveDuration = 0.8f;
    
    // 点击次数计数器
    private int clickCount = 0;
    
    private void Start()
    {
        // 尝试从按钮组件获取自身
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnNextStepButtonClicked);
        }
        
        // 初始化introduceText2为不可见（缩放为0）
        if (introduceText2 != null)
        {
            introduceText2.localScale = Vector3.zero;
        }
        
        // 初始化clickCount
        clickCount = 0;
    }
    
    /// <summary>
    /// 当NextStepButton被点击时调用
    /// 根据点击次数执行不同的动画序列
    /// </summary>
    public void OnNextStepButtonClicked()
    {
        // 增加点击计数
        clickCount++;
        Debug.Log($"NextStepButton第{clickCount}次被点击，开始执行动画序列");
        
        // 检查各组件引用
        Debug.Log($"组件引用检查：\n- patternSelectionPanel: {(patternSelectionPanel != null ? "已设置" : "未设置")}\n- toolbar: {(toolbar != null ? "已设置" : "未设置")}\n- designArea: {(designArea != null ? "已设置" : "未设置")}\n- introduceArea: {(introduceArea != null ? "已设置" : "未设置")}\n- introduceText1: {(introduceText1 != null ? "已设置" : "未设置")}\n- introduceText2: {(introduceText2 != null ? "已设置" : "未设置")}\n- plasticComponent: {(plasticComponent != null ? "已设置" : "未设置")}");
        
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        
        // 第三次点击直接处理场景切换，不执行任何UI动画
        if (clickCount == 3)
        {
            Debug.Log("执行第三次点击，准备切换到DyeProduct场景");
            
            // 延迟一小段时间，然后切换场景
            sequence.AppendInterval(0.2f);
            sequence.AppendCallback(() =>
            {
                try
                {
                    Debug.Log("正在加载DyeProduct场景");
                    SceneManager.LoadScene("Assets/Scenes/Design/DyeProduct.unity");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("加载DyeProduct场景失败: " + e.Message);
                }
            });
        }
        else
        {
            // 只有在前两次点击时才执行UI动画
            // 第一步：同时移动PatternSelectionPanel、Toolbar和DesignArea
            if (patternSelectionPanel != null)
            {
                // 使PatternSelectionPanel向左移动
                float startX = patternSelectionPanel.localPosition.x;
                float targetX = startX - panelMoveDistance;
                Debug.Log($"patternSelectionPanel移动：从{startX:0.##}到{targetX:0.##}");
                sequence.Join(patternSelectionPanel.DOLocalMoveX(targetX, animationDuration)
                    .SetEase(Ease.InOutQuad).OnComplete(() => Debug.Log("patternSelectionPanel动画完成")));
            }
            
            if (toolbar != null)
            {
                // 使Toolbar向右移动消失
                float startX = toolbar.localPosition.x;
                float targetX = startX + toolbarMoveDistance;
                Debug.Log($"toolbar移动：从{startX:0.##}到{targetX:0.##}");
                sequence.Join(toolbar.DOLocalMoveX(targetX, animationDuration)
                    .SetEase(Ease.InOutQuad).OnComplete(() => Debug.Log("toolbar动画完成")));
            }
            
            // 只有在不是第二次点击时才移动designArea
            if (designArea != null && clickCount != 2)
            {
                // 使DesignArea向左移动50个单位
                float startX = designArea.localPosition.x;
                float targetX = startX - designAreaMoveDistance;
                Debug.Log($"designArea移动：从{startX:0.##}到{targetX:0.##}");
                sequence.Join(designArea.DOLocalMoveX(targetX, animationDuration)
                    .SetEase(Ease.InOutQuad).OnComplete(() => Debug.Log("designArea动画完成")));
            }
            
            // 只有在不是第二次点击时才移动introduceArea
            if (clickCount != 2)
            {
                // 第二步：间隔0.1秒后，让IntroduceArea向左移动200个单位（动画持续0.8秒）
                Debug.Log("开始等待0.1秒");
                sequence.AppendInterval(0.1f).OnComplete(() => Debug.Log("等待完成，开始执行introduceArea动画"));
                
                if (introduceArea != null)
                {
                    float startX = introduceArea.localPosition.x;
                    float targetX = startX - introduceAreaMoveDistance;
                    Debug.Log($"introduceArea移动：从{startX:0.##}到{targetX:0.##}");
                    sequence.Append(introduceArea.DOLocalMoveX(targetX, 0.8f)
                        .SetEase(Ease.InOutQuad).OnComplete(() => Debug.Log("introduceArea动画完成")));
                }
            }
            
            // 第三步：间隔1秒后，让IntroduceText从中心缩放出现
            Debug.Log("开始等待0.3秒");
            sequence.AppendInterval(0.3f).OnComplete(() => Debug.Log("等待完成，开始执行introduceText1动画"));
            
            // 根据点击次数执行不同的动画
            if (clickCount == 1)
            {
                // 第一次点击时的动画
                if (introduceText1 != null)
                {
                    // 先确保文本初始为缩放为0状态
                    Debug.Log("设置introduceText1初始缩放到0");
                    introduceText1.localScale = Vector3.zero;
                    
                    // 执行从0到1的缩放动画（从中心缩放）
                    Debug.Log("执行introduceText1缩放动画");
                    sequence.Append(introduceText1.DOScale(Vector3.one, introduceTextScaleDuration)
                        .SetEase(Ease.OutQuad).OnComplete(() => Debug.Log("introduceText1动画完成")));
                }
            }
            else if (clickCount == 2)
            {
                Debug.Log("执行第二次点击的动画序列");
                
                // 第一步：introduceText1缩放消失
                if (introduceText1 != null)
                {
                    Debug.Log("执行introduceText1缩放消失动画");
                    sequence.Append(introduceText1.DOScale(Vector3.zero, introduceTextScaleDuration)
                        .SetEase(Ease.InQuad).OnComplete(() => Debug.Log("introduceText1缩放消失完成")));
                }
                
                // 第二步：introduceText2缩放出现
                sequence.AppendCallback(() =>
                {
                    if (introduceText2 != null)
                    {
                        Debug.Log("执行introduceText2缩放出现动画");
                        sequence.Join(introduceText2.DOScale(Vector3.one, introduceTextScaleDuration)
                            .SetEase(Ease.OutQuad).OnComplete(() => Debug.Log("introduceText2缩放出现完成")));
                    }
                });
                
                // 第三步：Plastic组件向下移动400个单位出现
                sequence.AppendCallback(() =>
                {
                    if (plasticComponent != null)
                    {
                        float startY = plasticComponent.localPosition.y;
                        float targetY = startY - plasticMoveDistance;
                        Debug.Log($"plasticComponent移动：从{startY:0.##}到{targetY:0.##}");
                        sequence.Join(plasticComponent.DOLocalMoveY(targetY, plasticMoveDuration)
                            .SetEase(Ease.OutQuad).OnComplete(() => Debug.Log("plasticComponent移动完成")));
                    }
                });
            }
        }
        
        sequence.OnComplete(() => Debug.Log($"第{clickCount}次点击的所有动画序列执行完成"));
    }
    
    private void OnDestroy()
    {
        // 清理动画，防止内存泄漏
        DOTween.Kill(patternSelectionPanel);
        DOTween.Kill(toolbar);
        DOTween.Kill(designArea);
        DOTween.Kill(introduceArea);
        DOTween.Kill(introduceText1);
        DOTween.Kill(introduceText2);
        DOTween.Kill(plasticComponent);
    }
}