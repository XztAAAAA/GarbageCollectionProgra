using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 主界面 UI 控制器。
/// 负责开始游戏、继续游戏和退出游戏。
/// </summary>
public class WasteMainMenuUI : MonoBehaviour
{
    [Header("流程管理器 / 게임 흐름 관리자")]
    [SerializeField]
    private WasteLevelFlowManager flowManager;

    [Header("主界面按钮 / 메인 메뉴 버튼")]
    [SerializeField]
    private Button startGameButton;

    [SerializeField]
    private Button continueButton;

    [SerializeField]
    private TMP_Text continueButtonText;

    [SerializeField]
    private Button exitButton;

    private float continueButtonTextOriginalAlpha = 1f;

    private void Awake()
    {
        // 自动获取继续游戏按钮的 TMP 文字
        if (continueButtonText == null && continueButton != null)
        {
            continueButtonText =
                continueButton.GetComponentInChildren<TMP_Text>(true);
        }

        if (continueButtonText != null)
        {
            continueButtonTextOriginalAlpha =
                continueButtonText.color.a;
        }
    }

    private void OnEnable()
    {
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(
                OnStartGameClicked
            );
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(
                OnContinueClicked
            );
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(
                OnExitClicked
            );
        }

        RefreshContinueButton();
    }

    private void OnDisable()
    {
        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveListener(
                OnStartGameClicked
            );
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(
                OnContinueClicked
            );
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(
                OnExitClicked
            );
        }
    }

    /// <summary>
    /// 根据存档更新继续游戏按钮。
    /// </summary>
    private void RefreshContinueButton()
    {
        if (flowManager == null || continueButton == null)
            return;

        bool canContinue = flowManager.CanContinueGame;

        continueButton.interactable = canContinue;

        if (continueButtonText != null)
        {
            ColorBlock colors = continueButton.colors;
            float targetAlpha = continueButtonTextOriginalAlpha *
                (canContinue ? 1f : colors.disabledColor.a);

            continueButtonText.CrossFadeAlpha(
                targetAlpha,
                colors.fadeDuration,
                true
            );
        }

        if (continueButtonText == null)
            return;

        if (WasteLevelProgress.IsAllLevelsCompleted)
        {
            continueButtonText.text =
                "모든 단계 완료\n全部关卡已完成";
        }
        else
        {
            continueButtonText.text =
                "이어하기\n继续游戏";
        }

        CommonTool.Log(
            $"[WasteMainMenuUI] 继续游戏按钮状态：{canContinue}"
        );
    }

    /// <summary>
    /// 开始新游戏。
    /// </summary>
    private void OnStartGameClicked()
    {
        if (flowManager == null)
        {
            CommonTool.LogError(
                "[WasteMainMenuUI] 没有绑定 WasteLevelFlowManager。",
                this
            );
            return;
        }

        flowManager.StartNewGame();
    }

    /// <summary>
    /// 继续上一次的游戏。
    /// </summary>
    private void OnContinueClicked()
    {
        if (flowManager == null)
            return;

        if (!flowManager.CanContinueGame)
        {
            RefreshContinueButton();
            return;
        }

        flowManager.ContinueGame();
    }

    /// <summary>
    /// 退出游戏。
    /// </summary>
    private void OnExitClicked()
    {
#if UNITY_EDITOR
        CommonTool.Log(
            "[WasteMainMenuUI] 编辑器中无法直接退出游戏。"
        );
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 在 Inspector 右键组件时，可用于更新按钮状态。
    /// </summary>
    [ContextMenu("刷新继续游戏按钮")]
    private void RefreshContinueButtonInEditor()
    {
        RefreshContinueButton();
    }
}