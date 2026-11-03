
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 垃圾预处理管理器。
/// 负责垃圾预处理状态、步骤进度和投放前校验，
/// 并只控制预处理面板的开关；不负责 UI 内容及按钮交互，
/// 也不负责最终分类判断。
/// </summary>
public class WastePreprocessManager : MonoSingleton<WastePreprocessManager>
{
    [Header("预处理面板 / 전처리 패널")]
    [SerializeField] private WastePreprocessUI preprocessPanel;

    /// <summary>
    /// 单件垃圾的预处理运行状态。
    /// </summary>
    private class ItemPreprocessState
    {
        public int ItemId;

        public string KoreanName;
        public string ChineseName;

        public List<cfg.waste.PreprocessType> Steps =
            new List<cfg.waste.PreprocessType>();

        public int CurrentStepIndex;

        public bool IsCompleted;
    }

    // 通过 EntityId 区分场景中的每件垃圾。
    private readonly Dictionary<EntityId, ItemPreprocessState> itemStates =
        new Dictionary<EntityId, ItemPreprocessState>();

    // 当前正在操作预处理面板的垃圾。
    private EntityId activeItemEntityId;

    private bool hasActiveItem;

    private void Awake()
    {
        if (preprocessPanel == null)
        {
            CommonTool.LogError(
                "[WastePreprocessManager] PreprocessPanel 未绑定。",
                this
            );
            return;
        }

        // 由 Manager 统一控制面板初始状态。
        preprocessPanel.gameObject.SetActive(false);
    }

    #region Event Subscription

    private void OnEnable()
    {
        GameEventBus.Subscribe<WasteItemSpawnedEvent>(
            OnWasteItemSpawned
        );

        GameEventBus.Subscribe<WasteItemSelectedEvent>(
            OnWasteItemSelected
        );

        GameEventBus.Subscribe<WasteDropAttemptEvent>(
            OnWasteDropAttempt
        );

        GameEventBus.Subscribe<PreprocessStepCompleteRequestedEvent>(
            OnStepCompleteRequested
        );

        // Manager 只接收面板状态并控制开关。
        GameEventBus.Subscribe<WastePreprocessPanelChangedEvent>(
            OnPreprocessPanelChanged
        );
        TableSystem.LanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        GameEventBus.Unsubscribe<WasteItemSpawnedEvent>(
            OnWasteItemSpawned
        );

        GameEventBus.Unsubscribe<WasteItemSelectedEvent>(
            OnWasteItemSelected
        );

        GameEventBus.Unsubscribe<WasteDropAttemptEvent>(
            OnWasteDropAttempt
        );

        GameEventBus.Unsubscribe<PreprocessStepCompleteRequestedEvent>(
            OnStepCompleteRequested
        );

        GameEventBus.Unsubscribe<WastePreprocessPanelChangedEvent>(
            OnPreprocessPanelChanged
        );
        TableSystem.LanguageChanged -= OnLanguageChanged;
    }

    #endregion

    private void OnLanguageChanged()
    {
        if (hasActiveItem &&
            itemStates.TryGetValue(
                activeItemEntityId,
                out ItemPreprocessState state
            ))
        {
            PublishCurrentStep(activeItemEntityId, state);
        }
    }

    /// <summary>
    /// 只负责控制预处理面板的显示和隐藏。
    /// 字体、按钮内容及按钮交互由 WastePreprocessUI 负责。
    /// </summary>
    private void OnPreprocessPanelChanged(
        WastePreprocessPanelChangedEvent eventData)
    {
        if (preprocessPanel == null)
        {
            CommonTool.LogError(
                "[WastePreprocessManager] PreprocessPanel 未绑定。",
                this
            );
            return;
        }

        int? itemId = null;
        if (itemStates.TryGetValue(
            eventData.ItemEntityId,
            out ItemPreprocessState state))
        {
            itemId = state.ItemId;
        }
        else if (eventData.IsVisible)
        {
            CommonTool.LogError(
                $"[WastePreprocessManager] 找不到垃圾预处理状态，无法传递配置 ID：" +
                $"{eventData.ItemEntityId}"
            );
            return;
        }

        preprocessPanel.gameObject.SetActive(eventData.IsVisible);
        preprocessPanel.OnPreprocessPanelChanged(eventData, itemId);

        CommonTool.Log(
            $"[WastePreprocessManager] PreprocessPanel 状态：{eventData.IsVisible}"
        );
    }

    #region Initialize Item

    /// <summary>
    /// 每生成一件垃圾时初始化其预处理状态。
    /// </summary>
    private void OnWasteItemSpawned(
        WasteItemSpawnedEvent eventData)
    {
        if (itemStates.ContainsKey(eventData.ItemEntityId))
        {
            CommonTool.LogWarning(
                $"[WastePreprocessManager] 垃圾已经初始化：" +
                $"{eventData.ItemId}"
            );
            return;
        }

        var data = TableSystem.Tables.TbWasteItem.Get(
            eventData.ItemId
        );

        if (data == null)
        {
            CommonTool.LogError(
                $"[WastePreprocessManager] 找不到垃圾数据：" +
                $"{eventData.ItemId}"
            );
            return;
        }

        ItemPreprocessState state = new ItemPreprocessState
        {
            ItemId = data.Id,
            KoreanName = data.KoreanName,
            ChineseName = data.ChineseName,
            CurrentStepIndex = 0
        };

        // 复制 Luban 中配置的预处理步骤。
        if (data.PreprocessingSteps != null)
        {
            for (int i = 0; i < data.PreprocessingSteps.Count; i++)
            {
                state.Steps.Add(
                    data.PreprocessingSteps[i]
                );
            }
        }

        // 没有预处理步骤，则直接视为已完成。
        state.IsCompleted = state.Steps.Count == 0;

        itemStates.Add(
            eventData.ItemEntityId,
            state
        );

        CommonTool.Log(
            $"[WastePreprocessManager] 初始化垃圾：" +
            $"{state.KoreanName} / {state.ChineseName}，" +
            $"预处理步骤：{state.Steps.Count}"
        );
    }

    #endregion

    #region Item Selection

    /// <summary>
    /// 玩家点击垃圾时触发。
    /// </summary>
    private void OnWasteItemSelected(
    WasteItemSelectedEvent eventData)
    {
        CommonTool.Log(
            $"[WastePreprocessManager] 收到垃圾点击事件：" +
            $"ItemId = {eventData.ItemId}, " +
            $"EntityId = {eventData.ItemEntityId}"
        );

        // 检查垃圾是否已经初始化
        if (!itemStates.TryGetValue(
            eventData.ItemEntityId,
            out ItemPreprocessState state))
        {
            CommonTool.LogWarning(
                $"[WastePreprocessManager] 垃圾尚未初始化：" +
                $"ItemId = {eventData.ItemId}, " +
                $"当前状态数量 = {itemStates.Count}"
            );

            return;
        }

        CommonTool.Log(
            $"[WastePreprocessManager] 找到垃圾预处理状态：" +
            $"步骤总数 = {state.Steps.Count}, " +
            $"当前步骤 = {state.CurrentStepIndex}, " +
            $"是否完成 = {state.IsCompleted}"
        );

        // 不需要预处理，直接提示玩家分类
        if (state.IsCompleted)
        {
            PublishFeedback(
                "feedback_ready_to_sort"
            );

            CommonTool.Log(
                "[WastePreprocessManager] 该垃圾不需要预处理，不打开面板。"
            );

            return;
        }

        // 记录当前操作的垃圾
        activeItemEntityId = eventData.ItemEntityId;
        hasActiveItem = true;

        CommonTool.Log(
            "[WastePreprocessManager] 准备显示预处理面板。"
        );

        PublishCurrentStep(
            activeItemEntityId,
            state
        );
    }

    #endregion

    #region Drop Validation

    /// <summary>
    /// 玩家尝试投放垃圾时检查预处理状态。
    /// </summary>
    private void OnWasteDropAttempt(
        WasteDropAttemptEvent eventData)
    {
        if (!itemStates.TryGetValue(
            eventData.ItemEntityId,
            out ItemPreprocessState state))
        {
            CommonTool.LogError(
                $"[WastePreprocessManager] 投放校验失败，" +
                $"垃圾状态未初始化：{eventData.ItemId}"
            );

            PublishRejectedDrop(
                eventData,
                true
            );

            return;
        }

        // 如果还没有完成预处理，拒绝本次投放。
        if (!state.IsCompleted)
        {
            CommonTool.Log(
                $"[WastePreprocessManager] 尚未完成预处理，" +
                $"拒绝投放：{eventData.ItemId}"
            );

            PublishRejectedDrop(
                eventData,
                true
            );

            return;
        }

        // 预处理已完成，允许进入正式分类判断。
        CommonTool.Log(
            $"[WastePreprocessManager] 预处理检查通过：" +
            $"{eventData.ItemId}"
        );

        GameEventBus.Publish(
            new WasteClassificationRequestedEvent(
                eventData.ItemEntityId,
                eventData.ItemId,
                eventData.ItemCategoryKey,
                eventData.TargetCategoryKey
            )
        );
    }

    /// <summary>
    /// 发布预处理未完成的投放结果。
    /// </summary>
    private void PublishRejectedDrop(
        WasteDropAttemptEvent eventData,
        bool requiresPreprocessing)
    {
        GameEventBus.Publish(
            new WasteDropResultEvent(
                eventData.ItemEntityId,
                eventData.ItemId,
                eventData.ItemCategoryKey,
                eventData.TargetCategoryKey,
                false,
                requiresPreprocessing
            )
        );

        PublishFeedback(
            "feedback_preprocess_required"
        );
    }

    #endregion

    #region Preprocess Step

    /// <summary>
    /// 玩家点击完成步骤按钮时触发。
    /// </summary>
    private void OnStepCompleteRequested(
        PreprocessStepCompleteRequestedEvent eventData)
    {
        if (!hasActiveItem)
            return;

        // 只处理当前面板对应的垃圾。
        if (!EqualityComparer<EntityId>.Default.Equals(
            activeItemEntityId,
            eventData.ItemEntityId))
        {
            return;
        }

        if (!itemStates.TryGetValue(
            activeItemEntityId,
            out ItemPreprocessState state))
        {
            return;
        }

        if (state.IsCompleted)
            return;

        // 完成当前步骤。
        state.CurrentStepIndex++;

        CommonTool.Log(
            $"[WastePreprocessManager] 步骤进度：" +
            $"{state.CurrentStepIndex}/{state.Steps.Count}"
        );

        // 全部步骤完成。
        if (state.CurrentStepIndex >= state.Steps.Count)
        {
            state.IsCompleted = true;

            AudioTool.PlaySound("level_rush_move");

            // 关闭当前预处理面板。
            PublishPanelClosed(activeItemEntityId);

            PublishFeedback(
                "feedback_preprocess_completed"
            );

            hasActiveItem = false;

            return;
        }

        // 还有剩余步骤，显示下一步。
        PublishCurrentStep(
            activeItemEntityId,
            state
        );
    }

    #endregion

    #region Panel Information

    /// <summary>
    /// 发布当前预处理步骤的信息。
    /// </summary>
    private void PublishCurrentStep(
        EntityId entityId,
        ItemPreprocessState state)
    {
        if (state.CurrentStepIndex < 0 ||
            state.CurrentStepIndex >= state.Steps.Count)
        {
            return;
        }

        cfg.waste.PreprocessType step =
            state.Steps[state.CurrentStepIndex];
        var message = TableSystem.Tables.TbPreprocessMessqge.Get(step);

        int currentStep = state.CurrentStepIndex + 1;
        int totalSteps = state.Steps.Count;

        string content =
            $"<align=center>" +
            $"<size=82%>{TableSystem.GetLanguage("preprocess_step", currentStep, totalSteps)}\n" +
            $"<size=118%><b>{TableSystem.GetLanguage(message.Title)}</b></size>\n" +
            $"<line-height=115%>{TableSystem.GetLanguage(message.Description)}</line-height>" +
            $"</align>";

        GameEventBus.Publish(
            new WastePreprocessPanelChangedEvent(
                true,
                entityId,
                content,
                TableSystem.GetLanguage("preprocess_button_complete")
            )
        );
    }

    #endregion

    #region Feedback

    private void PublishFeedback(string languageKey)
    {
        GameEventBus.Publish(
            new WasteFeedbackEvent(
                TableSystem.GetLanguage(languageKey),
                languageKey
            )
        );
    }

    private void PublishPanelClosed(EntityId entityId)
    {
        GameEventBus.Publish(
            new WastePreprocessPanelChangedEvent(
                false,
                entityId,
                string.Empty,
                string.Empty
            )
        );
    }

    #endregion
}
