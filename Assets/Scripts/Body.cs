using UnityEngine;
using System.Collections;

public class Body : MonoBehaviour
{
    public AudioClip coinSound;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Coin")
        {
            audio.PlayOneShot(coinSound, 1f);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.tag == "EnemyStumbler")
        {
            Destroy(other.gameObject);
            SendMessage("TimothyStumble", SendMessageOptions.DontRequireReceiver);
        }
    }
}