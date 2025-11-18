using UnityEngine;
using System.Collections.Generic;
using System;

public class UIResourceManager : MonoBehaviour
{
    // 单例模式
    private static UIResourceManager _instance;
    public static UIResourceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIResourceManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("UIResourceManager");
                    _instance = obj.AddComponent<UIResourceManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }
    
    [Header("资源路径配置")]
    public string uiSpritePath = "UI/";
    public string uiPrefabPath = "Prefabs/UI/";
    
    // 缓存字典
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        // 确保只有一个实例
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    // 加载精灵资源
    public Sprite LoadSprite(string spriteName, bool useCache = true)
    {
        // 检查缓存
        if (useCache && spriteCache.ContainsKey(spriteName))
        {
            return spriteCache[spriteName];
        }
        
        // 构建资源路径
        string fullPath = uiSpritePath + spriteName;
        
        // 尝试加载资源
        Sprite sprite = Resources.Load<Sprite>(fullPath);
        
        // 如果找到资源，添加到缓存
        if (sprite != null && useCache)
        {
            spriteCache[spriteName] = sprite;
        }
        else if (sprite == null)
        {
            Debug.LogWarning($"无法找到精灵资源: {fullPath}");
        }
        
        return sprite;
    }
    
    // 加载预制体资源
    public GameObject LoadPrefab(string prefabName, bool useCache = true)
    {
        // 检查缓存
        if (useCache && prefabCache.ContainsKey(prefabName))
        {
            return prefabCache[prefabName];
        }
        
        // 构建资源路径
        string fullPath = uiPrefabPath + prefabName;
        
        // 尝试加载资源
        GameObject prefab = Resources.Load<GameObject>(fullPath);
        
        // 如果找到资源，添加到缓存
        if (prefab != null && useCache)
        {
            prefabCache[prefabName] = prefab;
        }
        else if (prefab == null)
        {
            Debug.LogWarning($"无法找到预制体资源: {fullPath}");
        }
        
        return prefab;
    }
    
    // 实例化UI预制体
    public GameObject InstantiateUI(string prefabName, Transform parent = null, bool useCache = true)
    {
        GameObject prefab = LoadPrefab(prefabName, useCache);
        
        if (prefab == null)
        {
            return null;
        }
        
        // 如果没有指定父对象，使用Canvas
        if (parent == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                parent = canvas.transform;
            }
        }
        
        // 实例化预制体
        GameObject instance = Instantiate(prefab, parent);
        return instance;
    }
    
    // 清除指定资源的缓存
    public void ClearCache(string resourceName)
    {
        if (spriteCache.ContainsKey(resourceName))
        {
            spriteCache.Remove(resourceName);
        }
        
        if (prefabCache.ContainsKey(resourceName))
        {
            prefabCache.Remove(resourceName);
        }
    }
    
    // 清除所有缓存
    public void ClearAllCache()
    {
        spriteCache.Clear();
        prefabCache.Clear();
    }
    
    // 获取缓存信息
    public void LogCacheInfo()
    {
        Debug.Log($"UI资源缓存信息:\n" +
                  $"精灵缓存数量: {spriteCache.Count}\n" +
                  $"预制体缓存数量: {prefabCache.Count}");
    }
    
    // 预加载常用资源
    public void PreloadResources(List<string> spriteNames, List<string> prefabNames)
    {
        foreach (string name in spriteNames)
        {
            LoadSprite(name, true);
        }
        
        foreach (string name in prefabNames)
        {
            LoadPrefab(name, true);
        }
        
        LogCacheInfo();
    }
}