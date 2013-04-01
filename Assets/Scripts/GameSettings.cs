using UnityEngine;
using System.Collections;

public static class GameSettings 
{
    public static GameSounds gameSounds = GameSounds.enabled;
    public static Language gameLanguage = Language.English;
    public static GameObject backgroundMusic;

    public enum GameSounds
    { 
        enabled,
        disabled
    }

    public enum Language
    { 
        English,
        Tagalog,
        Chinese,
        Japanese,
        Korean,
        Espanol
    }

    public static void PlaySound(AudioSource audioSource)
    {
        if (gameSounds == GameSounds.enabled)
        {
            audioSource.Play();
        }
    }

    public static void PlaySoundWithDelay(ulong delay, AudioSource audioSource)
    {
        if (gameSounds == GameSounds.enabled)
        {
            audioSource.Play(delay);
        }
    }

    public static void PlaySoundOneShot(AudioClip audioClip, AudioSource audioSource)
    {
        if (gameSounds == GameSounds.enabled)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }

    public static void PlaySoundOneShot(AudioClip audioClip, float volumeScale, AudioSource audioSource)
    {
        if(gameSounds == GameSounds.enabled)
        {
            audioSource.PlayOneShot(audioClip, volumeScale);
        }
    }
}
