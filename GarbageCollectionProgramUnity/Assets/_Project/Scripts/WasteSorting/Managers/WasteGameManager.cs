using System.Collections.Generic;
using Core;
using UnityEngine;

/// <summary>
/// 垃圾分类游戏数据管理器。
/// 负责关卡数据、分类结果和进度统计。
/// 不负责任何 UI 显示。
/// </summary>
public class WasteGameManager : MonoSingleton<WasteGameManager>
{
    [Header("关卡设置 / 단계 설정")]
    [SerializeField] private int currentStage = 1;

    private int totalItems;
    private int completedItems;

    // 防止同一个垃圾被重复计分
    private readonly HashSet<EntityId> completedItemIds =
        new HashSet<EntityId>();

    public int CurrentStage => currentStage;
    public int TotalItems => totalItems;
    public int CompletedItems => completedItems;

    public int RemainingItems =>
        Mathf.Max(0, totalItems - completedItems);

    public float Progress =>
        totalItems > 0
            ? (float)completedItems / totalItems
            : 0f;

    private bool isLevelCompleted;

    private void OnEnable()
    {
        GameEventBus.Subscribe<WasteLevelStartRequestedEvent>(
            OnWasteLevelStartRequested
        );

        GameEventBus.Subscribe<WasteItemsSpawnedEvent>(
            OnWasteItemsSpawned
        );

        GameEventBus.Subscribe<WasteClassificationRequestedEvent>(
            OnWasteClassificationRequested
        );
    }

    private void OnDisable()
    {
        GameEventBus.Unsubscribe<WasteLevelStartRequestedEvent>(
            OnWasteLevelStartRequested
        );

        GameEventBus.Unsubscribe<WasteItemsSpawnedEvent>(
            OnWasteItemsSpawned
        );

        GameEventBus.Unsubscribe<WasteClassificationRequestedEvent>(
            OnWasteClassificationRequested
        );
    }

    private void OnWasteLevelStartRequested(
        WasteLevelStartRequestedEvent eventData)
    {
        currentStage = Mathf.Max(1, eventData.LevelId);
        completedItems = 0;
        totalItems = 0;
        completedItemIds.Clear();
        isLevelCompleted = false;
    }

    /// <summary>
    /// 接收垃圾全部生成完成事件。
    /// </summary>
    private void OnWasteItemsSpawned(
        WasteItemsSpawnedEvent eventData)
    {
        totalItems = Mathf.Max(0, eventData.TotalItems);
        completedItems = 0;

        completedItemIds.Clear();

        isLevelCompleted = false;
        PublishProgress();

        if (totalItems == 0)
        {
            PublishFeedback(
                "분류할 쓰레기가 없습니다.",
                "当前没有可分类的垃圾。"
            );

            return;
        }

        PublishFeedback(
            "쓰레기를 분류 구역으로 드래그하세요.",
            "请将垃圾拖到对应的分类区域。"
        );
    }

    /// <summary>
    /// 接收已经通过预处理检查的分类请求。
    /// 负责判断分类是否正确。
    /// </summary>
    private void OnWasteClassificationRequested(
        WasteClassificationRequestedEvent eventData)
    {
        // 比较垃圾实际分类和玩家选择的分类区域
        bool isCorrect =
            eventData.ItemCategoryKey == eventData.TargetCategoryKey;

        // 通知拖拽脚本分类结果。
        // 此时已经通过预处理检查，所以 RequiresPreprocessing 为 false。
        GameEventBus.Publish(
            new WasteDropResultEvent(
                eventData.ItemEntityId,
                eventData.ItemId,
                eventData.ItemCategoryKey,
                eventData.TargetCategoryKey,
                isCorrect,
                false
            )
        );

        // 分类错误，不增加进度
        if (!isCorrect)
        {
            PublishFeedback(
                "분류가 올바르지 않아요. 다시 확인해 보세요.",
                "分类不正确，请再检查一下。"
            );

            return;
        }

        // 防止同一个垃圾重复计分
        if (!completedItemIds.Add(eventData.ItemEntityId))
            return;

        completedItems = Mathf.Min(
            completedItems + 1,
            totalItems
        );

        PublishProgress();

        if (completedItems >= totalItems && !isLevelCompleted)
        {
            isLevelCompleted = true;

            PublishFeedback(
                "모든 쓰레기를 분류했어요! 잘했어요!",
                "所有垃圾都分类完成了！做得很好！"
            );

            GameEventBus.Publish(
                new WasteLevelCompletedEvent(
                    totalItems,
                    completedItems
                )
            );
        }
        else
        {
            PublishFeedback(
                "정답이에요! 잘했어요!",
                "分类正确！做得很好！"
            );
        }
    }

    /// <summary>
    /// 发布进度变化事件。
    /// </summary>
    private void PublishProgress()
    {
        GameEventBus.Publish(
            new WasteProgressChangedEvent(
                currentStage,
                totalItems,
                completedItems
            )
        );
    }

    /// <summary>
    /// 发布双语反馈信息事件。
    /// </summary>
    private void PublishFeedback(
        string korean,
        string chinese)
    {
        GameEventBus.Publish(
            new WasteFeedbackEvent(korean, chinese)
        );
    }
}