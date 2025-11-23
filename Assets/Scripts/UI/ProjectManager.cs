using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// 项目管理器 - 负责管理开场动画和背景音乐
/// </summary>
public class ProjectManager : MonoBehaviour
{
    // 视频播放器组件引用
    private VideoPlayer videoPlayer;
    
    // 视频渲染的RawImage组件
    private RawImage rawImage;
    
    // 音频源组件引用
    private AudioSource audioSource;
    
    // 开场视频路径
    [SerializeField] private string videoPath = "Animation/OpenAnimation.mp4";
    
    // 背景音乐路径
    [SerializeField] private string musicPath = "Audio/Theme";
    
    private void Awake()
    {
        // 初始化UI组件
        InitializeUIComponents();
    }

    private void Start()
    {
        // 开始播放开场动画
        PlayOpeningAnimation();
        
        // 开始播放背景音乐
        PlayBackgroundMusic();
    }

    /// <summary>
    /// 初始化UI组件
    /// </summary>
    private void InitializeUIComponents()
    {
        // 检查并添加VideoPlayer组件
        videoPlayer = gameObject.GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.source = VideoSource.VideoClip;
        }

        // 检查并添加RawImage组件
        rawImage = gameObject.GetComponentInChildren<RawImage>();
        if (rawImage == null)
        {
            // 创建Canvas
            GameObject canvas = new GameObject("Canvas");
            canvas.transform.SetParent(transform);
            canvas.AddComponent<Canvas>();
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();
            
            // 创建RawImage
            GameObject rawImageObj = new GameObject("VideoDisplay");
            rawImageObj.transform.SetParent(canvas.transform);
            rawImage = rawImageObj.AddComponent<RawImage>();
            
            // 设置RawImage尺寸以适应屏幕
            rawImage.rectTransform.anchorMin = Vector2.zero;
            rawImage.rectTransform.anchorMax = Vector2.one;
            rawImage.rectTransform.offsetMin = Vector2.zero;
            rawImage.rectTransform.offsetMax = Vector2.zero;
        }

        // 检查并添加AudioSource组件
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// 播放开场动画
    /// </summary>
    /// <summary>
    /// 播放开场动画
    /// </summary>
    private void PlayOpeningAnimation()
    {
        // 直接从AssetDatabase加载视频资源（运行时使用）
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Application.dataPath + "/Animation/OpenAnimation.mp4.mp4";
        videoPlayer.targetTexture = new RenderTexture(1920, 1080, 24); // 设置一个合适的分辨率
        rawImage.texture = videoPlayer.targetTexture;
        
        // 设置视频结束事件
        videoPlayer.loopPointReached += OnVideoEnd;
        
        // 开始播放视频
        videoPlayer.Play();
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    private void PlayBackgroundMusic()
    {        
        // 直接从路径加载音频资源
        StartCoroutine(LoadAudioAsync());
    }
    
    /// <summary>
    /// 异步加载音频文件
    /// </summary>
    private IEnumerator LoadAudioAsync()
    {        
        string audioFilePath = Application.dataPath + "/Audio/Theme.ogg";
        
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + audioFilePath, AudioType.OGGVORBIS))
        {            
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {                
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = audioClip;
                audioSource.Play();
            }            
            else
            {                
                Debug.LogWarning($"无法加载背景音乐: {www.error}");
            }
        }
    }

    /// <summary>
    /// 视频播放结束事件处理
    /// </summary>
    private void OnVideoEnd(VideoPlayer vp)
    {        
        // 视频播放结束后隐藏视频显示
        StartCoroutine(HideVideoAfterDelay(1.0f));
    }

    /// <summary>
    /// 延迟隐藏视频显示
    /// </summary>
    private IEnumerator HideVideoAfterDelay(float delay)
    {        
        yield return new WaitForSeconds(delay);
        if (rawImage != null)
        {
            rawImage.gameObject.SetActive(false);
        }
    }
}
