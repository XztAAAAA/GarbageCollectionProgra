using UnityEngine;

/// <summary>
/// 垃圾物品生成事件
/// 在每个垃圾物品生成并初始化后发布
/// </summary>
public readonly struct WasteItemSpawnedEvent
{
    /// <summary>
    /// 垃圾物品的实体 ID
    /// </summary>
    public EntityId ItemEntityId { get; }

    /// <summary>
    /// 垃圾物品的 Luban 配置表 ID
    /// </summary>
    public int ItemId { get; }

    public WasteItemSpawnedEvent(EntityId itemEntityId, int itemId)
    {
        ItemEntityId = itemEntityId;
        ItemId = itemId;
    }
}