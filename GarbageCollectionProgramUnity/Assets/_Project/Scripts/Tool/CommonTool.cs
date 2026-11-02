using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CommonTool
{
    public static void Log(object message, UnityEngine.Object context = null)
    {
#if UNITY_EDITOR
        Debug.Log(message, context);
#endif
    }

    public static void LogWarning(
        object message,
        UnityEngine.Object context = null)
    {
#if UNITY_EDITOR
        Debug.LogWarning(message, context);
#endif
    }

    public static void LogError(
        object message,
        UnityEngine.Object context = null)
    {
#if UNITY_EDITOR
        Debug.LogError(message, context);
#endif
    }

    public static void LogException(
        Exception exception,
        UnityEngine.Object context = null)
    {
        Debug.LogException(exception, context);
    }

    public static void SetTimeScale(float value)
    {
        Time.timeScale = value;
    }

    public static Sprite LoadItemSprite(string iconKey)
    {
        if (string.IsNullOrWhiteSpace(iconKey))
        {
            LogError("Cannot load an item sprite with an empty icon key.");
            return null;
        }

        return Resources.Load<Sprite>($"UI/Item/{iconKey}");
    }

    public static void LoadScene(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            LogError("Cannot load a scene with an empty path.");
            return;
        }

        string scenePath = path.Replace('\\', '/');
        if (scenePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            scenePath = scenePath.Substring("Assets/".Length);
        }

        if (scenePath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
        {
            scenePath = scenePath.Substring(0, scenePath.Length - ".unity".Length);
        }

        SceneManager.LoadSceneAsync(scenePath);
    }
}
