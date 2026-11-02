using Luban.SimpleJSON;
using UnityEngine;

public static class TableSystem
{
    private static cfg.Tables tables;
    private static cfg.TbLanguage languageTable;

    public static cfg.Tables Tables => tables ?? (tables = LoadTables());

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

        text = Application.systemLanguage == SystemLanguage.ChineseSimplified
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
