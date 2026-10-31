using UnityEngine;

/// <summary>
/// 关卡进度存档。
/// 负责保存和读取玩家的关卡进度。
/// </summary>
public static class WasteLevelProgress
{
    private const string HighestUnlockedLevelKey =
        "Waste_HighestUnlockedLevel";

    private const string ContinueLevelKey =
        "Waste_ContinueLevel";

    private const string HasSaveKey =
        "Waste_HasSave";

    private const string AllLevelsCompletedKey =
        "Waste_AllLevelsCompleted";

    /// <summary>
    /// 已解锁的最高关卡。
    /// </summary>
    public static int HighestUnlockedLevel =>
        PlayerPrefs.GetInt(HighestUnlockedLevelKey, 1);

    /// <summary>
    /// 继续游戏时需要进入的关卡。
    /// </summary>
    public static int ContinueLevelId =>
        PlayerPrefs.GetInt(ContinueLevelKey, 1);

    /// <summary>
    /// 是否存在可继续的存档。
    /// </summary>
    public static bool HasContinueData =>
        PlayerPrefs.GetInt(HasSaveKey, 0) == 1 &&
        !IsAllLevelsCompleted;

    /// <summary>
    /// 是否已经完成全部关卡。
    /// </summary>
    public static bool IsAllLevelsCompleted =>
        PlayerPrefs.GetInt(AllLevelsCompletedKey, 0) == 1;

    /// <summary>
    /// 开始新游戏，重置关卡进度。
    /// </summary>
    public static void ResetForNewGame()
    {
        PlayerPrefs.SetInt(HighestUnlockedLevelKey, 1);
        PlayerPrefs.SetInt(ContinueLevelKey, 1);
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.SetInt(AllLevelsCompletedKey, 0);

        PlayerPrefs.Save();

        Debug.Log("[WasteLevelProgress] 新游戏存档已初始化。");
    }

    /// <summary>
    /// 保存当前正在进行的关卡。
    /// </summary>
    public static void SaveCurrentLevel(int levelId)
    {
        levelId = Mathf.Max(1, levelId);

        PlayerPrefs.SetInt(ContinueLevelKey, levelId);
        PlayerPrefs.SetInt(HasSaveKey, 1);

        int highestLevel = HighestUnlockedLevel;

        if (levelId > highestLevel)
        {
            PlayerPrefs.SetInt(
                HighestUnlockedLevelKey,
                levelId
            );
        }

        PlayerPrefs.SetInt(AllLevelsCompletedKey, 0);

        PlayerPrefs.Save();

        Debug.Log(
            $"[WasteLevelProgress] 已保存当前关卡：{levelId}"
        );
    }

    /// <summary>
    /// 完成关卡后保存解锁进度。
    /// </summary>
    public static void CompleteLevel(
        int completedLevelId,
        int totalLevels)
    {
        int highestLevel = Mathf.Max(
            HighestUnlockedLevel,
            completedLevelId
        );

        PlayerPrefs.SetInt(
            HighestUnlockedLevelKey,
            highestLevel
        );

        if (completedLevelId >= totalLevels)
        {
            // 全部关卡已经完成
            PlayerPrefs.SetInt(
                AllLevelsCompletedKey,
                1
            );

            PlayerPrefs.SetInt(HasSaveKey, 0);
        }
        else
        {
            // 解锁下一关，并将继续游戏位置设置为下一关
            int nextLevelId = completedLevelId + 1;

            PlayerPrefs.SetInt(
                HighestUnlockedLevelKey,
                Mathf.Max(highestLevel, nextLevelId)
            );

            PlayerPrefs.SetInt(
                ContinueLevelKey,
                nextLevelId
            );

            PlayerPrefs.SetInt(HasSaveKey, 1);
            PlayerPrefs.SetInt(AllLevelsCompletedKey, 0);
        }

        PlayerPrefs.Save();

        Debug.Log(
            $"[WasteLevelProgress] 第 {completedLevelId} 关完成，存档已更新。"
        );
    }

    /// <summary>
    /// 清除关卡存档。
    /// 用于开发测试。
    /// </summary>
    public static void ClearProgress()
    {
        PlayerPrefs.DeleteKey(HighestUnlockedLevelKey);
        PlayerPrefs.DeleteKey(ContinueLevelKey);
        PlayerPrefs.DeleteKey(HasSaveKey);
        PlayerPrefs.DeleteKey(AllLevelsCompletedKey);

        PlayerPrefs.Save();

        Debug.Log("[WasteLevelProgress] 关卡存档已清除。");
    }
}