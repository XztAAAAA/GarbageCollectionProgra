
/// <summary>
/// 游戏进度发生变化时发布。
/// </summary>
public readonly struct WasteProgressChangedEvent
{
    public int CurrentStage { get; }
    public int TotalItems { get; }
    public int CompletedItems { get; }
    public int RemainingItems { get; }
    public float Progress { get; }

    public WasteProgressChangedEvent(
        int currentStage,
        int totalItems,
        int completedItems)
    {
        CurrentStage = currentStage;
        TotalItems = totalItems;
        CompletedItems = completedItems;
        RemainingItems = totalItems - completedItems;

        Progress = totalItems > 0
            ? (float)completedItems / totalItems
            : 0f;
    }
}
