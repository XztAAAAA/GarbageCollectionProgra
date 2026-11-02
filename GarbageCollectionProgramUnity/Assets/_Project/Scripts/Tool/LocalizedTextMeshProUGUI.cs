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
        RefreshLocalizedText();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        RefreshLocalizedText();
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

        cfg.Language language =
            TableSystem.Tables.TbLanguage.GetOrDefault(languageKey);
        if (language == null)
        {
            error = $"找不到多语言 key：{languageKey}";
            return false;
        }

        localizedText = TableSystem.GetLanguage(languageKey);
        return true;
    }

    private void RefreshLocalizedText()
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
