using System.Collections.Generic;
using UnityEngine;

// 这是一个静态仓库，专门存用户运行期间上传的图片数据
public static class UserPatternStorage
{
    public static Dictionary<string, PatternData> userPatterns = new Dictionary<string, PatternData>();

    // 清理方法 (可以在重置游戏或退出时调用)
    public static void Clear()
    {
        userPatterns.Clear();
    }
}