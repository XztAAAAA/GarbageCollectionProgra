
using UnityEngine;

/// <summary>
/// 玩家尝试将垃圾投放到某个分类区域时发布。
/// 由 WasteDropZone 发布，WastePreprocessManager 接收。
/// </summary>
public readonly struct WasteDropAttemptEvent
{
    // 当前垃圾物品实例 ID
    public EntityId ItemEntityId { get; }

    // Luban 配置表中的垃圾 ID
    public int ItemId { get; }

    // 垃圾实际所属分类
    public cfg.waste.WasteCategory ItemCategoryKey { get; }

    // 玩家当前选择的投放目标
    public cfg.waste.WasteCategory TargetCategoryKey { get; }

    public WasteDropAttemptEvent(
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
