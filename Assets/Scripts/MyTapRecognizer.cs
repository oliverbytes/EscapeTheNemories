using UnityEngine;
using System.Collections;

public class TapTutorial : MonoBehaviour 
{
	
	private GameObject timothy;
	
	void Awake()
	{
		timothy = GameObject.FindGameObjectWithTag("Timothy");
	}

	void OnTap(TapGesture gesture) 
	{
//		if(gesture.Selection)
//		{
//			Debug.Log("Tapped the object: " + gesture.Selection.name);
//		}
//		else
//		{
//			Debug.Log("nothing was tapped");
//		}
		
		timothy.SendMessage ("TimothyBrake", SendMessageOptions.DontRequireReceiver);
	}
}
