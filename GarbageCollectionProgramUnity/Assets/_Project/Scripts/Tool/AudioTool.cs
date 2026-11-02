using UnityEngine;

public static class AudioTool
{
    private const string DefaultClickSoundPath = "Sound/ui_btn_click";
    private const string ClickAudioSourceObjectName = "CommonTool Click Audio Source";
    private const string BackgroundMusicAudioSourceObjectName =
        "CommonTool Background Music Audio Source";

    private static AudioClip defaultClickSound;
    private static AudioSource clickAudioSource;
    private static AudioSource backgroundMusicAudioSource;

    public static void PlaySound(
        string audioClipPath,
        float volumeScale = 1f,
        AudioSource audioSource = null)
    {
        audioSource = audioSource ?? GetClickAudioSource();

        if (string.IsNullOrWhiteSpace(audioClipPath))
        {
            CommonTool.LogError("Cannot play a sound with an empty audio clip path.");
            return;
        }

        AudioClip audioClip =
            Resources.Load<AudioClip>($"Sound/{audioClipPath}");
        if (audioClip == null)
        {
            CommonTool.LogError(
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
            CommonTool.LogError(
                $"Cannot load the default click sound at " +
                $"Resources/{DefaultClickSoundPath}."
            );
            return;
        }

        PlaySound(defaultClickSound, GetClickAudioSource());
    }

    public static void PlayBackgroundMusic(
        string audioClipPath,
        float volumeScale = 0.2f)
    {
        if (string.IsNullOrWhiteSpace(audioClipPath))
        {
            CommonTool.LogError(
                "Cannot play background music with an empty audio clip path."
            );
            return;
        }

        AudioClip audioClip =
            Resources.Load<AudioClip>($"Sound/{audioClipPath}");
        if (audioClip == null)
        {
            CommonTool.LogError(
                $"Cannot load the background music at " +
                $"Resources/Sound/{audioClipPath}."
            );
            return;
        }

        AudioSource audioSource = GetBackgroundMusicAudioSource();
        audioSource.volume = Mathf.Clamp01(volumeScale);

        if (audioSource.clip == audioClip && audioSource.isPlaying)
        {
            return;
        }

        audioSource.Stop();
        audioSource.clip = audioClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private static void PlaySound(
        AudioClip audioClip,
        AudioSource audioSource,
        float volumeScale = 1f)
    {
        if (audioSource == null)
        {
            CommonTool.LogError("Cannot play a sound without an AudioSource.");
            return;
        }

        if (audioClip == null)
        {
            CommonTool.LogError("Cannot play a null AudioClip.", audioSource);
            return;
        }

        audioSource.PlayOneShot(audioClip, volumeScale);
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
            Object.DontDestroyOnLoad(audioSourceObject);
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

    private static AudioSource GetBackgroundMusicAudioSource()
    {
        if (backgroundMusicAudioSource != null)
        {
            return backgroundMusicAudioSource;
        }

        GameObject audioSourceObject =
            GameObject.Find(BackgroundMusicAudioSourceObjectName);
        if (audioSourceObject == null)
        {
            audioSourceObject =
                new GameObject(BackgroundMusicAudioSourceObjectName);
            Object.DontDestroyOnLoad(audioSourceObject);
        }

        backgroundMusicAudioSource =
            audioSourceObject.GetComponent<AudioSource>();
        if (backgroundMusicAudioSource == null)
        {
            backgroundMusicAudioSource =
                audioSourceObject.AddComponent<AudioSource>();
        }

        backgroundMusicAudioSource.playOnAwake = false;
        backgroundMusicAudioSource.loop = true;
        backgroundMusicAudioSource.spatialBlend = 0f;

        return backgroundMusicAudioSource;
    }
}
