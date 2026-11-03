using TMPro;
using UnityEngine;

[AddComponentMenu("UI/Localized TextMeshPro")]
[ExecuteAlways]
public class LocalizedTextMeshProUGUI : TextMeshProUGUI
{
    [SerializeField]
    private string languageKey;

    public string LanguageKey => languageKey;

    protected override void OnEnable()
    {
        base.OnEnable();
        TableSystem.LanguageChanged += RefreshLocalizedText;
        RefreshLocalizedText();
    }

    protected override void OnDisable()
    {
        TableSystem.LanguageChanged -= RefreshLocalizedText;
        base.OnDisable();
    }

    public bool TryGetLocalizedText(
        out string localizedText,
        out string error)
    {
        localizedText = string.Empty;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(languageKey))
        {
            error = "请输入多语言 key。";
            return false;
        }

        if (!TableSystem.TryGetLanguage(
            languageKey,
            out localizedText))
        {
            error = $"找不到多语言 key：{languageKey}";
            return false;
        }

        return true;
    }

    public void RefreshLocalizedText()
    {
        if (string.IsNullOrWhiteSpace(languageKey))
        {
            return;
        }

        if (TryGetLocalizedText(out string localizedText, out string error))
        {
            text = localizedText;
        }
        else
        {
            CommonTool.LogError(
                $"[LocalizedTextMeshProUGUI] {error}",
                this
            );
        }
    }
}
