using UnityEngine;
using System.Collections;

public class MyFingerUp : MonoBehaviour 
{
	private GameObject timothy;
	
	void Awake()
	{
		timothy = GameObject.FindGameObjectWithTag("Timothy");
	}
		
	void OnFingerUp( FingerUpEvent e ) 
	{
		timothy.SendMessage ("TimothyStopBrake", SendMessageOptions.DontRequireReceiver);
	}
}
