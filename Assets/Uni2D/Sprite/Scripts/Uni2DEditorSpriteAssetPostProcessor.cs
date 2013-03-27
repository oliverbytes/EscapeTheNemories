#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Uni2DTextureAndImportGUIDPair = System.Collections.Generic.KeyValuePair<string, UnityEngine.Texture2D>;

public class Uni2DEditorSpriteAssetPostProcessor : AssetPostprocessor {

	public delegate void OnPostprocessDelegate( );
	private static OnPostprocessDelegate m_dOnPostprocessHandler = null;
	private static bool ms_bEnabled = true;

	public static OnPostprocessDelegate OnPostprocessHandler
	{
		get
		{
			return m_dOnPostprocessHandler;
		}
		set
		{
			m_dOnPostprocessHandler = value;
		}
	}

	public static bool Enabled
	{
		get
		{
			return ms_bEnabled;
		}
		set
		{
			ms_bEnabled = value;
		}
	}
	
	// On preprocess texture
	private void OnPreprocessTexture()
	{			
		if(ms_bEnabled)
		{
			TextureImporter textureImporter = assetImporter as TextureImporter;
			Texture2D rTexture = AssetDatabase.LoadAssetAtPath(textureImporter.assetPath, typeof(Texture2D)) as Texture2D;
			if(rTexture != null)
			{
				string oNewTextureImportGUID = Uni2DEditorUtils.GenerateTextureImportGUID(rTexture);
				
				ms_oChangedTextures.Add(new Uni2DTextureAndImportGUIDPair(oNewTextureImportGUID, rTexture));
				
				// Update the sprite resources and those of the current scene
				EditorApplication.delayCall += OnTextureChange;
			}
			else
			{
				Uni2DEditorUtilsSpriteBuilder.SetDefaultTextureImporterSettings(textureImporter);
			}
		}
	}
	
	// Post process reimported prefabs
	private static List<Uni2DTextureAndImportGUIDPair> ms_oChangedTextures = new List<Uni2DTextureAndImportGUIDPair>();
	private static void OnTextureChange()
	{
		EditorApplication.delayCall -= OnTextureChange;
		
		foreach(Uni2DTextureAndImportGUIDPair oTextureAndImportGUIDPair in ms_oChangedTextures)
		{
			Uni2DEditorUtilsSpriteBuilder.UpdateSpriteInCurrentSceneAndResourcesAccordinglyToTextureChange(oTextureAndImportGUIDPair.Value, oTextureAndImportGUIDPair.Key);
		}
		ms_oChangedTextures.Clear();
	}
	
	public static void OnPostprocessAllAssets( string[] a_rImportedAssets, string[] a_rDeletedAssets, string[] a_rMovedAssets, string[] a_rMovedFromPath )
	{
		if(ms_bEnabled)
		{	
			// Loop through all the newly imported prefabs
			foreach(string rImportedAssetPath in a_rImportedAssets)
			{
				// Atlas
				GameObject rPrefab = AssetDatabase.LoadAssetAtPath(rImportedAssetPath, typeof(GameObject)) as GameObject;
				if(rPrefab != null && ms_oImportedPrefabs.Contains(rPrefab) == false)
				{
					ms_oImportedPrefabs.Add(rPrefab);
				}
			}
			
			// Loop through all the moved prefabs
			foreach(string rMovedAssetPath in a_rMovedAssets)
			{
				// Atlas
				GameObject rPrefab = AssetDatabase.LoadAssetAtPath(rMovedAssetPath, typeof(GameObject)) as GameObject;
				if(rPrefab != null && ms_oImportedPrefabs.Contains(rPrefab) == false)
				{
					ms_oImportedPrefabs.Add(rPrefab);
				}
			}
			
			if(ms_oImportedPrefabs.Count > 0 && ms_bReimportPrefabAlreadyCalled == false)
			{
				//Debug.Log("Ask for OnPostprocessReimportedPrefabs");
				ms_bReimportPrefabAlreadyCalled = true;
				EditorApplication.delayCall += OnPostprocessReimportedPrefabs;
			}
		}
	}
	
	// Post process reimported prefabs
	private static List<GameObject> ms_oImportedPrefabs = new List<GameObject>();
	private static bool ms_bReimportPrefabAlreadyCalled = false;
	private static void OnPostprocessReimportedPrefabs()
	{
		// Copy
		List<GameObject> oPrefabsToPostprocess = new List<GameObject>();
		oPrefabsToPostprocess.AddRange(ms_oImportedPrefabs);
		
		EditorApplication.delayCall -= OnPostprocessReimportedPrefabs;
		ms_oImportedPrefabs.Clear();
		ms_bReimportPrefabAlreadyCalled = false;
		
		//Debug.Log("OnPostprocessReimportedPrefabs");
		// Loop through all the newly imported prefabs
		foreach(GameObject rImportedPrefab in oPrefabsToPostprocess)
		{
			if(rImportedPrefab != null)
			{
				Uni2DEditorUtilsSpriteBuilder.OnPrefabPostProcess(rImportedPrefab);
			}
		}
	}
}
#endif