public readonly struct WasteItemsSpawnedEvent
{
    public int TotalItems { get; }

    public WasteItemsSpawnedEvent(int totalItems)
    {
        TotalItems = totalItems;
    }
}