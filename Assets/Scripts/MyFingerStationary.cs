using UnityEngine;
using System.Collections;

public class MyFingerStationary : MonoBehaviour 
{
	private GameObject timothy;
	
	void Awake()
	{
		timothy = GameObject.FindGameObjectWithTag("Timothy");
	}
	
	void OnFingerMove( FingerMotionEvent e ) 
	{
	    if( e.Phase == FingerMotionPhase.Started )
		{
			timothy.SendMessage ("TimothyStartBraking", SendMessageOptions.DontRequireReceiver);
		}
		else if( e.Phase == FingerMotionPhase.Updated )
		{
			timothy.SendMessage ("TimothyStartBraking", SendMessageOptions.DontRequireReceiver);
		}
	    else if( e.Phase == FingerMotionPhase.Ended )
		{
			timothy.SendMessage ("TimothyStopBraking", SendMessageOptions.DontRequireReceiver);
		}
	}

	void OnFingerStationary( FingerMotionEvent e ) 
	{
	     if( e.Phase == FingerMotionPhase.Started )
		{
			timothy.SendMessage ("TimothyStartBraking", SendMessageOptions.DontRequireReceiver);
		}
		else if( e.Phase == FingerMotionPhase.Updated )
		{
			timothy.SendMessage ("TimothyStartBraking", SendMessageOptions.DontRequireReceiver);
		}
	    else if( e.Phase == FingerMotionPhase.Ended )
		{
			timothy.SendMessage ("TimothyStopBraking", SendMessageOptions.DontRequireReceiver);
		}
	}
}
