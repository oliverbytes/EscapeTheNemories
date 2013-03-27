using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using Uni2DTextureImporterSettingsPair = System.Collections.Generic.KeyValuePair<UnityEngine.Texture2D, UnityEditor.TextureImporterSettings>;
#endif

[AddComponentMenu("Uni2D/Sprite/Uni2DTextureAtlas")]
// Texture Atlas
public class Uni2DTextureAtlas : MonoBehaviour, IComparable
{	
	// Atlas size
	public enum EAtlasSize
	{
		_2_ = 2,
		_4_ = 4,
		_8_ = 8,
		_16_ = 16,
		_32_ = 32,
		_64_ = 64,
		_128_ = 128,
		_256_ = 256,
		_512_ = 512,
		_1024_ = 1024,
		_2048_ = 2048,
		_4096_ = 4096,
	}
	
	// Inspector settings
	
	// The inspector texture field
	public Texture2D[] textures;
	
	// Padding
	public int padding = 1;
	
	// Maximum atlas size
	public EAtlasSize maximumAtlasSize = (EAtlasSize)2048;
	
	// Generated Data
	
	// The atlas texture
	[HideInInspector]
	public Texture2D atlasTexture;
	
	// The atlas material
	[HideInInspector]
	public Material atlasMaterial;
	
	// The uv
	[HideInInspector]
	public Rect[] uvs;
	
	// The generation id
	[HideInInspector]
	public string generationId = "";
	
	// Parameters
	
	// The textures
	[HideInInspector]
	public Texture2D[] m_rTextures;
	
	// Padding
	[HideInInspector]
	public int m_iPadding = 1;
	
	// Maximum atlas size
	[HideInInspector]
	public EAtlasSize m_eMaximumAtlasSize = (EAtlasSize)2048;
		
	// Compare to
	public int CompareTo(System.Object a_rObject) 
	{
        if(a_rObject is Uni2DTextureAtlas) 
		{
            Uni2DTextureAtlas rOtherTextureAtlas = a_rObject as Uni2DTextureAtlas;

            return gameObject.name.CompareTo(rOtherTextureAtlas.gameObject.name);
        }
		
        return 1;
    }
	
#if UNITY_EDITOR
	// Unapplied Settings?
	public bool UnappliedSettings
	{
		get
		{
			bool bSettingsAreDifferentFromParameters = false;
			
			if(	m_eMaximumAtlasSize != maximumAtlasSize 
				||	m_iPadding != padding
				|| m_rTextures.Length != textures.Length)
			{
				bSettingsAreDifferentFromParameters = true;
			}
			else
			{
				int iTextureIndex = 0;
				foreach(Texture2D rTexture in m_rTextures)
				{
					if(rTexture != textures[iTextureIndex])
					{
						bSettingsAreDifferentFromParameters = true;
						break;
					}
					iTextureIndex++;
				}
			}
			
			return bSettingsAreDifferentFromParameters;
		}
	}

	// Apply settings
	public void ApplySettings()
	{
		m_rTextures = new Texture2D[textures.Length];
		textures.CopyTo(m_rTextures, 0);
		
		m_iPadding = padding;
		
		m_eMaximumAtlasSize = maximumAtlasSize;
		
		Generate();
	}
	
	// Revert settings
	public void RevertSettings()
	{
		textures = new Texture2D[m_rTextures.Length];
		m_rTextures.CopyTo(textures, 0);
		
		padding = m_iPadding;
		
		maximumAtlasSize = m_eMaximumAtlasSize;
	}
	
	// On texture change
	public void OnTextureChange()
	{
		//Debug.Log("Texture change");
		Generate();
	}
	
	// Contains the texture ?
	public bool Contains(Texture2D a_rTexture)
	{
		foreach(Texture2D rTexture in m_rTextures)
		{
			if(rTexture == a_rTexture)
			{
				return true;
			}
		}
		return false;
	}
	
	// Contains the texture ?
	public Rect GetUvs(Texture2D a_rTexture)
	{
		int iTextureIndex = 0;
		foreach(Texture2D rTexture in textures)
		{
			if(rTexture == a_rTexture)
			{
				return uvs[iTextureIndex];
			}
			iTextureIndex++;
		}
		return new Rect(0,0,1,1);
	}
	
	// Generate
	public void Generate()
	{	
		// Make sure the data directory exist
		string oGeneratedDataPathLocal = Uni2DEditorUtils.GetLocalAssetFolderPath(gameObject) + gameObject.name + "_GeneratedData" + "/";
		string oGeneratedDataPathGlobal = Uni2DEditorUtils.LocalToGlobalAssetPath(oGeneratedDataPathLocal);
		if(!Directory.Exists(oGeneratedDataPathGlobal))
		{
			Directory.CreateDirectory(oGeneratedDataPathGlobal);
		}
		
		GenerateAtlasTexture(oGeneratedDataPathLocal);
		GenerateAtlasMaterial(oGeneratedDataPathLocal);

		generationId = System.Guid.NewGuid().ToString();
		
		EditorUtility.SetDirty(this);
		AssetDatabase.SaveAssets();
		
		Uni2DEditorUtilsSpriteBuilder.UpdateSpriteInCurrentSceneAndResourcesAccordinglyToAtlasChange(this);
	}
	
	// Generate
	public void GenerateAtlasTexture(string a_oGeneratedDataPathLocal)
	{	
		// Prepare textures for atlasing
		List<Uni2DTextureImporterSettingsPair> oImporterSettingsPair = Uni2DEditorUtilsSpriteBuilder.TexturesProcessingBegin(m_rTextures);
		
		// Generate atlas texture
		
		bool bNewTexture = false;
		
		// Look if there is already a texture at the wanted path
		if(atlasTexture == null)
		{
			// Create texture 
			bNewTexture = true;
		}
		else
		{
			string oFolderPathLocal = Uni2DEditorUtils.GetLocalAssetFolderPath(atlasTexture);
			if(oFolderPathLocal != a_oGeneratedDataPathLocal)
			{	
				bNewTexture = true;
			}
		}
		// Create new texture
		Texture2D rOldAtlasTexture = atlasTexture;
		atlasTexture = new Texture2D(0, 0);
		
		if(bNewTexture)
		{
			atlasTexture.name = gameObject.name + "_AtlasTexture";
		}
		else
		{
			atlasTexture.name = rOldAtlasTexture.name;
		}
		
		// Get the atlas texture path
		string oAtlasTexturePathLocal = a_oGeneratedDataPathLocal + atlasTexture.name + ".png";
		string oAtlasTexturePathGlobal = Uni2DEditorUtils.LocalToGlobalAssetPath(oAtlasTexturePathLocal);
	
		// Packing
	    uvs = atlasTexture.PackTextures(m_rTextures, m_iPadding, (int)m_eMaximumAtlasSize);
		
		// Save texture
		string oAtlasTextureSavePathGlobal = oAtlasTexturePathGlobal;
		FileStream oFileStream = new FileStream(oAtlasTextureSavePathGlobal, FileMode.Create);
		BinaryWriter oBinaryWriter = new BinaryWriter(oFileStream);
		oBinaryWriter.Write(atlasTexture.EncodeToPNG());
		oBinaryWriter.Close();
		oFileStream.Close();
		
		// If we had just created a new texture set the default import settings
		if(bNewTexture)
		{
			ImportNewAtlasTexture(oAtlasTexturePathLocal);
		}
		else
		{
			AssetDatabase.ImportAsset(oAtlasTexturePathLocal);
		}
		
		// Set new texture
		atlasTexture = AssetDatabase.LoadAssetAtPath(oAtlasTexturePathLocal, typeof(Texture2D)) as Texture2D;
		
		// Mark the atlas texture
		//Uni2DEditorUtils.MarkAsTextureAtlas(atlasTexture);
		
		// Restore Texture import settings
		Uni2DEditorUtilsSpriteBuilder.TexturesProcessingEnd(oImporterSettingsPair);
	}
	
	// Generate
	public void GenerateAtlasMaterial(string a_oGeneratedDataPathLocal)
	{	
		bool bNewMaterial = false;
		
		// Create material 
		if(atlasMaterial == null)
		{
			atlasMaterial = new Material(Shader.Find(Uni2DEditorUtilsSpriteBuilder.mc_oSpriteDefaultShader));
			bNewMaterial = true;
		}
		else
		{
			string oFolderPathLocal = Uni2DEditorUtils.GetLocalAssetFolderPath(atlasMaterial);
			if(oFolderPathLocal != a_oGeneratedDataPathLocal)
			{	
				// Duplicate
				atlasMaterial = new Material(atlasMaterial);
				
				bNewMaterial = true;
			}
		}
		
		// If we have created a new material
		if(bNewMaterial)
		{
			atlasMaterial.name = gameObject.name + "_AtlasMaterial";
			string oMaterialPathLocal = a_oGeneratedDataPathLocal + atlasMaterial.name + ".mat";
			
			// Ensure the material can be created
			Material rMaterialAtWantedPath = AssetDatabase.LoadAssetAtPath(oMaterialPathLocal, typeof(Texture2D)) as Material;
			if(rMaterialAtWantedPath != null)
			{
				// Todo_Sev : ask user before deletion?
				AssetDatabase.DeleteAsset(oMaterialPathLocal);
			}
			
			// Create material
			AssetDatabase.CreateAsset(atlasMaterial, oMaterialPathLocal);
		}
		
		// Assign the atlas texture
		atlasMaterial.mainTexture = atlasTexture;
	}
	
	// Prepare Texture
	private void ImportNewAtlasTexture(string a_rTexturePathLocal)
	{
		Uni2DTextureAtlasImporter.importNewAtlasTexture = true;
		AssetDatabase.ImportAsset(a_rTexturePathLocal);
		Uni2DTextureAtlasImporter.importNewAtlasTexture = false;
	}
	
	// Prepare Texture
	public static void SetDefaultAtlasTextureImportSettings(TextureImporter a_rTextureImporter)
	{
		a_rTextureImporter.textureType = TextureImporterType.Advanced;
		a_rTextureImporter.mipmapEnabled = false;
		a_rTextureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
		a_rTextureImporter.maxTextureSize = 4096;
	}
#endif
}
