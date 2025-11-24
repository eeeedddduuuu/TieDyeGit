using UnityEngine;

public class SimpleBackgroundMusic : MonoBehaviour
{
    public AudioClip backgroundMusic;

    void Start()
    {
        // 确保对象在场景切换时不被销毁
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

        // 防止重复创建（简单检查）
        if (GameObject.FindGameObjectsWithTag("BackgroundMusic").Length > 1)
        {
            Destroy(gameObject);
        }
    }
}