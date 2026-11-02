
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 关卡完成 UI。
/// 负责结算文本和按钮交互，不负责面板开关。
/// </summary>
public class WasteLevelCompleteUI : MonoBehaviour
{
    [Header("流程管理器 / 게임 흐름 관리자")]
    [SerializeField]
    private WasteLevelFlowManager flowManager;

    [Header("结算文本 / 결과 텍스트")]
    [SerializeField]
    private TMP_Text descText;

    [SerializeField]
    private TMP_Text resultText;

    [Header("下一关按钮 / 다음 단계 버튼")]
    [SerializeField]
    private Button nextLevelButton;

    [SerializeField]
    private TMP_Text nextLevelButtonText;

    [Header("返回主界面按钮 / 메인 메뉴 버튼")]
    [SerializeField]
    private Button mainMenuButton;

    [SerializeField]
    private TMP_Text mainMenuButtonText;

    private RectTransform content;
    private Tween contentPopupTween;

    private void Awake()
    {
        // 自动获取按钮下的 TMP 文字组件
        if (nextLevelButtonText == null && nextLevelButton != null)
        {
            nextLevelButtonText =
                nextLevelButton.GetComponentInChildren<TMP_Text>(true);
        }

        if (mainMenuButtonText == null && mainMenuButton != null)
        {
            mainMenuButtonText =
                mainMenuButton.GetComponentInChildren<TMP_Text>(true);
        }

        CheckReferences();
    }

    private void OnEnable()
    {
        PlayContentPopup();

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(
                OnNextLevelClicked
            );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(
                OnMainMenuClicked
            );
        }
    }

    private void OnDisable()
    {
        if (contentPopupTween != null)
        {
            contentPopupTween.Kill();
            contentPopupTween = null;
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveListener(
                OnNextLevelClicked
            );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(
                OnMainMenuClicked
            );
        }
    }

    private void PlayContentPopup()
    {
        if (content == null)
        {
            content = transform.Find("Content") as RectTransform;
        }

        if (content == null)
        {
            CommonTool.LogError(
                "[WasteLevelCompleteUI] 找不到 Content 子物体，无法播放弹出动画。",
                this
            );
            return;
        }

        if (contentPopupTween != null)
        {
            contentPopupTween.Kill();
        }

        content.localScale = Vector3.one * 0.75f;
        contentPopupTween = content
            .DOScale(Vector3.one, 0.35f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    /// <summary>
    /// 检查 UI 引用。
    /// </summary>
    private void CheckReferences()
    {
        if (flowManager == null)
            CommonTool.LogError("[WasteLevelCompleteUI] FlowManager 未绑定。", this);

        if (descText == null)
            CommonTool.LogError("[WasteLevelCompleteUI] TitleText 未绑定。", this);

        if (resultText == null)
            CommonTool.LogError("[WasteLevelCompleteUI] ResultText 未绑定。", this);

        if (nextLevelButton == null)
            CommonTool.LogError("[WasteLevelCompleteUI] NextLevelButton 未绑定。", this);

        if (nextLevelButtonText == null)
            CommonTool.LogError("[WasteLevelCompleteUI] NextLevelButtonText 未找到。", this);

        if (mainMenuButton == null)
            CommonTool.LogError("[WasteLevelCompleteUI] MainMenuButton 未绑定。", this);

        if (mainMenuButtonText == null)
            CommonTool.LogError("[WasteLevelCompleteUI] MainMenuButtonText 未找到。", this);
    }

    /// <summary>
    /// 接收关卡完成事件并更新结算内容。
    /// </summary>
    public void OnLevelCompleted(
        WasteLevelCompletedEvent eventData)
    {
        AudioTool.PlaySound("level_rush_arrive", 0.8f);
        if (flowManager == null)
            return;

        int levelId = flowManager.CurrentLevelId;
        bool isLastLevel = flowManager.IsLastLevel;

        // 设置结算标题
        if (descText != null)
        {
            if (isLastLevel)
            {
                descText.text = TableSystem.GetLanguage("allstages_clear");
            }
            else
            {
                descText.text =TableSystem.GetLanguage("stageslevel_clear", levelId);
            }
        }

        // 设置结算结果
        if (resultText != null)
        {
            resultText.text = TableSystem.GetLanguage("level_complete_score",eventData.CompletedItems,eventData.TotalItems);
        }

        // 设置下一关按钮
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(!isLastLevel);
            nextLevelButton.interactable = !isLastLevel;
        }

        if (nextLevelButtonText != null)
        {
            nextLevelButtonText.text = TableSystem.GetLanguage("common_next_level");
        }

        // 设置返回主界面按钮
        if (mainMenuButtonText != null)
        {
            mainMenuButtonText.text = TableSystem.GetLanguage("common_back");
        }

        CommonTool.Log(
            $"[WasteLevelCompleteUI] " +
            $"第 {levelId} 关结算内容更新完成，" +
            $"完成数量：{eventData.CompletedItems}/{eventData.TotalItems}"
        );
    }

    /// <summary>
    /// 点击下一关。
    /// </summary>
    private void OnNextLevelClicked()
    {
        if (flowManager == null)
            return;

        flowManager.NextLevel();
    }

    /// <summary>
    /// 点击返回主界面。
    /// </summary>
    private void OnMainMenuClicked()
    {
        if (flowManager == null)
            return;

        flowManager.ShowMainMenu();
    }
}
