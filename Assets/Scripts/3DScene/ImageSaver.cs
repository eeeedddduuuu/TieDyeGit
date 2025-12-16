using UnityEngine;
using System.IO;
using System;
using System.Collections;

public class ImageSaver : MonoBehaviour
{
    [Header("核心设置")]
    public Camera photoCamera;

    [Tooltip("请拖入 DynamicCloth 物体本身，而不是它的组件")]
    public GameObject targetClothObject; // <--- 修改：存物体，不存组件

    [Header("构图设置")]
    [Tooltip("画面留白比例 (1.1 - 1.3)")]
    public float framingPadding = 1.2f;

    [Tooltip("拍照清晰度倍数")]
    public int resolutionMultiplier = 2;

    [Tooltip("是否隐藏UI")]
    public bool excludeUILayer = true;

    private int uiLayerIndex = 5;

    void Start()
    {
        if (photoCamera == null) photoCamera = Camera.main;

        // 如果没拖物体，自动找
        if (targetClothObject == null)
        {
            targetClothObject = GameObject.Find("DynamicCloth");
        }
    }

    public void CaptureAndSave()
    {
        Debug.Log("【系统】正在寻找最佳角度拍照...");
        StartCoroutine(CaptureProcess());
    }

    private IEnumerator CaptureProcess()
    {
        yield return new WaitForEndOfFrame();

        // --- 1. 动态寻找活着的渲染器 ---
        Renderer currentRenderer = null;
        if (targetClothObject != null)
        {
            // 优先找 SkinnedMeshRenderer (布料专用)
            currentRenderer = targetClothObject.GetComponent<SkinnedMeshRenderer>();
            // 如果没找到，再找 MeshRenderer (保底)
            if (currentRenderer == null) currentRenderer = targetClothObject.GetComponent<MeshRenderer>();
        }

        if (currentRenderer == null)
        {
            Debug.LogError("拍照失败：在目标物体上找不到任何 Renderer 组件！");
            yield break;
        }

        // --- 2. 记录原始状态 ---
        Vector3 originalPos = photoCamera.transform.position;
        Quaternion originalRot = photoCamera.transform.rotation;
        RenderTexture oldRT = photoCamera.targetTexture;
        int oldMask = photoCamera.cullingMask;

        // --- 3. 自动构图 (使用刚才找到的活体渲染器) ---
        FocusCameraOnTarget(photoCamera, currentRenderer.bounds);

        // --- 4. 准备渲染 ---
        if (excludeUILayer) photoCamera.cullingMask &= ~(1 << uiLayerIndex);

        int width = Screen.width * resolutionMultiplier;
        int height = Screen.height * resolutionMultiplier;
        RenderTexture tempRT = new RenderTexture(width, height, 24);
        tempRT.antiAliasing = 4;

        photoCamera.targetTexture = tempRT;
        photoCamera.Render();

        // --- 5. 保存 ---
        RenderTexture.active = tempRT;
        Texture2D finalImage = new Texture2D(width, height, TextureFormat.RGB24, false);
        finalImage.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        finalImage.Apply();

        // --- 6. 复位 ---
        photoCamera.targetTexture = oldRT;
        photoCamera.cullingMask = oldMask;
        photoCamera.transform.position = originalPos;
        photoCamera.transform.rotation = originalRot;

        RenderTexture.active = null;
        Destroy(tempRT);

        string fileName = $"TieDye_Design_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
        File.WriteAllBytes(savePath, finalImage.EncodeToPNG());

        Debug.Log($"照片已保存: {savePath}");
    }

    void FocusCameraOnTarget(Camera cam, Bounds bounds)
    {
        Vector3 center = bounds.center;
        float objectSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * cam.fieldOfView);
        float distance = (framingPadding * objectSize) / cameraView;
        distance += 0.5f * objectSize;

        cam.transform.position = center - cam.transform.forward * distance;
        cam.transform.LookAt(center);
    }
}