using Core;
using UnityEngine;

/// <summary>
/// 游戏流程管理器。
/// 负责主界面、游戏界面和关卡结算界面的切换。
/// </summary>
public class WasteLevelFlowManager : MonoSingleton<WasteLevelFlowManager>
{
    [Header("游戏面板 / 게임 패널")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject levelCompletePanel;

    private int currentLevelId = 1;

    public int CurrentLevelId => currentLevelId;

    /// <summary>
    /// 当前关卡是否为最后一关。
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
        ShowMainMenu();
    }

    /// <summary>
    /// 开始新游戏。
    /// </summary>
    public void StartNewGame()
    {
        currentLevelId = 1;
        StartCurrentLevel();
    }

    /// <summary>
    /// 开始当前关卡。
    /// </summary>
    private void StartCurrentLevel()
    {
        SetPanelState(false, true, false);

        Debug.Log(
            $"[WasteLevelFlowManager] 开始第 {currentLevelId} 关。"
        );

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
        Debug.Log(
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
        if (IsLastLevel)
        {
            Debug.Log(
                "[WasteLevelFlowManager] 已经完成全部关卡。"
            );

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

        Debug.Log(
            "[WasteLevelFlowManager] 返回主界面。"
        );
    }

    /// <summary>
    /// 统一控制界面显示。
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