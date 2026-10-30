
using UnityEngine;

/// <summary>
/// 玩家选中垃圾物品时发布的事件。
/// </summary>
public readonly struct WasteItemSelectedEvent
{
    // 当前被选中的垃圾实例 ID
    public EntityId ItemEntityId { get; }

    // Luban 垃圾配置表 ID
    public int ItemId { get; }

    public WasteItemSelectedEvent(
        EntityId itemEntityId,
        int itemId)
    {
        ItemEntityId = itemEntityId;
        ItemId = itemId;
    }
}
