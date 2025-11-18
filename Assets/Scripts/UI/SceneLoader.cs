using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("场景名称配置")]
    public string mainSceneName = "SampleScene";
    public string productSceneName = "Product";
    public string designSceneName = "DesignScene";
    public string techniqueSceneName = "Technique";
    public string patternerSceneName = "PatternerScene";
    public string historySceneName = "History";

    // 跳转到成品界面
    public void LoadProductScene()
    {
        LoadScene(productSceneName);
    }

    // 跳转到设计界面
    public void LoadDesignScene()
    {
        LoadScene(designSceneName);
    }

    // 跳转到技法界面
    public void LoadTechniqueScene()
    {
        LoadScene(techniqueSceneName);
    }

    // 跳转到花纹界面
    public void LoadPatternerScene()
    {
        LoadScene(patternerSceneName);
    }

    // 跳转到历史界面
    public void LoadHistoryScene()
    {
        LoadScene(historySceneName);
    }

    // 返回主界面
    public void LoadMainScene()
    {
        LoadScene(mainSceneName);
    }

    // 通用场景加载方法
    public void LoadScene(string sceneName)
    {
        if (SceneExists(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"场景 '{sceneName}' 不存在！请检查场景名称和Build Settings。");
        }
    }

    // 检查场景是否存在
    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string scene = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (scene == sceneName)
                return true;
        }
        return false;
    }

    // 重新加载当前场景
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}