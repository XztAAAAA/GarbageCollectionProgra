using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CommonTool
{
    private const string DefaultClickSoundPath = "Sound/ui_btn_click";
    private const string ClickAudioSourceObjectName = "CommonTool Click Audio Source";
    private static AudioClip defaultClickSound;
    private static AudioSource clickAudioSource;

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

    public static void PlaySound(
        AudioClip audioClip, 
        AudioSource audioSource,
        float volumeScale = 1f)
    {
        if (audioSource == null)
        {
            LogError("Cannot play a sound without an AudioSource.");
            return;
        }

        if (audioClip == null)
        {
            LogError("Cannot play a null AudioClip.", audioSource);
            return;
        }

        audioSource.PlayOneShot(audioClip, volumeScale);
    }

    public static void PlaySound(
        string audioClipPath,
        AudioSource audioSource = null,
        float volumeScale = 1f)
    {
        audioSource = audioSource ?? GetClickAudioSource();

        if (string.IsNullOrWhiteSpace(audioClipPath))
        {
            LogError("Cannot play a sound with an empty audio clip path.");
            return;
        }

        AudioClip audioClip = Resources.Load<AudioClip>($"Sound/{audioClipPath}");
        if (audioClip == null)
        {
            LogError(
                $"Cannot load the audio clip at " +
                $"Resources/Sound/{audioClipPath}.",
                audioSource
            );
            return;
        }

        PlaySound(audioClip, audioSource, volumeScale);
    }

    public static void PlayClickSound()
    {
        if (defaultClickSound == null)
        {
            defaultClickSound =
                Resources.Load<AudioClip>(DefaultClickSoundPath);
        }

        if (defaultClickSound == null)
        {
            LogError(
                $"Cannot load the default click sound at " +
                $"Resources/{DefaultClickSoundPath}."
            );
            return;
        }

        PlaySound(defaultClickSound, GetClickAudioSource());
    }

    private static AudioSource GetClickAudioSource()
    {
        if (clickAudioSource != null)
        {
            return clickAudioSource;
        }

        GameObject audioSourceObject =
            GameObject.Find(ClickAudioSourceObjectName);
        if (audioSourceObject == null)
        {
            audioSourceObject = new GameObject(ClickAudioSourceObjectName);
            UnityEngine.Object.DontDestroyOnLoad(audioSourceObject);
        }

        clickAudioSource = audioSourceObject.GetComponent<AudioSource>();
        if (clickAudioSource == null)
        {
            clickAudioSource = audioSourceObject.AddComponent<AudioSource>();
        }

        clickAudioSource.playOnAwake = false;
        clickAudioSource.loop = false;
        clickAudioSource.spatialBlend = 0f;

        return clickAudioSource;
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
