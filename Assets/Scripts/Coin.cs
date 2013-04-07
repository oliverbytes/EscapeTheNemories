using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour
{
    public AudioClip coinSound;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Coin")
        {
            audio.PlayOneShot(coinSound, 1f);
            Destroy(other.gameObject);
        }
    }
}