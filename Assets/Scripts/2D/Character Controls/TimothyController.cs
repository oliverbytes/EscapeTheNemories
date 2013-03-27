using UnityEngine;
using System.Collections;

public class TimothyController : MonoBehaviour
{
	// public variables
	public float runSpeed = 10f;
	public float dashSpeed = 15f;
	public float brakeSpeed = 2f;
	public float jumpSpeed = 30f;
	public float inAirMultiplier = 0.25f;
	public float dashDuration = 5f;
	public int maxCoolDowns = 2;
	public float dashCoolDownDuration = 3f;
	public Vector3 moveDirection = Vector3.zero;
	
	// private variables
	private float dashDurationCopy;
	private float dashCoolDownDurationCopy = 5f;
	private CharacterController characterController;
	private Vector3 velocity;	
	private bool jumping = false;
	private bool dashing = false;
	private bool dashCoolDownTimerEnabled = false;
	private Vector3 timothyMovement;
	private TimothyAnimation timothyAnimation;
	private bool isBraking = false;
	
	void Start ()
	{
		characterController = gameObject.GetComponent<CharacterController>();
		timothyMovement = Vector3.zero;
		dashDurationCopy = dashDuration;
		dashCoolDownDurationCopy = dashCoolDownDuration;
		timothyAnimation = gameObject.GetComponent<TimothyAnimation>();
	}
	
	public void TimothyDash()
	{
		if(dashCoolDownTimerEnabled == false)
		{
			if(maxCoolDowns > 0) // has cool downs left
			{
				maxCoolDowns--; // decrease cool downs
				dashing = true;
				dashCoolDownTimerEnabled = true;
				timothyAnimation.run.speed = 3f;
			}
			else // if no more cool downs
			{
				// no more cool downs
			}
		}
	}
	
	public void TimothyBrake()
	{
		timothyMovement = Vector3.right * brakeSpeed;
		timothyAnimation.run.speed = 0.7f;
	}
	
	public void TimothyStartBraking()
	{
		isBraking = true;
	}
	
	public void TimothyStopBraking()
	{
		timothyAnimation.run.speed = 1.5f;
		isBraking = false;
	}
	
	public void TimothyJump()
	{
		velocity = characterController.velocity;
		velocity.y = jumpSpeed;	
		SendMessage ("DidJump", SendMessageOptions.DontRequireReceiver);
		jumping = true;
	}
	
	public void TimothySlide()
	{
		
	}
	
	void Update ()
	{
		if(dashing) // start dashing
		{
			dashDuration -= Time.deltaTime;
			timothyMovement = Vector3.right * dashSpeed;
		}
		else // not dashing
		{
			timothyMovement = Vector3.right * runSpeed;
		}
		
		if (dashDuration <= 0) // dashing finished
	    {
			dashing = false;
			dashCoolDownTimerEnabled = true;
			timothyAnimation.run.speed = timothyAnimation.runAnimationSpeedModifier;
			dashDuration = dashDurationCopy;
		}
		
		if(dashCoolDownTimerEnabled) // start cooldown timer
		{
			dashCoolDownDuration -= Time.deltaTime;
			//Debug.Log("cooling down: " + dashCoolDownDuration);
		}
		
		if(dashCoolDownDuration <= 0) // cooldown finished
		{
			dashCoolDownTimerEnabled = false;
			dashCoolDownDuration = dashCoolDownDurationCopy;
			//Debug.Log("FINISHED cooling down: " + dashCoolDownDuration);
		}
		
		if (characterController.isGrounded) // if characterController can jump
		{
			if(jumping) // if landed
			{
				SendMessage ("DidLand", SendMessageOptions.DontRequireReceiver);
				jumping = false;
			}
		
			if (Input.GetButtonDown("Jump"))
			{
				this.TimothyJump();
			}
			
			if (Input.GetKeyDown(KeyCode.RightArrow)) // dash
			{
				this.TimothyDash();
			}
			
			if (Input.GetKey(KeyCode.LeftArrow)) // brake, slow down
			{
				this.TimothyBrake();
			}
			
			if(isBraking)
			{
				this.TimothyBrake();
			}
			
			if (Input.GetKeyUp(KeyCode.LeftArrow)) // brake, slow down
			{
				timothyAnimation.run.speed = timothyAnimation.runAnimationSpeedModifier;
			}
			
			if (Input.GetKeyDown(KeyCode.DownArrow)) // slide
			{
				//SendMessage ("Slide", SendMessageOptions.DontRequireReceiver);
			}
			
			if (Input.GetKey(KeyCode.UpArrow)) // fly
			{
				float hoverForce = 12f;
				gameObject.rigidbody.AddForce(Vector3.up * hoverForce, ForceMode.Acceleration);
			}
		}
		else // if characterController is in Air
		{
			velocity.y += Physics.gravity.y * Time.deltaTime;
			timothyMovement.x *= inAirMultiplier;
		}
			
		timothyMovement += velocity;	
		timothyMovement += Physics.gravity;
		timothyMovement *= Time.deltaTime;
		
		characterController.Move( timothyMovement ); // Actually move the characterController	
		
		if ( characterController.isGrounded )
		{
			velocity = Vector3.zero; // Remove any persistent velocity after landing	
		}
	}
	
	void OnEndGame()
	{
		this.enabled = false; // Disable this Script
	}
}

