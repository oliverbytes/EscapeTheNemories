using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]
[RequireComponent (typeof (SphereCollider))]
[RequireComponent (typeof (Rigidbody))]

public class Foot : MonoBehaviour 
{
	public float baseFootAudioVolume = 1.0f;
	public float soundEffectPitchRandomness = 0.05f;
	private TimothyController timothyController;
	
	void Start()
	{
		timothyController = GameObject.FindGameObjectWithTag("Timothy").GetComponent<TimothyController>();
	}

	void OnTriggerEnter(Collider other) 
	{
		if(other.gameObject.tag == "EnemyStone")
		{
            //other.gameObject.tag = "EnemyStoneDone";
            timothyController.SendMessage("TimothyStepOnStone", SendMessageOptions.DontRequireReceiver);
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
