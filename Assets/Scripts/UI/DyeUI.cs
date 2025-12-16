using UnityEngine;
using System.Collections;

public class DyeUI : MonoBehaviour
{
    private GameObject bucket;
    private GameObject banlan;
    private GameObject lime;
    private GameObject rawmaterialsIntroduce;

    void Start()
    {
        // 自动查找组件
        bucket = GameObject.Find("MainCanvas/Bucket");
        banlan = GameObject.Find("MainCanvas/Banlan");
        lime = GameObject.Find("MainCanvas/Lime");
        rawmaterialsIntroduce = GameObject.Find("MainCanvas/RawmaterialsIntroduce");
        
        if (bucket != null) Debug.Log("Bucket found!");
        if (banlan != null) Debug.Log("Banlan found!");
        if (lime != null) Debug.Log("Lime found!");
        if (rawmaterialsIntroduce != null) Debug.Log("RawmaterialsIntroduce found!");
        
        // 直接设置动画，不使用DoTween
        StartCoroutine(PlayAnimations());
    }
    
    IEnumerator PlayAnimations()
    {
        yield return new WaitForSeconds(0.5f); // 等待一点时间确保场景加载完成
        
        // Bucket向左平移400单位，时长1秒
        if (bucket != null)
        {
            StartCoroutine(MoveObject(bucket, new Vector3(bucket.transform.position.x - 400, bucket.transform.position.y, bucket.transform.position.z), 1f));
        }
        
        // Banlan、Lime、RawmaterialsIntroduce先后从左向右展开
        if (banlan != null)
        {
            StartCoroutine(AnimateUIElement(banlan, 0f));
        }
        
        if (lime != null)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(AnimateUIElement(lime, 0f));
        }
        
        if (rawmaterialsIntroduce != null)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(AnimateUIElement(rawmaterialsIntroduce, 0f));
        }
    }
    
    IEnumerator MoveObject(GameObject obj, Vector3 targetPos, float duration)
    {
        Vector3 startPos = obj.transform.position;
        float elapsedTime = 0;
        
        while (elapsedTime < duration)
        {
            obj.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        obj.transform.position = targetPos;
    }
    
    IEnumerator AnimateUIElement(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 确保对象可见
        obj.SetActive(true);
        
        // 存储目标位置，当前位置作为起始位置
        Vector3 startPos = obj.transform.position;
        Vector3 targetPos = new Vector3(0, startPos.y, startPos.z);
        
        // 添加CanvasGroup组件用于透明度控制
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0;
        
        // 执行动画：移动并渐显
        float duration = 1f;
        float elapsedTime = 0;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            obj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        obj.transform.position = targetPos;
        canvasGroup.alpha = 1;
    }
}