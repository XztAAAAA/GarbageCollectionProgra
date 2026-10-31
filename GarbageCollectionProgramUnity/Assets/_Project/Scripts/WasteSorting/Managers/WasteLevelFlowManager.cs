using Core;
using UnityEngine;

/// <summary>
/// 游戏流程管理器。
/// 负责主界面、游戏界面和结算界面的切换。
/// 不负责垃圾生成、分类和预处理。
/// </summary>
public class WasteLevelFlowManager : MonoSingleton<WasteLevelFlowManager>
{
    [Header("游戏面板 / 게임 패널")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject levelCompletePanel;

    private int currentLevelId = 1;

    public int CurrentLevelId => currentLevelId;

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
    /// 显示当前关卡。
    /// </summary>
    private void StartCurrentLevel()
    {
        SetPanelState(
            mainMenu: false,
            gameplay: true,
            levelComplete: false
        );

        Debug.Log(
            $"[WasteLevelFlowManager] 开始第 {currentLevelId} 关。"
        );

        // 通知关卡系统开始加载当前关卡
        GameEventBus.Publish(
            new WasteLevelStartRequestedEvent(currentLevelId)
        );
    }

    /// <summary>
    /// 显示主界面。
    /// </summary>
    public void ShowMainMenu()
    {
        SetPanelState(
            mainMenu: true,
            gameplay: false,
            levelComplete: false
        );

        Debug.Log("[WasteLevelFlowManager] 返回主界面。");
    }

    /// <summary>
    /// 显示关卡结算界面。
    /// 后续由关卡完成事件调用。
    /// </summary>
    public void ShowLevelComplete()
    {
        SetPanelState(
            mainMenu: false,
            gameplay: false,
            levelComplete: true
        );
    }

    /// <summary>
    /// 统一控制三个面板。
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