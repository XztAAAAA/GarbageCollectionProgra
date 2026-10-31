using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 主界面 UI 控制器。
/// 负责开始游戏、继续游戏和退出游戏按钮。
/// </summary>
public class WasteMainMenuUI : MonoBehaviour
{
    [Header("流程管理器 / 게임 흐름 관리자")]
    [SerializeField] private WasteLevelFlowManager flowManager;

    [Header("主界面按钮 / 메인 메뉴 버튼")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        // 暂时没有实现存档系统，先禁用继续游戏
        if (continueButton != null)
        {
            continueButton.interactable = false;
        }
    }

    private void OnEnable()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClicked);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnDisable()
    {
        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(OnStartGameClicked);

        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnExitClicked);
    }

    /// <summary>
    /// 开始游戏。
    /// </summary>
    private void OnStartGameClicked()
    {
        if (flowManager == null)
        {
            Debug.LogError(
                "[WasteMainMenuUI] 没有绑定 WasteLevelFlowManager。",
                this
            );
            return;
        }

        flowManager.StartNewGame();
    }

    /// <summary>
    /// 继续游戏。
    /// 后续接入存档系统。
    /// </summary>
    private void OnContinueClicked()
    {
        Debug.Log(
            "[WasteMainMenuUI] 继续游戏功能尚未实现。"
        );
    }

    /// <summary>
    /// 退出游戏。
    /// </summary>
    private void OnExitClicked()
    {
#if UNITY_EDITOR
        Debug.Log("[WasteMainMenuUI] 编辑器中无法直接退出游戏。");
#else
        Application.Quit();
#endif
    }
}