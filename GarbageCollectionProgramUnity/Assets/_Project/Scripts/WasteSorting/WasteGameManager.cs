
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

    private void OnEnable()
    {
        GameEventBus.Subscribe<WasteItemsSpawnedEvent>(
            OnWasteItemsSpawned
        );

        GameEventBus.Subscribe<WasteDropResultEvent>(
            OnWasteDropResult
        );
    }

    private void OnDisable()
    {
        GameEventBus.Unsubscribe<WasteItemsSpawnedEvent>(
            OnWasteItemsSpawned
        );

        GameEventBus.Unsubscribe<WasteDropResultEvent>(
            OnWasteDropResult
        );
    }

    /// <summary>
    /// 接收垃圾生成完成事件。
    /// </summary>
    private void OnWasteItemsSpawned(
        WasteItemsSpawnedEvent eventData)
    {
        totalItems = Mathf.Max(0, eventData.TotalItems);
        completedItems = 0;

        completedItemIds.Clear();

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
    /// 接收垃圾分类结果事件。
    /// </summary>
    private void OnWasteDropResult(
        WasteDropResultEvent eventData)
    {
        // 分类错误，不增加进度。
        if (!eventData.IsCorrect)
        {
            PublishFeedback(
                "분류가 올바르지 않아요. 다시 확인해 보세요.",
                "分类不正确，请再检查一下。"
            );
            return;
        }

        // 防止重复计分。
        if (!completedItemIds.Add(eventData.ItemEntityId))
            return;

        completedItems = Mathf.Min(
            completedItems + 1,
            totalItems
        );

        PublishProgress();

        if (completedItems >= totalItems)
        {
            PublishFeedback(
                "모든 쓰레기를 분류했어요! 잘했어요!",
                "所有垃圾都分类完成了！做得很好！"
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
    /// 发布反馈信息事件。
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
