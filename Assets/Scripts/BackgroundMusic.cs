using UnityEngine;
using System.Collections;

public class BackgroundMusic : MonoBehaviour 
{
	void Awake () 
    {
        GameSettings.backgroundMusic = gameObject;
        DontDestroyOnLoad(GameSettings.backgroundMusic);
	}
}
