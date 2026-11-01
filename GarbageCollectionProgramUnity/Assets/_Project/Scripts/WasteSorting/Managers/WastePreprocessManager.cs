
using System.Collections.Generic;
using Core;
using UnityEngine;

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
    }

    #endregion

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
                "이제 분류 구역으로 드래그하세요.",
                "这件垃圾已经可以直接分类。"
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
            "먼저 필요한 전처리를 완료해 주세요.",
            "请先完成这件垃圾所需的预处理步骤。"
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

            // 关闭当前预处理面板。
            PublishPanelClosed(activeItemEntityId);

            PublishFeedback(
                "전처리가 완료되었습니다. 이제 분류해 주세요.",
                "预处理已完成，现在可以进行分类。"
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

        if (!TryGetStepInfo(
            step,
            out string koreanTitle,
            out string chineseTitle,
            out string koreanDescription,
            out string chineseDescription))
        {
            CommonTool.LogError(
                $"[WastePreprocessManager] 未处理的步骤类型：{step}"
            );
            return;
        }

        int currentStep = state.CurrentStepIndex + 1;
        int totalSteps = state.Steps.Count;

        string content =
            $"<align=center>" +
            $"<size=82%><color=#718176>{currentStep} / {totalSteps} 단계 · " +
            $"第 {currentStep} / {totalSteps} 步</color></size>\n" +
            $"<size=118%><b>{koreanTitle}</b></size>" +
            $"<size=88%><color=#718176>{chineseTitle}</color></size>\n" +
            $"<line-height=115%>{koreanDescription}</line-height>" +
            $"<size=90%><color=#718176>{chineseDescription}</color></size>" +
            $"</align>";

        GameEventBus.Publish(
            new WastePreprocessPanelChangedEvent(
                true,
                entityId,
                content,
                "단계 완료\n完成当前步骤"
            )
        );
    }

    /// <summary>
    /// 根据 Luban 枚举获取中韩双语说明。
    /// 暂时使用代码映射，后续可以迁移到 Luban。
    /// </summary>
    private bool TryGetStepInfo(
        cfg.waste.PreprocessType step,
        out string koreanTitle,
        out string chineseTitle,
        out string koreanDescription,
        out string chineseDescription)
    {
        switch (step)
        {
            case cfg.waste.PreprocessType.EMPTY_CONTENTS:

                koreanTitle = "내용물 비우기";
                chineseTitle = "倒空内容物";

                koreanDescription =
                    "용기 안의 내용물을 모두 비워 주세요.";

                chineseDescription =
                    "请将容器内的内容物倒空。";

                return true;

            case cfg.waste.PreprocessType.REMOVE_LABEL:

                koreanTitle = "라벨 제거하기";
                chineseTitle = "移除标签";

                koreanDescription =
                    "분리할 수 있는 라벨을 제거해 주세요.";

                chineseDescription =
                    "请移除可以分离的标签。";

                return true;

            case cfg.waste.PreprocessType.SEPARATE_MATERIALS:

                koreanTitle = "다른 재질 분리하기";
                chineseTitle = "分离不同材质";

                koreanDescription =
                    "서로 다른 재질의 부품을 분리해 주세요.";

                chineseDescription =
                    "请将不同材质的部件分开。";

                return true;

            case cfg.waste.PreprocessType.REMOVE_LIQUID:

                koreanTitle = "물기 제거하기";
                chineseTitle = "去除水分";

                koreanDescription =
                    "남아 있는 액체와 물기를 제거해 주세요.";

                chineseDescription =
                    "请去除残留液体和水分。";

                return true;

            case cfg.waste.PreprocessType.FLATTEN:

                koreanTitle = "압축하거나 접기";
                chineseTitle = "压扁或折叠";

                koreanDescription =
                    "부피를 줄일 수 있도록 눌러 주세요.";

                chineseDescription =
                    "请压扁或折叠，以减少体积。";

                return true;

            default:

                koreanTitle = string.Empty;
                chineseTitle = string.Empty;
                koreanDescription = string.Empty;
                chineseDescription = string.Empty;

                return false;
        }
    }

    #endregion

    #region Feedback

    private void PublishFeedback(
        string korean,
        string chinese)
    {
        GameEventBus.Publish(
            new WasteFeedbackEvent(
                korean,
                chinese
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
