using UnityEngine;
using System.Collections;

public class ButtonClickedEvent : MonoBehaviour 
{
    public void OnClicked_btnPlay()
    {
        Application.LoadLevel("Level1");
    }

    public void OnClicked_btnAbout()
    {
        Application.LoadLevel("MenuAbout");
    }

    public void OnClicked_btnOptions()
    {
        Application.LoadLevel("MenuOptions");
    }

    public void OnClicked_btnHelp()
    {
        Application.LoadLevel("MenuHelp");
    }

    public void OnClicked_btnLanguage()
    {
        //Application.LoadLevel("MenuLanguageSelection");
    }

    public void OnClicked_btnBackOptions()
    {
        Application.LoadLevel("MenuMain");
    }

    public void OnClicked_btnBackHelp()
    {
        Application.LoadLevel("MenuOptions");
    }

    public void OnClicked_btnBackLanguageSelection()
    {
        Application.LoadLevel("MenuOptions");
    }

    public void OnClicked_btnSounds()
    {
        int activeID = 14;
        int normalID = 13;

        tk2dSprite tk2dSpriteComponent = GameObject.Find("btnSound").GetComponent<tk2dSprite>();

        if (tk2dSpriteComponent.spriteId == normalID)
        {
            tk2dSpriteComponent.spriteId = activeID;
            GameSettings.gameSounds = GameSettings.GameSounds.enabled;
        }
        else
        {
            tk2dSpriteComponent.spriteId = normalID;
            GameSettings.gameSounds = GameSettings.GameSounds.disabled;
        }
    }
}