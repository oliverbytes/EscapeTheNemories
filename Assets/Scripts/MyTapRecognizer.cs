using UnityEngine;
using System.Collections;

public class MyTapRecognizer : MonoBehaviour 
{
    private GameObject timothy;

    void Awake()
    {
        timothy = GameObject.FindGameObjectWithTag("Timothy");
    }

	void OnTap(TapGesture gesture) 
	{
        if (gesture.Selection) // tapped an object
        {
            if (gesture.Selection.tag != "Button")
            {
                Vector2 offset = new Vector2(380, 195);
                Vector2 newGesturePosition = gesture.Position - offset;
                timothy.SendMessage("TimothyShoot", newGesturePosition, SendMessageOptions.DontRequireReceiver);
            }
        }
        else
        {
            // timothy.SendMessage("TimothyShoot", SendMessageOptions.DontRequireReceiver);
        }
	}
}
