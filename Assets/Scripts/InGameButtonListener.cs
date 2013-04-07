using UnityEngine;
using System.Collections;

public class InGameButtonListener : MonoBehaviour 
{
    public GameObject btnPause;
    public GameObject btnResume;
    public GameObject btnBackToMenu;
    private AudioSource[] audioSources;
    public GameObject loading;

    void Awake()
    {
        audioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
    }

    public void OnClicked_btnPause()
    {
        this.MuteAudios();
        Time.timeScale = 0.0f;
        btnPause.SetActive(false);
        btnResume.SetActive(true);
        btnBackToMenu.SetActive(true);
    }

    public void OnClicked_btnResume()
    {
        this.UnMuteAudios();
        Time.timeScale = 1.0f;
        btnPause.SetActive(true);
        btnResume.SetActive(false);
        btnBackToMenu.SetActive(false);
    }

    public void OnClicked_btnBackToMenu()
    {
        Application.LoadLevelAsync("MainMenu");
        loading.SetActive(true);
        btnResume.SetActive(false);
        btnBackToMenu.SetActive(false);
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