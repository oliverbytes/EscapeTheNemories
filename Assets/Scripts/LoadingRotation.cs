using UnityEngine;
using System.Collections;

public class LoadingRotation : MonoBehaviour 
{
    public bool rotationEnabled = true;
    public float rotationSpeed = 360;

	void Update () 
    {
        if (rotationEnabled)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
	}
}
