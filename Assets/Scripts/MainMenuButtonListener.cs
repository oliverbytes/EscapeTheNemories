using UnityEngine;
using System.Collections;

public class MainMenuButtonListener : MonoBehaviour 
{
    public GameObject main;
    public GameObject options;
    public GameObject language;
    public GameObject help;
    public GameObject about;
    public GameObject btnBackFromOptions;
    public GameObject btnBackFromHelp;
    public GameObject btnBackFromLanguage;
    public GameObject btnBackFromAbout;
    public GameObject btnSounds;
    public GameObject title;
    public GameObject loading;

    private tk2dSprite tk2dSpriteComponent;
    private AudioSource[] audioSources;

    void Awake()
    {
        audioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        tk2dSpriteComponent = btnSounds.GetComponent<tk2dSprite>();
    }

    public void OnClicked_btnPlay()
    {
        Application.LoadLevelAsync("Level1");
        title.SetActive(false);
        main.SetActive(false);
        loading.SetActive(true);
    }

    public void OnClicked_btnOptions()
    {
        main.SetActive(false);
        options.SetActive(true);
    }

    public void OnClicked_btnAbout()
    {
        main.SetActive(false);
        title.SetActive(false);
        about.SetActive(true);
    }

    public void OnClicked_btnLanguage()
    {
        options.SetActive(false);
        title.SetActive(false);
        language.SetActive(true);
    }

    public void OnClicked_btnHelp()
    {
        options.SetActive(false);
        title.SetActive(false);
        help.SetActive(true);
    }

    public void OnClicked_btnBackFromOptions()
    {
        options.SetActive(false);
        main.SetActive(true);
    }

    public void OnClicked_btnBackFromLanguage()
    {
        language.SetActive(false);
        title.SetActive(true);
        options.SetActive(true);
    }

    public void OnClicked_btnBackFromHelp()
    {
        help.SetActive(false);
        title.SetActive(true);
        options.SetActive(true);
    }

    public void OnClicked_btnBackFromAbout()
    {
        about.SetActive(false);
        title.SetActive(true);
        main.SetActive(true);
    }

    public void OnClicked_btnSounds()
    {
        int activeID = 14;
        int normalID = 13;

        if (tk2dSpriteComponent.spriteId == activeID) // sounds are on
        {
            MuteAudios();
            tk2dSpriteComponent.spriteId = normalID;
            GameSettings.gameSounds = GameSettings.GameSounds.enabled;
        }
        else  // sounds are off
        {
            UnMuteAudios();
            tk2dSpriteComponent.spriteId = activeID;
            GameSettings.gameSounds = GameSettings.GameSounds.disabled;
        }
    }
    
    private void MuteAudios()
    {
        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.mute = true;
        }
    }

    private void UnMuteAudios()
    {
        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.mute = false;
        }
    } 
}
