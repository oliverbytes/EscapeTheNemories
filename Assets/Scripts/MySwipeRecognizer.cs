using UnityEngine;
using System.Collections;

public class MySwipeRecognizer : MonoBehaviour 
{
	private GameObject timothy;
	
	void Awake()
	{
		timothy = GameObject.FindGameObjectWithTag("Timothy");
	}
	
	void OnSwipe( SwipeGesture gesture ) 
	{
	    // Total swipe vector (from start to end position)
	    Vector2 move = gesture.Move;
	 
	    // Instant gesture velocity in screen units per second
	    float velocity = gesture.Velocity;
	 
	    // Approximate swipe direction
	    FingerGestures.SwipeDirection direction = gesture.Direction;
		
		//Debug.Log("move: " + move + ", velocity: " + velocity + ", direction: " + direction);
		
		if(direction == FingerGestures.SwipeDirection.Right)
		{
			timothy.SendMessage ("TimothyDash", SendMessageOptions.DontRequireReceiver);
		}
		else if(direction == FingerGestures.SwipeDirection.Left)
		{
			// punching, kicking enemies
		}
		else if(direction == FingerGestures.SwipeDirection.Up)
		{
			timothy.SendMessage ("TimothyJump", SendMessageOptions.DontRequireReceiver);
		}
		else if(direction == FingerGestures.SwipeDirection.Down)
		{
			// slide
		}
	}
}
