using UnityEngine;
using UnityEngine.UI;

public class TieDyeColorController : MonoBehaviour
{
    [Header("组件引用")]
    public PatternTextureBaker baker; // 拖入场景里的 TextureBakerManager
    public Slider colorSlider;       

    [Header("颜色范围设置")]
    public Gradient colorGradient;    

    void Start()
    {
        // 1. 初始化渐变色 (如果没有在Inspector设置的话，给个默认值)
        if (colorGradient == null || colorGradient.colorKeys.Length == 0)
        {
            SetupDefaultGradient();
        }

        // 2. 监听 Slider 变化
        if (colorSlider != null)
        {
            // 设定 Slider 范围
            colorSlider.minValue = 0f;
            colorSlider.maxValue = 1f;
            colorSlider.value = 0f; // 默认蓝色

            // 绑定事件
            colorSlider.onValueChanged.AddListener(OnColorChanged);

            // 初始化一次颜色
            OnColorChanged(colorSlider.value);
        }
    }

    // 当滑块拖动时调用
    void OnColorChanged(float value)
    {
        if (baker != null)
        {
            // 从渐变中取色
            Color targetColor = colorGradient.Evaluate(value);

            // 通知 Baker 改色
            baker.UpdateBackgroundColor(targetColor);
        }
    }

    // 设置默认的“靓蓝 -> 蓝紫 -> 深紫”渐变
    void SetupDefaultGradient()
    {
        colorGradient = new Gradient();

        // 定义颜色键 (时间 0~1)
        GradientColorKey[] colorKeys = new GradientColorKey[3];
        colorKeys[0] = new GradientColorKey(new Color(0f, 0.4f, 0.8f), 0.0f);   // 0.0: 靓蓝色
        colorKeys[1] = new GradientColorKey(new Color(0.3f, 0.2f, 0.8f), 0.5f); // 0.5: 蓝紫色
        colorKeys[2] = new GradientColorKey(new Color(0.5f, 0.0f, 0.5f), 1.0f); // 1.0: 紫罗兰

        // 定义透明度键 (全是 1)
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);

        colorGradient.SetKeys(colorKeys, alphaKeys);
    }
}