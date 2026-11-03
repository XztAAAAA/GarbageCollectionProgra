
/// <summary>
/// 游戏需要显示提示信息时发布。
/// </summary>
public readonly struct WasteFeedbackEvent
{
    public string Message { get; }
    public string LanguageKey { get; }

    public WasteFeedbackEvent(string message)
    {
        Message = message;
        LanguageKey = null;
    }

    public WasteFeedbackEvent(string message, string languageKey)
    {
        Message = message;
        LanguageKey = languageKey;
    }
}
