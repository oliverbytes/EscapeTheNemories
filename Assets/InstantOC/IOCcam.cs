using UnityEngine;
using System.Collections;

public class IOCcam : MonoBehaviour {
	public LayerMask layerMsk;
	public int samples;
	public float viewDistance;
	public int hideDelay;
	public bool realtimeShadows;
	public float lod1Distance;
	public float lod2Distance;
	public float lodMargin;
	
	private RaycastHit hit;
	private Ray r;
	private int layerMask;
	private IOClod l;
	private int haltonIndex;
	private float[] hx;
	private float[] hy;
	private int pixels;
	private Camera cam;
	
	void Awake () {
		cam = camera;
		hit = new RaycastHit();
		if(viewDistance == 0) viewDistance = 100;
		cam.farClipPlane = viewDistance;
		haltonIndex = 0;
	}
	
	void Start () {
		pixels = Mathf.FloorToInt(Screen.width * Screen.height / 2f);
		hx = new float[pixels];
		hy = new float[pixels];
		for(int i=0; i < pixels; i++)
		{
			hx[i] = HaltonSequence(i, 2);
			hy[i] = HaltonSequence(i, 3);
		}
	}
	
	void Update () {
		for(int k=0; k <= samples; k++)
		{
			r = cam.ViewportPointToRay(new Vector3(hx[haltonIndex], hy[haltonIndex], 0f));
			haltonIndex++;
			if(haltonIndex >= pixels) haltonIndex = 0;
			if(Physics.Raycast(r, out hit, viewDistance, layerMsk.value))
			{
				if(l = hit.transform.GetComponent<IOClod>())
				{
					l.UnHide(hit.distance);
				}
				else if(l = hit.transform.parent.GetComponent<IOClod>())
				{
					l.UnHide(hit.distance);
				}
			}
		}
	}
	
	private float HaltonSequence(int index, int b)
	{
		float res = 0f;
		float f = 1f / b;
		int i = index;
		while(i > 0)
		{
			res = res + f * (i % b);
			i = Mathf.FloorToInt(i/b);
			f = f / b;
		}
		return res;
	}
}