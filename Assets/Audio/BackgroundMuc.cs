using UnityEngine;

public class SimpleBackgroundMusic : MonoBehaviour
{
    public AudioClip backgroundMusic;

    private static SimpleBackgroundMusic instance;

    void Start()
    {
        // 如果已存在实例，销毁新创建的
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 设置当前实例
        instance = this;
        DontDestroyOnLoad(gameObject);

        // 设置AudioSource
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.volume = 0.7f;
        audioSource.Play();
    }
}