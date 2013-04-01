using UnityEngine;
using System.Collections;

public class OnStart : MonoBehaviour 
{
	void Start () 
    {
        AudioSource backgroundMusic = GameObject.Find("BackgroundMusic").GetComponent<AudioSource>();
        GameSettings.PlaySound(backgroundMusic);
	}
}
