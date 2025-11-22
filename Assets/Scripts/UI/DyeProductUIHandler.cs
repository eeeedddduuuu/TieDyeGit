using UnityEngine;
using System.Collections;
using DG.Tweening;

public class DyeProductUIHandler : MonoBehaviour
{
    private GameObject bucket;
    private GameObject banlan;
    private GameObject lime;
    private GameObject rawmaterialsIntroduce;

    private void Start()
    {
        // 自动查找组件
        bucket = GameObject.Find("MainCanvas/Bucket");
        banlan = GameObject.Find("MainCanvas/Banlan");
        lime = GameObject.Find("MainCanvas/Lime");
        rawmaterialsIntroduce = GameObject.Find("MainCanvas/RawmaterialsIntroduce");
        
        StartAnimations();
    }

    private void StartAnimations()
    {
        if (bucket == null || banlan == null || lime == null || rawmaterialsIntroduce == null)
        {
            Debug.LogWarning("Some UI components not found!");
            return;
        }
        
        // 存储初始位置和状态
        Vector3 bucketInitialPos = bucket.transform.position;
        
        // 设置其他组件初始状态（隐藏并放置在左侧）
        banlan.transform.position = new Vector3(-1000, banlan.transform.position.y, banlan.transform.position.z);
        lime.transform.position = new Vector3(-1000, lime.transform.position.y, lime.transform.position.z);
        rawmaterialsIntroduce.transform.position = new Vector3(-1000, rawmaterialsIntroduce.transform.position.y, rawmaterialsIntroduce.transform.position.z);

        // 执行动画
        // Bucket向左平移400单位，时长1秒
        bucket.transform.DOMoveX(bucketInitialPos.x - 400, 1f);

        // Banlan、Lime、RawmaterialsIntroduce先后从左向右展开（渐显）
        // 时差间隔0.5秒，均持续1秒
        StartCoroutine(AnimateElement(banlan, 0f));
        StartCoroutine(AnimateElement(lime, 0.5f));
        StartCoroutine(AnimateElement(rawmaterialsIntroduce, 1f));
    }

    private IEnumerator AnimateElement(GameObject element, float delay)
    {
        // 等待指定延迟时间
        yield return new WaitForSeconds(delay);
        
        // 记录当前位置
        Vector3 startPos = element.transform.position;
        Vector3 targetPos = new Vector3(0, startPos.y, startPos.z); // 目标位置设为X=0
        
        // 从左侧滑入并渐显
        element.transform.DOMoveX(targetPos.x, 1f).SetEase(Ease.OutCubic);
        
        // 同时设置透明度动画（可选，如果需要渐显效果）
        CanvasGroup canvasGroup = element.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = element.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, 1f);
    }
}