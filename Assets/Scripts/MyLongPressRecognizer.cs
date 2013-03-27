using UnityEngine;
using System.Collections;

public class MyLongPressRecognizer : MonoBehaviour 
{
	private GameObject timothy;
	
	void Awake()
	{
		timothy = GameObject.FindGameObjectWithTag("Timothy");
	}
	
	void OnLongPress(LongPressGesture gesture) 
	{
		//timothy.SendMessage ("TimothyBrake", SendMessageOptions.DontRequireReceiver);
	}
}
