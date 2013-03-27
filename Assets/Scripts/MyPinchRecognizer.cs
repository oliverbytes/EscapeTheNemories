using UnityEngine;
using System.Collections;

public class MyPinchRecognizer : MonoBehaviour {

	private GameObject timothy;
	
	void Awake()
	{
		timothy = GameObject.FindGameObjectWithTag("Timothy");
	}
	
	void OnPinch(PinchGesture gesture) 
	{
		Debug.Log("PINCHED");
		
		//timothy.SendMessage ("TimothyBrake", SendMessageOptions.DontRequireReceiver);
		
		// current gesture phase (Started/Updated/Ended)
		// ContinuousGesturePhase phase = gesture.Phase;
		
		// Current gap distance between the two fingers
		// float gap = gesture.Gap;
		
		// Gap difference since last frame
		// float delta = gesture.Delta;
	}
}
