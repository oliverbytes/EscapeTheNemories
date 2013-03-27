#pragma strict

@script RequireComponent( CharacterController )

var forwardSpeed : float = 10f;
var brakeSpeed : float = 5f;
var jumpSpeed : float = 30f;
var inAirMultiplier : float = 0.25f;					// Limiter for ground speed while jumping

private var thisTransform : Transform;
private var character : CharacterController;
private var velocity : Vector3;						// Used for continuing momentum while in air
private var jumping = false;

function Start()
{	
	thisTransform = GetComponent( Transform );
	character = GetComponent( CharacterController );	
}

function OnEndGame()
{
	this.enabled = false; // Disable this Script
}

function Update()
{
	// always run
	var movement = Vector3.right * forwardSpeed;

	if (character.isGrounded) // Check for jump
	{
		if(jumping)
		{
			SendMessage ("DidLand", SendMessageOptions.DontRequireReceiver);
			jumping = false;
		}
	
		if (Input.GetButtonDown("Jump"))
		{
			velocity = character.velocity;
			velocity.y = jumpSpeed;	
			SendMessage ("DidJump", SendMessageOptions.DontRequireReceiver);
			jumping = true;
		}
		
		if (Input.GetKeyDown(KeyCode.RightArrow)) // run faster
		{
			Debug.Log("RUN FASTER");
		}
		
		if (Input.GetKey(KeyCode.LeftArrow)) // brake, slow down
		{
			movement = Vector3.right * brakeSpeed;
		}
		
		if (Input.GetKeyDown(KeyCode.DownArrow)) // slide
		{
			SendMessage ("Slide", SendMessageOptions.DontRequireReceiver);
		}
		
		if (Input.GetKeyDown(KeyCode.UpArrow)) // fly
		{
			Debug.Log("FLY");
		}
	}
	else // if character is in Air
	{
		velocity.y += Physics.gravity.y * Time.deltaTime;
		movement.x *= inAirMultiplier;
	}
		
	movement += velocity;	
	movement += Physics.gravity;
	movement *= Time.deltaTime;
	
	character.Move( movement ); // Actually move the character	
	
	if ( character.isGrounded )
	{
		velocity = Vector3.zero; // Remove any persistent velocity after landing	
	}
}