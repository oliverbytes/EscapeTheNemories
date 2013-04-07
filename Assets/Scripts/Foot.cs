using UnityEngine;
using System.Collections;

public class Foot : MonoBehaviour 
{
	private TimothyController timothyController;
	
	void Start()
	{
		timothyController = gameObject.GetComponent<TimothyController>();
	}

	void OnTriggerEnter(Collider other) 
	{
		if(other.gameObject.tag == "EnemyStumble")
		{
            timothyController.SendMessage("TimothyStumble", SendMessageOptions.DontRequireReceiver);
		}
		
        //CollisionParticleEffect collisionParticleEffect = other.GetComponent<CollisionParticleEffect>();

        //if (collisionParticleEffect) 
        //{
        //    Instantiate(collisionParticleEffect.effect, transform.position, transform.rotation);
        //}
		
        //CollisionSoundEffect collisionSoundEffect = other.GetComponent<CollisionSoundEffect>();
	
        //if (collisionSoundEffect) 
        //{
        //    audio.clip = collisionSoundEffect.audioClip;
        //    audio.volume = collisionSoundEffect.volumeModifier * baseFootAudioVolume;
        //    audio.pitch = (float) Random.Range(1.0f - soundEffectPitchRandomness, 1.0f + soundEffectPitchRandomness);
        //    audio.Play();
        //}
	}
	
	void Reset() {
		rigidbody.isKinematic = true;
		collider.isTrigger = true;
	}
}
