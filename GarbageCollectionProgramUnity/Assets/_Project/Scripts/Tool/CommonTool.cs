using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CommonTool
{
    public static void SetTimeScale(float value)
    {
        Time.timeScale = value;
    }

    public static void LoadScene(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            Debug.LogError("Cannot load a scene with an empty path.");
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
