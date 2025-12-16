using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("场景名称配置")]
    // 这里已经帮你改成了 MainMenuScene
    public string mainSceneName = "MainMenuScene";
    public string productSceneName = "Product";
    public string designSceneName = "DesignScene";
    public string techniqueSceneName = "Technique";
    public string patternerSceneName = "PatternerScene";
    public string historySceneName = "History";
    public string tieDyeSceneName = "TieDyeScene";

    // 跳转到产品场景
    public void LoadProductScene()
    {
        LoadScene(productSceneName);
    }

    // 跳转到设计场景
    public void LoadDesignScene()
    {
        LoadScene(designSceneName);
    }

    // 跳转到工艺场景
    public void LoadTechniqueScene()
    {
        LoadScene(techniqueSceneName);
    }

    // 跳转到制版场景
    public void LoadPatternerScene()
    {
        LoadScene(patternerSceneName);
    }

    // 跳转到历史场景
    public void LoadHistoryScene()
    {
        LoadScene(historySceneName);
    }

    // 跳转到主场景 (这里会去加载 MainMenuScene)
    public void LoadMainScene()
    {
        LoadScene(mainSceneName);
    }

    // 加载TieDye场景
    public void LoadTieDyeScene()
    {
        LoadScene(tieDyeSceneName);
    }

    // 通过名称加载场景
    public void LoadScene(string sceneName)
    {
        if (SceneExists(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"场景 '{sceneName}' 不存在，请检查场景名称和 Build Settings。");
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