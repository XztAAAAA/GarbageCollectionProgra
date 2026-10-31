
using UnityEngine;

/// <summary>
/// 垃圾已经满足预处理要求，请求进行分类判断。
/// 由 WastePreprocessManager 发布，WasteGameManager 接收。
/// </summary>
public readonly struct WasteClassificationRequestedEvent
{
    public EntityId ItemEntityId { get; }

    public int ItemId { get; }

    public cfg.waste.WasteCategory ItemCategoryKey { get; }

    public cfg.waste.WasteCategory TargetCategoryKey { get; }

    public WasteClassificationRequestedEvent(
        EntityId itemEntityId,
        int itemId,
        cfg.waste.WasteCategory itemCategoryKey,
        cfg.waste.WasteCategory targetCategoryKey)
    {
        ItemEntityId = itemEntityId;
        ItemId = itemId;
        ItemCategoryKey = itemCategoryKey;
        TargetCategoryKey = targetCategoryKey;
    }
}
