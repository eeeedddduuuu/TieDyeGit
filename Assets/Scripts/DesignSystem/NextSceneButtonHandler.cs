using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneButtonHandler : MonoBehaviour
{
    // 目标场景名称
    public string targetSceneName = "TieDyeScene";

    // 按钮点击事件处理方法
    public void OnNextSceneButtonClick()
    {
        // 检查场景是否存在于Build Settings中
        if (SceneExists(targetSceneName))
        {
            // 加载目标场景
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"场景 '{targetSceneName}' 不存在，请检查场景名称或Build Settings设置");
        }
    }

    // 检查场景是否存在于Build Settings中
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
}