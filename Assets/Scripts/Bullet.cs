using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour 
{
    void OnTriggerEnter(Collider collider)
    { 
        if(collider.gameObject.tag == "Enemy")
        {
            Debug.Log("enemy destroyed");
            Destroy(collider.gameObject);
        }
    }
}
