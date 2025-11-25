using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("��Ϸ״̬")]
    public List<PatternData> draftDesign; // ����׸��������
    public bool dyeReady = false; // Ⱦ���Ƿ�׼����

    // ��������ࣨ��Ҫ��GameManager�����¶��壬����ʹ��DraftDesignManager�еĶ��壩
    [System.Serializable]
    public class PatternData
    {
        public string patternName;
        public Vector2 position;
        public Sprite patternSprite;
    }

    void Awake()
    {
        // 修复场景跳转时GameManager无法正常运行的问题
        // 检查是否已有实例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager: 创建新实例并保持不销毁");
        }
        else
        {
            // 不销毁新场景中的GameManager，而是将数据合并
            // 这样可以保留新场景中GameManager的设置和引用
            Debug.Log("GameManager: 已有实例，保留当前实例");
            // 可以在这里添加数据合并逻辑
        }
    }

    

    // ��ȡ������������
    public List<PatternData> GetSavedDesign()
    {
        return draftDesign;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}