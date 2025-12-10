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
    public string tieDyeSceneName = "TieDyeScene";

    // ��ת����Ʒ����
    public void LoadProductScene()
    {
        LoadScene(productSceneName);
    }

    // ��ת����ƽ���
    public void LoadDesignScene()
    {
        LoadScene(designSceneName);
    }

    // ��ת����������
    public void LoadTechniqueScene()
    {
        LoadScene(techniqueSceneName);
    }

    // ��ת�����ƽ���
    public void LoadPatternerScene()
    {
        LoadScene(patternerSceneName);
    }

    // ��ת����ʷ����
    public void LoadHistoryScene()
    {
        LoadScene(historySceneName);
    }

    // ����������
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
            Debug.LogError($"���� '{sceneName}' �����ڣ����鳡�����ƺ�Build Settings��");
        }
    }

    // ��鳡���Ƿ����
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

    // ���¼��ص�ǰ����
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}