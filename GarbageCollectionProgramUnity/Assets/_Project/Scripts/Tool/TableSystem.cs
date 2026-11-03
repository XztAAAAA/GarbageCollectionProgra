using Luban.SimpleJSON;
using UnityEngine;

public enum GameLanguage
{
    Chinese = 0,
    Korean = 1
}

public static class TableSystem
{
    private const string LanguagePreferenceKey = "GameLanguage";

    private static cfg.Tables tables;
    private static cfg.TbLanguage languageTable;
    private static bool languageInitialized;
    private static GameLanguage currentLanguage;

    public static event System.Action LanguageChanged;

    public static cfg.Tables Tables => tables ?? (tables = LoadTables());

    public static GameLanguage CurrentLanguage
    {
        get
        {
            if (!languageInitialized)
            {
                int savedLanguage = PlayerPrefs.GetInt(
                    LanguagePreferenceKey,
                    Application.systemLanguage == SystemLanguage.ChineseSimplified
                        ? (int)GameLanguage.Chinese
                        : (int)GameLanguage.Korean
                );

                currentLanguage = savedLanguage == (int)GameLanguage.Chinese
                    ? GameLanguage.Chinese
                    : GameLanguage.Korean;
                languageInitialized = true;
            }

            return currentLanguage;
        }
    }

    public static void SetLanguage(GameLanguage language)
    {
        if (language != GameLanguage.Chinese &&
            language != GameLanguage.Korean)
        {
            throw new System.ArgumentOutOfRangeException(
                nameof(language),
                language,
                "Unsupported game language."
            );
        }

        bool languageChanged = CurrentLanguage != language;
        currentLanguage = language;
        languageInitialized = true;
        PlayerPrefs.SetInt(LanguagePreferenceKey, (int)language);
        PlayerPrefs.Save();

        if (languageChanged)
        {
            LanguageChanged?.Invoke();
        }
    }

    private static cfg.Tables LoadTables()
    {
        return new cfg.Tables(name =>
        {
            var asset = Resources.Load<TextAsset>($"Luban/{name}");

            if (asset == null)
                throw new System.Exception($"Luban data not found: {name}");

            return JSON.Parse(asset.text);
        });
    }

    public static string GetLanguage(string key, params object[] args)
    {
        if (!TryGetLanguage(key, out string text, args))
        {
            throw new System.Collections.Generic.KeyNotFoundException(
                $"Luban language key not found: {key}"
            );
        }

        return text;
    }

    public static bool TryGetLanguage(
        string key,
        out string text,
        params object[] args)
    {
        text = string.Empty;
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        cfg.Language language = LanguageTable.GetOrDefault(key);
        if (language == null)
        {
            return false;
        }

        text = CurrentLanguage == GameLanguage.Chinese
            ? language.ChineseText
            : language.KoreanText;
        if (args != null && args.Length > 0)
        {
            text = string.Format(text, args);
        }

        return true;
    }

    private static cfg.TbLanguage LanguageTable =>
        languageTable ?? (languageTable = LoadLanguageTable());

    private static cfg.TbLanguage LoadLanguageTable()
    {
        TextAsset asset = Resources.Load<TextAsset>("Luban/tblanguage");
        if (asset == null)
        {
            throw new System.Exception(
                "Luban language data not found: tblanguage"
            );
        }

        return new cfg.TbLanguage(JSON.Parse(asset.text));
    }
}
