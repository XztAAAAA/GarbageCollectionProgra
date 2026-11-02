
/// <summary>
/// 游戏需要显示提示信息时发布。
/// </summary>
public readonly struct WasteFeedbackEvent
{
    public string Message { get; }

    public WasteFeedbackEvent(string message)
    {
        Message = message;
    }
}
