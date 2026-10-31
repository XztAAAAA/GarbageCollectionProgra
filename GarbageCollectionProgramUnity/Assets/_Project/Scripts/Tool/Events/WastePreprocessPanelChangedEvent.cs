using UnityEngine;

/// <summary>
/// 预处理面板状态变化事件
/// 用于通知 UI 显示、隐藏或更新预处理面板
/// </summary>
public readonly struct WastePreprocessPanelChangedEvent
{
    /// <summary>
    /// 是否显示预处理面板
    /// </summary>
    public bool IsVisible { get; }

    /// <summary>
    /// 当前正在预处理的垃圾实体 ID
    /// </summary>
    public EntityId ItemEntityId { get; }

    /// <summary>
    /// 预处理步骤说明文本
    /// </summary>
    public string ContentText { get; }

    /// <summary>
    /// 完成按钮显示文本
    /// </summary>
    public string ButtonText { get; }

    public WastePreprocessPanelChangedEvent(
        bool isVisible,
        EntityId itemEntityId,
        string contentText,
        string buttonText)
    {
        IsVisible = isVisible;
        ItemEntityId = itemEntityId;
        ContentText = contentText;
        ButtonText = buttonText;
    }
}