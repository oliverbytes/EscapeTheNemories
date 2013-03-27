private var cameraScrolling : CameraScrolling;
var targets : Transform[];

function Awake () {

	// Get the reference to our CameraScrolling script attached to this camera;
	cameraScrolling = GetComponent (CameraScrolling);
	
	// Set the scrolling camera's target to be our character at the start.
	cameraScrolling.SetTarget (targets[0], true);
	
	// tell all targets (except the first one) to switch off.
	for (var i=0; i < targets.Length; i++)
	{
		targets[i].gameObject.SendMessage ("SetControllable", (i == 0), SendMessageOptions.DontRequireReceiver);
	}
}

@script RequireComponent (CameraScrolling)
