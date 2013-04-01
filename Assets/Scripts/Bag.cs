using UnityEngine;
using System.Collections;

public class Bag : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "BagPoints")
        {
            Destroy(other.gameObject);
            Debug.Log("Points + 1");
        }
    }
}
