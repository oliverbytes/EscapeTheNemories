#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class Uni2DTextureAtlasImporter : AssetPostprocessor
{
	public static bool importNewAtlasTexture = false;
	
	// On preprocess texture
	public void OnPreprocessTexture()
	{
		TextureImporter rTextureImporter = assetImporter as TextureImporter; 
			
		if(importNewAtlasTexture)
		{
			Uni2DTextureAtlas.SetDefaultAtlasTextureImportSettings(rTextureImporter);
		}
		
		// The texture process
		if(Uni2DEditorSpriteAssetPostProcessor.Enabled)
		{
			Texture2D rTexture = AssetDatabase.LoadAssetAtPath(rTextureImporter.assetPath, typeof(Texture2D)) as Texture2D;
			if(rTexture != null)
			{
				ms_rChangedTextures.Add(rTexture);
				EditorApplication.delayCall += NotifyAtlasesContainingATextureOfItsChanges;
			}
		}
	}
		
	// Notif all the atlases containing a texture
	// that this texture has changed
	static List<Texture2D> ms_rChangedTextures = new List<Texture2D>();
	private static void NotifyAtlasesContainingATextureOfItsChanges()
	{	
		EditorApplication.delayCall -= NotifyAtlasesContainingATextureOfItsChanges;
		foreach(Texture2D rTexture in ms_rChangedTextures)
		{
			// Loop through all the atlases in the resources
			foreach(string rAssetPath in AssetDatabase.GetAllAssetPaths())
			{
				GameObject rPrefab = AssetDatabase.LoadAssetAtPath(rAssetPath, typeof(GameObject)) as GameObject;
				if(rPrefab != null)
				{
					Uni2DTextureAtlas rAtlas = rPrefab.GetComponent<Uni2DTextureAtlas>();
					if(rAtlas != null)
					{
						if(rAtlas.Contains(rTexture))
						{
							rAtlas.OnTextureChange();
						}
					}
				}
			}
		}
		ms_rChangedTextures.Clear();
	}
}
#endif