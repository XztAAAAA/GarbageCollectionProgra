using DG.Tweening;
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

    [SerializeField]
    private Button settingButton;

    [SerializeField]
    private GameObject SettingPanel;
    private float continueButtonTextOriginalAlpha = 1f;
    private RectTransform title;
    private Vector3 titleOriginalScale;
    private Tween titleBreathingTween;

    private void Awake()
    {
        Transform titleTransform = transform.Find("Title");
        if (titleTransform != null)
        {
            title = titleTransform as RectTransform;
            titleOriginalScale = title.localScale;
        }
        else
        {
            CommonTool.LogError(
                "[WasteMainMenuUI] 找不到 Title 子物体，无法播放呼吸效果。",
                this
            );
        }

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
        PlayTitleBreathingEffect();
        TableSystem.LanguageChanged += OnLanguageChanged;

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

        settingButton.onClick.AddListener(OnSettingClicked);

        RefreshContinueButton();
    }

    private void OnDisable()
    {
        if (titleBreathingTween != null)
        {
            titleBreathingTween.Kill();
            titleBreathingTween = null;
        }

        if (title != null)
        {
            title.localScale = titleOriginalScale;
        }

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

        settingButton.onClick.RemoveListener(OnSettingClicked);

        TableSystem.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        RefreshContinueButton();
    }

    private void PlayTitleBreathingEffect()
    {
        if (title == null)
        {
            return;
        }

        titleBreathingTween?.Kill();
        title.localScale = titleOriginalScale;
        titleBreathingTween = title
            .DOScale(titleOriginalScale * 1.03f, 0.8f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
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
            continueButtonText.text = TableSystem.GetLanguage("allstages_clear");
        }
        else
        {
            continueButtonText.text = TableSystem.GetLanguage("menu_continue");
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

    private void OnSettingClicked()
    {
        SettingPanel.SetActive(true);
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