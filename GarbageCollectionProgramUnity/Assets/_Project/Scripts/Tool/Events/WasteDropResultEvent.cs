
using UnityEngine;

/// <summary>
/// 垃圾投放后的最终结果事件。
/// 由 WastePreprocessManager 或 WasteGameManager 发布。
/// 由 WasteItemDragHandler 接收。
/// </summary>
public readonly struct WasteDropResultEvent
{
    // 当前场景中垃圾物品的实例 ID
    public EntityId ItemEntityId { get; }

    // Luban 配置表中的垃圾 ID
    public int ItemId { get; }

    // 垃圾实际所属分类
    public cfg.waste.WasteCategory ItemCategoryKey { get; }

    // 玩家选择的投放目标
    public cfg.waste.WasteCategory TargetCategoryKey { get; }

    // 是否分类正确
    public bool IsCorrect { get; }

    // 是否因为没有完成预处理而被拒绝
    public bool RequiresPreprocessing { get; }

    public WasteDropResultEvent(
        EntityId itemEntityId,
        int itemId,
        cfg.waste.WasteCategory itemCategoryKey,
        cfg.waste.WasteCategory targetCategoryKey,
        bool isCorrect,
        bool requiresPreprocessing)
    {
        ItemEntityId = itemEntityId;
        ItemId = itemId;
        ItemCategoryKey = itemCategoryKey;
        TargetCategoryKey = targetCategoryKey;
        IsCorrect = isCorrect;
        RequiresPreprocessing = requiresPreprocessing;
    }
}
