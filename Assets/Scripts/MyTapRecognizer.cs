using UnityEngine;
using System.Collections;

public class MyTapRecognizer : MonoBehaviour {

	void OnTap(TapGesture gesture) 
	{
		if(gesture.Selection)
		{
			if(gesture.Selection.name == "PlayButton")
			{
				Application.LoadLevel("Level1");
			}
		}
	}
}
