using UnityEngine;
using System.Collections;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

public class TimothyController : MonoBehaviour
{
	// public variables
	public int lives = 5;
	public int airDashes = 5;
	public int groundDashes = 5;
	public float runSpeed = 10f;
	public float dashSpeed = 15f;
	public float brakeSpeed = 2f;
	public float jumpSpeed = 30f;
    public int maxJumps = 2;
	public float inAirMultiplier = 0.25f;
	public float groundDashDuration = 1f;
	public float airDashDuration = 1f;
	public float groundDashCoolDownDuration = 3f;
	public float airDashCoolDownDuration = 3f;
    public float slideDuration = 1f;
	
	// private variables
	private float groundDashDurationCopy;
	private float groundDashCoolDownDurationCopy;
	private float airDashDurationCopy;
	private float airDashCoolDownDurationCopy;
    private float slideDurationCopy;
	private bool jumping = false;
	private bool groundDashing = false;
	private bool airDashing = false;
	private bool isBraking = false;
    private bool isSliding = false;
    private bool isSmashing = false;
	private bool groundDashCoolDownTimerEnabled = false;
	private bool airDashCoolDownTimerEnabled = false;
    private bool slideDurationTimerEnabled = false;
    private int maxJumpsCopy = 0;
   
	private Vector3 moveDirection = Vector3.zero;
	private CharacterController characterController;
	private Vector3 velocity;
	private Vector3 timothyMovement;
	private Transform timothyTransform;
	private TimothyAnimation timothyAnimation;
	private SkinnedMeshRenderer timothySkinnedMeshRenderer;
	private Transform spawnPoint;
	
	void Start ()
	{
		characterController = gameObject.GetComponent<CharacterController>();
		timothyTransform = gameObject.GetComponent<Transform>();
		timothySkinnedMeshRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		spawnPoint = GameObject.Find("SpawnPoint").transform;
		timothyMovement = Vector3.zero;
		groundDashDurationCopy = groundDashDuration;
		groundDashCoolDownDurationCopy = groundDashCoolDownDuration;
		timothyAnimation = gameObject.GetComponent<TimothyAnimation>();
        maxJumpsCopy = maxJumps;
        slideDurationCopy = slideDuration;
	}
	
	void GameOver()
	{
		Debug.Log("Game Over");
	}
	
	void DecreaseLives()
	{
		lives--;
		
		if(lives < 1)
		{
			GameOver();
		}
	}

    public IEnumerator TimothyStepOnStone() 
	{
		DecreaseLives();
        
        // flash
        timothySkinnedMeshRenderer.enabled = false;
        yield return new WaitForSeconds(0.1f);
        timothySkinnedMeshRenderer.enabled = true;
        yield return new WaitForSeconds(0.1f);
        timothySkinnedMeshRenderer.enabled = false;
        yield return new WaitForSeconds(0.1f);
        timothySkinnedMeshRenderer.enabled = true;
        yield return new WaitForSeconds(0.1f);
        timothySkinnedMeshRenderer.enabled = false;
        yield return new WaitForSeconds(0.1f);
        timothySkinnedMeshRenderer.enabled = true;
        yield return new WaitForSeconds(0.1f);
        timothySkinnedMeshRenderer.enabled = false;
        yield return new WaitForSeconds(0.1f);
        timothySkinnedMeshRenderer.enabled = true;
    }
	
	public void TimothyDash()
	{
		if(groundDashCoolDownTimerEnabled == false)
		{
			if(groundDashes > 0) // has more dashes
			{
				groundDashes--;
				groundDashing = true;
				groundDashCoolDownTimerEnabled = true;
				timothyAnimation.run.speed = 3f;
			}
			else // if no more dashes
			{
				
			}
		}
	}

    public void TimothyFinishedDashing()
    {
        groundDashing = false;
        groundDashCoolDownTimerEnabled = true;
        timothyAnimation.run.speed = timothyAnimation.runAnimationSpeedModifier;
        groundDashDuration = groundDashDurationCopy;
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
        if (maxJumps > 1)
        {
            if(isSliding)
            {
                this.TimothyFinishedSliding();
            }

            SendMessage("TimothyAnimationJump", SendMessageOptions.DontRequireReceiver);
            velocity = characterController.velocity;
            velocity.y = jumpSpeed;
            jumping = true;
            maxJumps--;
        }
	}

    public void TimothyFinishedJumping()
    {
        maxJumps = maxJumpsCopy; // reset maxJumps

        if (jumping) // if landed
        {
            SendMessage("TimothyAnimationLand", SendMessageOptions.DontRequireReceiver);
            jumping = false;
        }
    }

	public void TimothySlide()
	{
        if(!isSliding)
        {
            isSliding = true;
            slideDurationTimerEnabled = true;
            transform.Rotate(-50f, 0f, 0f);
            characterController.height = 0.9f;
            SendMessage("TimothyAnimationSlide", SendMessageOptions.DontRequireReceiver);
        }
	}

    public void TimothyFinishedSliding()
    {
        isSliding = false;
        slideDurationTimerEnabled = false;
        slideDuration = slideDurationCopy;
        characterController.height = 1.69f;
        transform.Rotate(50f, 0f, 0f);
        SendMessage("TimothyAnimationRun", SendMessageOptions.DontRequireReceiver);
        // hack so characterController won't fall
        velocity = characterController.velocity;
        velocity.y = 25f;
    }

    public void TimothySwipedDown()
    {
        if (characterController.isGrounded)
        {
            this.TimothySlide();
        }
        else
        {
            this.TimothySmash();
        }
    }

    public void TimothySmash()
    {
        isSmashing = true;
        SendMessage("TimothyAnimationSmash", SendMessageOptions.DontRequireReceiver);
        velocity = characterController.velocity;
        velocity.y = -jumpSpeed;
    }

    public void TimothyFinishSmash()
    {
        isSmashing = false;
        SendMessage("TimothyAnimationRun", SendMessageOptions.DontRequireReceiver);
    }
	
	void Update ()
	{
		if(groundDashing) // start groundDashing
		{
			groundDashDuration -= Time.deltaTime;
			timothyMovement = Vector3.right * dashSpeed;
		}
		else // not groundDashing
		{
			timothyMovement = Vector3.right * runSpeed;
		}
		
		if (groundDashDuration <= 0) // groundDashing finished
	    {
            this.TimothyFinishedDashing();
		}
		
		if(groundDashCoolDownTimerEnabled) // start cooldown timer
		{
			groundDashCoolDownDuration -= Time.deltaTime;
			//Debug.Log("cooling down: " + groundDashCoolDownDuration);
		}
		
		if(groundDashCoolDownDuration <= 0) // cooldown finished
		{
			groundDashCoolDownTimerEnabled = false;
			groundDashCoolDownDuration = groundDashCoolDownDurationCopy;
		}

        if(slideDurationTimerEnabled)
        {
            slideDuration -= Time.deltaTime;
        }

        if (slideDuration <= 0) // sliding duration finished
        {
            this.TimothyFinishedSliding();
        }
		
		if (characterController.isGrounded) // if characterController can jump
		{
            this.TimothyFinishedJumping();

            if(isSmashing)
            {
                this.TimothyFinishSmash();
            }
		
			if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.UpArrow)  || Input.GetKeyDown(KeyCode.W))
			{
				this.TimothyJump();
			}
			
			if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) // dash
			{
				this.TimothyDash();
			}
			
			if (Input.GetKey(KeyCode.A)  || Input.GetKey(KeyCode.LeftArrow)) // brake, slow down
			{
				this.TimothyBrake();
			}
			
			if(isBraking)
			{
				this.TimothyBrake();
			}
			
			if (Input.GetKeyUp(KeyCode.A)  || Input.GetKeyUp(KeyCode.LeftArrow)) // brake, slow down
			{
				timothyAnimation.run.speed = timothyAnimation.runAnimationSpeedModifier;
			}
			
			if (Input.GetKeyDown(KeyCode.DownArrow)) // slide
			{
                this.TimothySwipedDown();
			}
			
			if (Input.GetKey(KeyCode.UpArrow)) // fly
			{
				
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

