using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 确保脚本在编辑器加载时运行
[InitializeOnLoad]
public class BootSceneLoader
{
    // 菜单路径
    const string MENU_PATH = "Tools/设置当前场景为启动场景";
    const string CLEAR_MENU_PATH = "Tools/清除启动场景设置";
    const string PREF_KEY = "MyGame_BootScenePath";

    static BootSceneLoader()
    {
        // 编辑器启动或编译完成后，尝试恢复设置
        string path = EditorPrefs.GetString(PREF_KEY);
        if (!string.IsNullOrEmpty(path))
        {
            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (scene != null)
            {
                EditorSceneManager.playModeStartScene = scene;
            }
            else
            {
                // 如果场景文件被删除了，清除设置
                EditorSceneManager.playModeStartScene = null;
            }
        }
    }

    [MenuItem(MENU_PATH)]
    static void SetCurrentSceneAsBoot()
    {
        // 获取当前打开的场景
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        if (string.IsNullOrEmpty(currentScene.path))
        {
            EditorUtility.DisplayDialog("错误", "请先保存当前场景再设置！", "好的");
            return;
        }

        // 保存路径到本地配置
        EditorPrefs.SetString(PREF_KEY, currentScene.path);

        // 设置 Unity API
        SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(currentScene.path);
        EditorSceneManager.playModeStartScene = scene;

        Debug.Log($"<color=green>已设置启动场景为: {currentScene.name}</color>");
    }

    // 增加一个勾选状态显示
    [MenuItem(MENU_PATH, true)]
    static bool ValidateSetBootScene()
    {
        Menu.SetChecked(MENU_PATH, EditorSceneManager.playModeStartScene != null);
        return true;
    }

    [MenuItem(CLEAR_MENU_PATH)]
    static void ClearBootScene()
    {
        EditorPrefs.DeleteKey(PREF_KEY);
        EditorSceneManager.playModeStartScene = null;
        Debug.Log("<color=yellow>已清除启动场景设置，现在运行将直接打开当前场景。</color>");
    }
}