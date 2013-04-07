using UnityEngine;
using System.Collections;

public class TimothyAnimation : MonoBehaviour
{
	public float walkAnimationSpeedModifier = 2.5f;
	public float runAnimationSpeedModifier = 1f;
	public float jumpAnimationSpeedModifier = 2.0f;
	public float jumpLandAnimationSpeedModifier = 3.0f;
	public float hangTimeUntilFallingAnimation = 0.05f;
	public AnimationState run;
		
	void Start ()
	{
		animation.Stop();
		animation.wrapMode = WrapMode.Loop;
	
		// Jump animation are in a higher layer:
		// Thus when a jump animation is playing it will automatically override all other animations until it is faded out.
		// This simplifies the animation script because we can just keep playing the walk / run / idle cycle without having to spcial case jumping animations.
		int jumpingLayer = 1;
        AnimationState jump = animation["gun_jump"];
		jump.layer = jumpingLayer;
		jump.speed *= jumpAnimationSpeedModifier;
        jump.wrapMode = WrapMode.ClampForever;

        AnimationState jumpFall = animation["falling_down"]; // jumpFall
        jumpFall.layer = jumpingLayer;
        jumpFall.wrapMode = WrapMode.ClampForever;

        //AnimationState jumpLand = animation["jumpLand"];
        //jumpLand.layer = jumpingLayer;
        //jumpLand.speed *= jumpLandAnimationSpeedModifier;
        //jumpLand.wrapMode = WrapMode.Once;

        AnimationState jumpLand = animation["jump_landing"];
        jumpLand.layer = jumpingLayer;
        jumpLand.speed *= jumpLandAnimationSpeedModifier;
        jumpLand.wrapMode = WrapMode.Once;

        //AnimationState buttStomp = animation["buttStomp"];
        //buttStomp.wrapMode = WrapMode.ClampForever;

        run = animation["gun_run"];
		run.speed *= runAnimationSpeedModifier;

        animation["getting_hit"].wrapMode = WrapMode.Once;

        this.TimothyAnimationRun();
	}
	
    //void Update () 
    //{
    //    animation.CrossFade ("run"); // always run
    //}

    void TimothyAnimationStumble()
    {
        animation.Play("getting_hit");
        animation.CrossFade("run");
    }

    void TimothyAnimationRun()
    {
        //animation.Stop("buttStomp");
        animation.Play("gun_run");
    }

    void TimothyAnimationDash()
    {
        //animation.Play("gun_run");
    }

    void TimothyAnimationSmash()
    {
        //animation.CrossFade("buttStomp");
        //animation.PlayQueued("buttStomp");
    }

    void TimothyAnimationSlide() 
	{
        animation.Stop("gun_run");
        //animation.Play("buttStomp");
	}

    void TimothyAnimationJump() 
	{
        animation.Stop("gun_run");
        animation.Play("gun_jump");
        animation.PlayQueued("falling_down"); // jumpFall
	}

    void TimothyAnimationLanded() 
	{
        animation.Stop("gun_jump");
        animation.Stop("falling_down"); // jumpFall
        animation.Play("gun_run");
	}
}

