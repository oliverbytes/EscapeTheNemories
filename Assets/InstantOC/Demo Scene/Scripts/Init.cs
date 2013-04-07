using UnityEngine;
using System.Collections;

public class Init : MonoBehaviour {

	public GameObject[] Prefabs;
	public int PrefabNum;
	public float PosY;
	
	void Start() {
		GenerateLevel();
	}
	
	void GenerateLevel()
	{
		Vector3 prefabPos;
		GameObject go;
		GameObject[] gos = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach(GameObject g in gos)
		{
			if(g.layer == 8)
			{
				Destroy(g);
			}
		}
		for (var i = 0; i < PrefabNum; i++)
		{
			prefabPos = new Vector3(Random.Range(50f, 950f), PosY, Random.Range(50f, 950f));
			go = Instantiate(Prefabs[Random.Range(0, Prefabs.Length)], prefabPos, Quaternion.identity) as GameObject;
			go.transform.Rotate(Vector3.up, Random.Range(0f, 360f));
		}
	}
}
