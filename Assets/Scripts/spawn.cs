using UnityEngine;
using System.Collections;

public class spawn : MonoBehaviour 
{
    public GameObject spawnPoint;

    void OnTriggerEnter(Collider other)
    { 
        if(other.gameObject.name == "Character")
        {
            other.gameObject.transform.position = spawnPoint.transform.position;
        }
    }
}
