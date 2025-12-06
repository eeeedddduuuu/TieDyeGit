using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// 音频管理器
/// 管理全局音频播放，包括点击音效等
/// </summary>
public class AudioManager : MonoBehaviour, IPointerClickHandler
{
    // 单例实例
    private static AudioManager instance;
    
    // 点击音效
    [Header("点击音效")]
    public AudioClip clickSound;
    
    // 音频源组件
    private AudioSource audioSource;
    
    // 事件系统引用
    private EventSystem eventSystem;
    
    /// <summary>
    /// 获取单例实例
    /// </summary>
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("AudioManager实例不存在！");
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        // 实现单例模式
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 获取或添加音频源组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 设置音频源参数
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D音效
        
        // 加载点击音效
        LoadClickSound();
        
        // 获取事件系统
        eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("场景中没有EventSystem！");
        }
    }
    
    /// <summary>
    /// 加载点击音效
    /// </summary>
    private void LoadClickSound()
    {
        // 检查是否已经在Inspector中指定了音效
        if (clickSound == null)
        {
            Debug.LogWarning("未找到点击音效，请在AudioManager的Inspector面板中指定'泡泡音.wav'文件！");
        }
    }
    
    /// <summary>
    /// 播放点击音效
    /// </summary>
    public void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        else if (clickSound == null)
        {
            Debug.LogWarning("点击音效未设置，无法播放！");
        }
        else if (audioSource == null)
        {
            Debug.LogWarning("音频源未初始化，无法播放音效！");
        }
    }
    
    private void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            // 播放点击音效
            PlayClickSound();
        }
    }
    
    /// <summary>
    /// 实现IPointerClickHandler接口
    /// 处理UI元素的点击事件
    /// </summary>
    /// <param name="eventData">事件数据</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 播放点击音效
        PlayClickSound();
    }
}