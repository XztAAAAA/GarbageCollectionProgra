using Core;
using UnityEngine;

/// <summary>
/// 游戏流程管理器。
/// 负责主界面、游戏界面、结算界面和关卡流程。
/// </summary>
public class WasteLevelFlowManager : MonoSingleton<WasteLevelFlowManager>
{
    [Header("游戏面板 / 게임 패널")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject levelCompletePanel;

    private int currentLevelId = 1;

    private bool isCurrentLevelCompleted;

    public int CurrentLevelId => currentLevelId;

    /// <summary>
    /// 是否存在可继续的游戏存档。
    /// </summary>
    public bool CanContinueGame
    {
        get
        {
            if (!WasteLevelProgress.HasContinueData)
                return false;

            int levelId = WasteLevelProgress.ContinueLevelId;
            int totalLevels = TableSystem.Tables.TbLevel.DataList.Count;

            return levelId >= 1 && levelId <= totalLevels;
        }
    }

    /// <summary>
    /// 当前是否为最后一关。
    /// </summary>
    public bool IsLastLevel
    {
        get
        {
            int totalLevels =
                TableSystem.Tables.TbLevel.DataList.Count;

            return currentLevelId >= totalLevels;
        }
    }

    private void OnEnable()
    {
        GameEventBus.Subscribe<WasteLevelCompletedEvent>(
            OnWasteLevelCompleted
        );
    }

    private void OnDisable()
    {
        GameEventBus.Unsubscribe<WasteLevelCompletedEvent>(
            OnWasteLevelCompleted
        );
    }

    private void Start()
    {
        AudioTool.PlayBackgroundMusic("lilvillage", 0.1f);
        ShowMainMenu();
    }

    /// <summary>
    /// 开始新游戏。
    /// 重置原有的关卡进度。
    /// </summary>
    public void StartNewGame()
    {
        WasteLevelProgress.ResetForNewGame();

        currentLevelId = 1;

        StartCurrentLevel();
    }

    /// <summary>
    /// 从存档中继续游戏。
    /// </summary>
    public void ContinueGame()
    {
        if (!CanContinueGame)
        {
            CommonTool.LogWarning(
                "[WasteLevelFlowManager] 没有可继续的游戏存档。"
            );
            return;
        }

        currentLevelId = WasteLevelProgress.ContinueLevelId;

        CommonTool.Log(
            $"[WasteLevelFlowManager] 从存档继续第 {currentLevelId} 关。"
        );

        StartCurrentLevel();
    }

    public bool JumpToLevel(int levelId)
    {
        if (TableSystem.Tables.TbLevel.GetOrDefault(levelId) == null)
        {
            CommonTool.LogError(
                $"[WasteLevelFlowManager] 找不到第 {levelId} 关配置，无法跳转。"
            );
            return false;
        }

        currentLevelId = levelId;
        StartCurrentLevel();
        return true;
    }

    /// <summary>
    /// 开始当前关卡。
    /// </summary>
    private void StartCurrentLevel()
    {
        isCurrentLevelCompleted = false;

        // 保存当前关卡
        WasteLevelProgress.SaveCurrentLevel(currentLevelId);

        // 先激活 GameplayPanel，让 Spawner 完成事件订阅
        SetPanelState(false, true, false);

        CommonTool.Log(
            $"[WasteLevelFlowManager] 开始第 {currentLevelId} 关。"
        );

        // 通知垃圾生成系统开始生成
        GameEventBus.Publish(
            new WasteLevelStartRequestedEvent(currentLevelId)
        );
    }

    /// <summary>
    /// 接收关卡完成事件。
    /// </summary>
    private void OnWasteLevelCompleted(
        WasteLevelCompletedEvent eventData)
    {
        if (isCurrentLevelCompleted)
            return;

        isCurrentLevelCompleted = true;

        int totalLevels =
            TableSystem.Tables.TbLevel.DataList.Count;

        // 保存已完成关卡和下一关解锁情况
        WasteLevelProgress.CompleteLevel(
            currentLevelId,
            totalLevels
        );

        CommonTool.Log(
            $"[WasteLevelFlowManager] 第 {currentLevelId} 关完成。"
        );

        levelCompletePanel.GetComponent<WasteLevelCompleteUI>().OnLevelCompleted(eventData);
        ShowLevelComplete();
    }

    /// <summary>
    /// 显示关卡结算界面。
    /// </summary>
    public void ShowLevelComplete()
    {
        SetPanelState(false, true, true);
    }

    /// <summary>
    /// 进入下一关。
    /// </summary>
    public void NextLevel()
    {
        if (!isCurrentLevelCompleted)
        {
            CommonTool.LogWarning(
                "[WasteLevelFlowManager] 当前关卡尚未完成。"
            );
            return;
        }

        if (IsLastLevel)
        {
            ShowMainMenu();
            return;
        }

        currentLevelId++;

        StartCurrentLevel();
    }

    /// <summary>
    /// 返回主界面。
    /// </summary>
    public void ShowMainMenu()
    {
        SetPanelState(true, false, false);

        CommonTool.Log(
            "[WasteLevelFlowManager] 返回主界面。"
        );
    }

    /// <summary>
    /// 统一控制面板显示。
    /// </summary>
    private void SetPanelState(
        bool mainMenu,
        bool gameplay,
        bool levelComplete)
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(mainMenu);

        if (gameplayPanel != null)
            gameplayPanel.SetActive(gameplay);

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(levelComplete);
    }
}