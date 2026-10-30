
/// <summary>
/// 游戏需要显示提示信息时发布。
/// </summary>
public readonly struct WasteFeedbackEvent
{
    public string KoreanMessage { get; }
    public string ChineseMessage { get; }

    public WasteFeedbackEvent(
        string koreanMessage,
        string chineseMessage)
    {
        KoreanMessage = koreanMessage;
        ChineseMessage = chineseMessage;
    }
}
