/// <summary>
/// 当前关卡完成事件。
/// </summary>
public readonly struct WasteLevelCompletedEvent
{
    public int TotalItems { get; }
    public int CompletedItems { get; }

    public WasteLevelCompletedEvent(
        int totalItems,
        int completedItems)
    {
        TotalItems = totalItems;
        CompletedItems = completedItems;
    }
}