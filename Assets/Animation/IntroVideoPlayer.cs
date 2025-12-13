using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroVideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject videoCanvas; // ����Video Player��UI����

    void Start()
    {
        // ��ȡVideo Player���
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // ע����Ƶ��������¼�
        videoPlayer.loopPointReached += OnVideoFinished;

        // ��ʼ������Ƶ
        videoPlayer.Play();
    }

    // ��Ƶ�������ʱ�Ļص�����
    void OnVideoFinished(VideoPlayer vp)
    {
        // �ر���Ƶ�����������UI
        if (videoCanvas != null)
            videoCanvas.SetActive(false);
        else
            gameObject.SetActive(false);

        // ������һ������
        SceneManager.LoadScene("SampleScene");
    }

    void OnDestroy()
    {
        // ȡ���¼�ע��
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }
}