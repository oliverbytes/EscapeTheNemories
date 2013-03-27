using UnityEngine;
using System.Collections;

public class TimothyAnimation : MonoBehaviour
{
	
	// Adjusts the speed at which the walk animation is played back
	public float walkAnimationSpeedModifier = 2.5f;
	// Adjusts the speed at which the run animation is played back
	public float runAnimationSpeedModifier = 1.5f;
	// Adjusts the speed at which the jump animation is played back
	public float jumpAnimationSpeedModifier = 2.0f;
	// Adjusts the speed at which the hang time animation is played back
	public float jumpLandAnimationSpeedModifier = 3.0f;
	
	// Adjusts after how long the falling animation will be 
	public float hangTimeUntilFallingAnimation = 0.05f;
	
	public AnimationState run;
		
	// Use this for initialization
	void Start ()
	{
		animation.Stop();

		// By default loop all animations
		animation.wrapMode = WrapMode.Loop;
	
		// Jump animation are in a higher layer:
		// Thus when a jump animation is playing it will automatically override all other animations until it is faded out.
		// This simplifies the animation script because we can just keep playing the walk / run / idle cycle without having to spcial case jumping animations.
		int jumpingLayer = 1;
		AnimationState jump = animation["jump"];
		jump.layer = jumpingLayer;
		jump.speed *= jumpAnimationSpeedModifier;
		jump.wrapMode = WrapMode.Once;
		
		AnimationState jumpFall = animation["jumpFall"];
		jumpFall.layer = jumpingLayer;
		jumpFall.wrapMode = WrapMode.ClampForever;
	
		AnimationState jumpLand = animation["jumpLand"];
		jumpLand.layer = jumpingLayer;
		jumpLand.speed *= jumpLandAnimationSpeedModifier;
		jumpLand.wrapMode = WrapMode.Once;
		
		//AnimationState buttStomp = animation["buttStomp"];
	
		run = animation["run"];
		run.speed *= runAnimationSpeedModifier;
		
		AnimationState walk = animation["walk"];
		walk.speed *= walkAnimationSpeedModifier;
	}
	
	void Update () 
	{
		animation.CrossFade ("run"); // always run
	}
	
	void Slide () 
	{
		//animation.Play ("buttStomp");
	}
	
	void DidJump () 
	{
		animation.Play ("jump");
		animation.PlayQueued ("jumpFall");
	}
	
	void DidLand () 
	{
		animation.Stop ("jumpFall");
		animation.Play ("jumpLand");
		animation.Blend ("jumpLand", 0);
	}
}

