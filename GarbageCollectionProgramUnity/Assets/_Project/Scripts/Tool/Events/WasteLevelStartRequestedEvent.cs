/// <summary>
/// 请求开始指定关卡。
/// </summary>
public readonly struct WasteLevelStartRequestedEvent
{
    public int LevelId { get; }

    public WasteLevelStartRequestedEvent(int levelId)
    {
        LevelId = levelId;
    }
}