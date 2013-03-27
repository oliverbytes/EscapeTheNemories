var lineMaterial : Material;
var line : LineRenderer;

function Start()
{ 
	line = this.gameObject.AddComponent(LineRenderer); 
	line.SetWidth(.5, .5); 
	line.SetVertexCount(2); 
	// line.useWorldSpace = true; 
	line.material = lineMaterial; 
	line.SetColors(Color(1,0,0,1), Color(1,0,0,1)); 
	//line.renderer.enabled = false; 
	//the line is later enabled 
} 