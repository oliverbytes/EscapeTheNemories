using UnityEngine;
using System.Collections;

public class IOChud : MonoBehaviour {
	
	private Texture2D Icon;
	private bool iocActive;
	private IOCcam ioc;
	private bool realtimeShadows;
	private bool hud;
	private bool dirty;

	void Awake () {
		Icon = (Texture2D) Resources.Load("Icon");
		hud = false;
		dirty = false;
	}
	void Start () {
		ioc = Camera.main.transform.GetComponent<IOCcam>();
		iocActive = ioc.enabled;
	}
	
	void Update () {
		if(Input.GetKeyUp(KeyCode.I))
		{
			iocActive = !iocActive;
			ToggleIOC();
		}
		if(Input.GetKeyUp(KeyCode.Escape))
		{
			ToggleHUD();
		}
		if(Input.GetMouseButtonUp(0))
		{
			if(dirty)
			{
				ToggleIOC();
				dirty = false;
			}
		}
	}
	
	void OnGUI() {
		GUI.Label(new Rect(25f,10f, 360f,20f), "Press 'i' to toggle InstantOC - Press 'Esc' to toggle HUD");
		if(hud)
		{
			GUI.Label(new Rect(25f,35f, 320f,20f), "Samples");
			ioc.samples = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(25f,55f, 150f,20f), ioc.samples, 100f, 1500f));
			GUI.Label(new Rect(180f,50f, 50f,20f), ioc.samples.ToString());
			
			GUI.Label(new Rect(25f,65f, 320f,20f), "Hide delay");
			ioc.hideDelay = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(25f,85f, 150f,20f), ioc.hideDelay, 10, 300f));
			GUI.Label(new Rect(180f,80f, 50f,20f), ioc.hideDelay.ToString());
			
			GUI.Label(new Rect(25f,95f, 320f,20f), "View Distance");
			ioc.viewDistance = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(25f,115f, 150f,20f), ioc.viewDistance, 100, 3000f));
			GUI.Label(new Rect(180f,110f, 50f,20f), ioc.viewDistance.ToString());
			
			GUI.Label(new Rect(25f,125f, 320f,20f), "Lod 1");
			ioc.lod1Distance = Mathf.Round(GUI.HorizontalSlider(new Rect(25f,145f, 150f,20f), ioc.lod1Distance, 10f, 300f));
			GUI.Label(new Rect(180f,140f, 50f,20f), ioc.lod1Distance.ToString());
			
			GUI.Label(new Rect(25f,155f, 320f,20f), "Lod 2");
			ioc.lod2Distance = Mathf.Round(GUI.HorizontalSlider(new Rect(25f,175f, 150f,20f), ioc.lod2Distance, 10f, 600f));
			GUI.Label(new Rect(180f,170f, 50f,20f), ioc.lod2Distance.ToString());
			
			GUI.Label(new Rect(25f,185f, 320f,20f), "Lod margin");
			ioc.lodMargin = Mathf.Round(GUI.HorizontalSlider(new Rect(25f,205f, 150f,20f), ioc.lodMargin, 1f, 100f));
			GUI.Label(new Rect(180f,200f, 50f,20f), ioc.lodMargin.ToString());
		}
		if(iocActive)
		{
			GUI.Label(new Rect(Screen.width - 74f, 10f, 64f,64f), Icon);
		}
		if(GUI.changed)
		{
			dirty = true;
		}
	}
	private void ToggleHUD()
	{
		hud = !hud;
		ioc.GetComponent<MouseLook>().enabled = !ioc.GetComponent<MouseLook>().enabled;
		ioc.transform.parent.GetComponent<MouseLook>().enabled = !ioc.transform.parent.GetComponent<MouseLook>().enabled;
	}
	private void ToggleIOC()
	{
		ioc.enabled = iocActive;
		GameObject[] gos = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach(GameObject g in gos)
		{
			IOClod l = g.GetComponent<IOClod>();
			if(l != null)
			{
				switch(iocActive)
				{
				case true:
					l.UpdateValues();
					l.Initialize();
					l.enabled = true;
					break;
				case false:
					l.enabled = false;
					l.UpdateValues();
					l.Initialize();
					break;
				}
			}
		}
	}
}
