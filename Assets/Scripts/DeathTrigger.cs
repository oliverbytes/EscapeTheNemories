using UnityEngine;
using System.Collections;

public class DeathTrigger : MonoBehaviour
{
	void OnTriggerEnter(Collider other) 
	{
		other.gameObject.SendMessage ("GameOver", SendMessageOptions.DontRequireReceiver);
	}
}