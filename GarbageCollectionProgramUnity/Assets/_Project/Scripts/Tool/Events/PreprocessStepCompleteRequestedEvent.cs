using UnityEngine;

/// <summary>
/// 预处理步骤完成请求事件
/// 玩家点击预处理面板的完成按钮时发布
/// </summary>
public readonly struct PreprocessStepCompleteRequestedEvent
{
    /// <summary>
    /// 当前正在进行预处理的垃圾物品实体 ID
    /// </summary>
    public EntityId ItemEntityId { get; }

    public PreprocessStepCompleteRequestedEvent(EntityId itemEntityId)
    {
        ItemEntityId = itemEntityId;
    }
}