using Luban.SimpleJSON;
using UnityEngine;

public static class TableSystem
{
    private static cfg.Tables tables;

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

    public static string GetLanguage(string key)
    {
        var language = Tables.TbLanguage.Get(key);
         return Application.systemLanguage == SystemLanguage.ChineseSimplified
            ? language.ChineseText
            : language.KoreanText;
    }
}
