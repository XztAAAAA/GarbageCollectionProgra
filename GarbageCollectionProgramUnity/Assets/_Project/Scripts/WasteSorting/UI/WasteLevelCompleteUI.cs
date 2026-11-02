
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
    private TMP_Text titleText;

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

    /// <summary>
    /// 检查 UI 引用。
    /// </summary>
    private void CheckReferences()
    {
        if (flowManager == null)
            CommonTool.LogError("[WasteLevelCompleteUI] FlowManager 未绑定。", this);

        if (titleText == null)
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
        CommonTool.PlaySound("level_rush_arrive");
        if (flowManager == null)
            return;

        int levelId = flowManager.CurrentLevelId;
        bool isLastLevel = flowManager.IsLastLevel;

        // 设置结算标题
        if (titleText != null)
        {
            if (isLastLevel)
            {
                titleText.text =
                    "ALL STAGES CLEAR!\n" +
                    "모든 단계 완료!\n" +
                    "全部关卡通关！";
            }
            else
            {
                titleText.text =
                    $"STAGE {levelId} CLEAR\n" +
                    $"제 {levelId} 단계 완료\n" +
                    $"第 {levelId} 关完成";
            }
        }

        // 设置结算结果
        if (resultText != null)
        {
            resultText.text =
                $"분류 완료: {eventData.CompletedItems}/{eventData.TotalItems}\n" +
                $"分类完成：{eventData.CompletedItems}/{eventData.TotalItems}";
        }

        // 设置下一关按钮
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(!isLastLevel);
            nextLevelButton.interactable = !isLastLevel;
        }

        if (nextLevelButtonText != null)
        {
            nextLevelButtonText.text =
                "다음 단계\n下一关";
        }

        // 设置返回主界面按钮
        if (mainMenuButtonText != null)
        {
            mainMenuButtonText.text =
                "메인 메뉴\n返回主界面";
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
