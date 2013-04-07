using UnityEngine;
using System.Collections;

public class IOClod : MonoBehaviour {
	
	public float Lod1;
	public float Lod2;
	public float LodMargin;
	public bool LodOnly;
	
	private float lod_1;
	private float lod_2;
	private float lodMargin;
	private bool realtimeShadows;
	private IOCcam iocCam;
	private int counter;
	private Renderer[] rs0;
	private Renderer[] rs1;
	private Renderer[] rs2;
	private Renderer[] rs;
	private bool hidden;
	private int currentLod;
	private float prevDist;
	private float distOffset;
	private int lods;
	private float dt;
	private float lodDistanceFromCam;
	private float hitTimeOffset;
	private float prevHitTime;
	private bool sleeping;
	
	private Shader shInvisible;
	private Shader[] sh;
	private Shader[] sh0;
	private float distanceFromCam;
	private float shadowDistance;
	private int frameInterval;
	
	void Awake () {
		shadowDistance = QualitySettings.shadowDistance * 2f;
		iocCam =  Camera.main.GetComponent<IOCcam>();
		if(iocCam == null)
		{
			this.enabled = false;
		}
		else
		{
			prevDist = 0f;
			prevHitTime = Time.time;
			sleeping = true;
		}
	}
	
	void Start () {
		UpdateValues();
		if(transform.Find("Lod_0"))
		{
			lods = 1;
			rs0 = transform.Find("Lod_0").GetComponentsInChildren<Renderer>(false);
			sh0 = new Shader[rs0.Length];
			for(int i=0; i<rs0.Length; i++)
			{
				sh0[i] = rs0[i].material.shader;
			}
			if(transform.Find("Lod_1"))
			{
				lods++;
				rs1 = transform.Find("Lod_1").GetComponentsInChildren<Renderer>(false);

				if(transform.Find("Lod_2"))
				{
					lods++;
					rs2 = transform.Find("Lod_2").GetComponentsInChildren<Renderer>(false);
				}
			}
		}
		else
		{
			lods = 0;
		}
		rs = GetComponentsInChildren<Renderer>(false);
		sh = new Shader[rs.Length];
		for(int i=0; i<rs.Length; i++)
		{
			sh[i] = rs[i].material.shader;
		}
		shInvisible = Shader.Find("Custom/Invisible");
		Initialize();
	}
	public void Initialize() {
		if(iocCam.enabled == true)
		{
			HideAll();
		}
		else
		{
			this.enabled = false;
			ShowLod(1);
		}
	}
	void Update () {
		frameInterval = Time.frameCount % 60;
		if(frameInterval == 0)
		{
			switch(LodOnly)
			{
			case false:
				if(!hidden && Time.frameCount - counter > iocCam.hideDelay)
				{
					Hide();
				}
				break;
			case true:
				if(!sleeping && Time.frameCount - counter > iocCam.hideDelay)
				{
					ShowLod(3000f);
					sleeping = true;
				}
				break;
			}
		}
		else if(realtimeShadows && frameInterval == 30)
		{
			distanceFromCam = Vector3.Distance(transform.position, iocCam.transform.position);
			if(hidden)
			{
				switch(lods)
				{
				case 0:
					if(distanceFromCam > shadowDistance)
					{
						if(rs[0].enabled)
						{
							for(int i=0;i<rs.Length;i++)
							{
								rs[i].enabled = false;
								rs[i].material.shader = sh[i];
							}
						}
					}
					else
					{
						if(!rs[0].enabled)
						{
							for(int i=0;i<rs.Length;i++)
							{
								rs[i].material.shader = shInvisible;
								rs[i].enabled = true;
							}
						}
					}
					break;
				default:
					if(distanceFromCam > shadowDistance)
					{
						if(rs0[0].enabled)
						{
							for(int i=0;i<rs0.Length;i++)
							{
								rs0[i].enabled = false;
								rs0[i].material.shader = sh0[i];
							}
						}
					}
					else
					{
						if(!rs0[0].enabled)
						{
							for(int i=0;i<rs0.Length;i++)
							{
								rs0[i].material.shader = shInvisible;
								rs0[i].enabled = true;
							}
						}
					}
					break;
				}
			}
		}
	}
	
	public void UpdateValues () {
		if(Lod1 != 0)
		{
			lod_1 = Lod1;
		}
		else lod_1 = iocCam.lod1Distance;
		if(Lod2 != 0)
		{
			lod_2 = Lod2;
		}
		else lod_2 = iocCam.lod2Distance;
		if(LodMargin != 0)
		{
			lodMargin = LodMargin;
		}
		else lodMargin = iocCam.lodMargin;
		realtimeShadows = iocCam.realtimeShadows;
	}
	
	public void UnHide(float d)
	{
		counter = Time.frameCount;
		if(hidden)
		{
			hidden = false;
			ShowLod(d);
		}
		else
		{
			if(lods > 0)
			{
				distOffset = prevDist - d;
				hitTimeOffset = Time.time - prevHitTime;
				prevHitTime = Time.time;
				if(Mathf.Abs(distOffset) > lodMargin | hitTimeOffset > 1f)
				{
					ShowLod(d);
					prevDist = d;
					sleeping = false;
				}
			}
		}
	}
	
	public void ShowLod(float d)
	{
		int i = 0;
		switch(lods)
		{
		case 0:
			currentLod = -1;
			break;
		case 2:
			if(d < lod_1)
			{
				currentLod = 0;
			}
			else
			{
				currentLod = 1;
			}
			break;
		case 3:
			if(d < lod_1)
			{
				currentLod = 0;
			}
			else if(d > lod_1 & d < lod_2)
			{
				currentLod = 1;
			}
			else
			{
				currentLod = 2;
			}
			break;
		}
		switch(currentLod)
		{
		case 0:
			if(!LodOnly && rs0[0].enabled)
			{
				for(i=0;i<rs0.Length;i++)
				{
					rs0[i].material.shader = sh0[i];
				}
			}
			else
			{
				for(i=0;i<rs0.Length;i++)
				{
					rs0[i].enabled = true;
				}
			}
			for(i=0;i<rs1.Length;i++)
			{
				rs1[i].enabled = false;
			}
			if(lods == 3)
			{
				for(i=0;i<rs2.Length;i++)
				{
					rs2[i].enabled = false;
				}
			}
			break;
		case 1:
			for(i=0;i<rs1.Length;i++)
			{
				rs1[i].enabled = true;
			}
			for(i=0;i<rs0.Length;i++)
			{
				rs0[i].enabled = false;
				if(!LodOnly && realtimeShadows)
				{
					rs0[i].material.shader = sh0[i];
				}
			}
			if(lods == 3)
			{
				for(i=0;i<rs2.Length;i++)
				{
					rs2[i].enabled = false;
				}
			}
			break;
		case 2:
			for(i=0;i<rs2.Length;i++)
			{
				rs2[i].enabled = true;
			}
			for(i=0;i<rs0.Length;i++)
			{
				rs0[i].enabled = false;
				if(!LodOnly && realtimeShadows)
				{
					rs0[i].material.shader = sh0[i];
				}
			}
			for(i=0;i<rs1.Length;i++)
			{
				rs1[i].enabled = false;
			}
			break;
		default:
            if (rs.Length > 0)
            {
                if (!LodOnly && rs[0].enabled)
                {
                    for (i = 0; i < rs.Length; i++)
                    {
                        rs[i].material.shader = sh[i];
                    }
                }
                else
                {
                    for (i = 0; i < rs.Length; i++)
                    {
                        rs[i].enabled = true;
                    }
                }
            }
			
			break;
		}
	}
	public void Hide()
	{
		int i = 0;
		hidden = true;
		switch(currentLod)
		{
		case 0:
			if(realtimeShadows && distanceFromCam <= shadowDistance)
			{
				for(i=0;i<rs0.Length;i++)
				{
					rs0[i].material.shader = shInvisible;
				}
			}
			else
			{
				for(i=0;i<rs0.Length;i++)
				{
					rs0[i].enabled = false;
				}
			}
			break;
		case 1:
			for(i=0;i<rs1.Length;i++)
			{
				rs1[i].enabled = false;
			}
			break;
		case 2:
			for(i=0;i<rs2.Length;i++)
			{
				rs2[i].enabled = false;
			}
			break;
		default:
			if(realtimeShadows && distanceFromCam <= shadowDistance)
			{
				for(i=0;i<rs.Length;i++)
				{
					rs[i].material.shader = shInvisible;
				}
			}
			else
			{
				for(i=0;i<rs.Length;i++)
				{
					rs[i].enabled = false;
				}
			}
			break;
		}
	}
	public void HideAll()
	{
		int i = 0;
		switch(LodOnly)
		{
		case false:
			hidden = true;
			if(lods == 0 && rs != null)
			{
				for(i=0;i<rs.Length;i++)
				{
					rs[i].enabled = false;
					if(realtimeShadows)
					{
						rs[i].material.shader = sh[i];
					}
				}
			}
			else
			{
				for(i=0;i<rs0.Length;i++)
				{
					rs0[i].enabled = false;
					if(realtimeShadows)
					{
						rs0[i].material.shader = sh0[i];
					}
				}
				for(i=0;i<rs1.Length;i++)
				{
					rs1[i].enabled = false;
				}
				for(i=0;i<rs2.Length;i++)
				{
					rs2[i].enabled = false;
				}
			}
			break;
		case true:
			prevHitTime = prevHitTime - 3f;
			ShowLod(3000f);
			break;
		}
	}
}
