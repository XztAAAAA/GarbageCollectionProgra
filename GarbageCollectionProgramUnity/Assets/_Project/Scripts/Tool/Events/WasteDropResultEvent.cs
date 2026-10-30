
using UnityEngine;

/// <summary>
/// 垃圾物品被放入分类区域后的判定事件。
/// </summary>
public readonly struct WasteDropResultEvent
{
    // 当前场景中垃圾物品实例的 ID
    public EntityId ItemEntityId { get; }

    // Luban 配置表中的垃圾 ID
    public int ItemId { get; }

    // 垃圾原本所属分类
    public cfg.waste.WasteCategory ItemCategoryKey { get; }

    // 当前放入的目标分类
    public cfg.waste.WasteCategory TargetCategoryKey { get; }

    // 是否分类正确
    public bool IsCorrect { get; }

    public WasteDropResultEvent(
        EntityId itemInstanceId,
        int itemId,
        cfg.waste.WasteCategory itemCategoryKey,
        cfg.waste.WasteCategory targetCategoryKey,
        bool isCorrect)
    {
        ItemEntityId = itemInstanceId;
        ItemId = itemId;
        ItemCategoryKey = itemCategoryKey;
        TargetCategoryKey = targetCategoryKey;
        IsCorrect = isCorrect;
    }
}
