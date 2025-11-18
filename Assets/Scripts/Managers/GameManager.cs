using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("游戏状态")]
    public List<PatternData> draftDesign; // 保存底稿设计数据
    public bool dyeReady = false; // 染料是否准备好

    // 设计数据类（需要在GameManager中重新定义，或者使用DraftDesignManager中的定义）
    [System.Serializable]
    public class PatternData
    {
        public string patternName;
        public Vector2 position;
        public Sprite patternSprite;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    

    // 获取保存的设计数据
    public List<PatternData> GetSavedDesign()
    {
        return draftDesign;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}