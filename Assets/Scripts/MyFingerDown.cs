using UnityEngine;
using System.Collections;

public class MyFingerDown : MonoBehaviour 
{
	private GameObject timothy;
	
	void Awake()
	{
		timothy = GameObject.FindGameObjectWithTag("Timothy");
	}
	
	void OnFingerDown( FingerDownEvent e ) 
	{
	    timothy.SendMessage ("TimothyBrake", SendMessageOptions.DontRequireReceiver);
	}
}
