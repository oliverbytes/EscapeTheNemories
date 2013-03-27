#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditorInternal;

// Uni2D Utils
public static class Uni2DEditorUtils 
{	
	// The prefix for the texture import guid label
	public const string mc_oTextureImportGUIDPrefix = "Uni2DID_";
	
	// The atlas label for the texture import guid label
	public const string mc_oTextureAtlasLabel = "Uni2D_TextureAtlas";
	
	// Clear log
	public static void ClearLog()
	{
		/*Assembly assembly = Assembly.GetAssembly(typeof(Macros));
        Type type = assembly.GetType("UnityEditorInternal.LogEntries");
        MethodInfo method = type.GetMethod ("Clear");
        method.Invoke (new object (), null);*/
	}
	
	// Get selected asset folder path
	public static string GenerateNewPrefabLocalPath(string a_rPrefabName)
	{
		string oAssetPath = GetLocalSelectedAssetFolderPath() + a_rPrefabName + ".prefab";
		oAssetPath = AssetDatabase.GenerateUniqueAssetPath(oAssetPath);
		
		return oAssetPath;
	}
	
	// Get selected asset folder path
	public static string GetLocalSelectedAssetFolderPath()
	{
		return GetLocalAssetFolderPath(Selection.activeObject);
	}
	
	// Get selected asset folder path
	public static string GetGlobalSelectedAssetFolderPath()
	{
		return GetGlobalAssetFolderPath(Selection.activeObject);
	}
	
	// Get asset folder path
	public static string GetLocalAssetFolderPath(UnityEngine.Object a_rAsset)
	{
		return GlobalToLocalAssetPath(GetGlobalAssetFolderPath(a_rAsset));
	}
	
	// Get asset folder path
	public static string GetGlobalAssetFolderPath(UnityEngine.Object a_rAsset)
	{
		string oAssetPath = AssetDatabase.GetAssetPath(a_rAsset);
		string oAssetFolderPath = "";
		if (oAssetPath.Length > 0)
		{
			oAssetFolderPath = Application.dataPath + "/" + oAssetPath.Substring(7);
			oAssetFolderPath = oAssetFolderPath.Replace('\\', '/');
			if ((File.GetAttributes(oAssetFolderPath) & FileAttributes.Directory) != FileAttributes.Directory)
			{
				for (int i = oAssetFolderPath.Length - 1; i > 0; --i)
				{
					if (oAssetFolderPath[i] == '/')
					{
						oAssetFolderPath = oAssetFolderPath.Substring(0, i);
						break;
					}
				}
			}
			oAssetFolderPath += "/";
		}
		else
		{
			oAssetFolderPath = Application.dataPath + "/";
		}
		
		return oAssetFolderPath;
	}
	
	// Local to global asset path
	public static string LocalToGlobalAssetPath(string a_rLocalPath)
	{
		return Application.dataPath.Replace("Assets", "") + a_rLocalPath;
	}
	
	// Global to local asset path
	public static string GlobalToLocalAssetPath(string a_rGlobalPath)
	{
		return a_rGlobalPath.Replace(Application.dataPath.Replace("Assets", ""), "");
	}
	
	// Get global asset path
	public static string GetGlobalAssetPath(UnityEngine.Object a_rAsset)
	{
		return LocalToGlobalAssetPath(GetLocalAssetPath(a_rAsset));
	}
		
	// Get local asset path
	public static string GetLocalAssetPath(UnityEngine.Object a_rAsset)
	{
		return AssetDatabase.GetAssetPath(a_rAsset);
	}	
	
	// Fins the first texture atlas
	public static Uni2DTextureAtlas FindFirstTextureAtlas(Texture2D a_rTexture)
	{
		// Find the selectable atlas
		foreach(string rAssetPath in AssetDatabase.GetAllAssetPaths())
		{
			GameObject rPrefab = AssetDatabase.LoadAssetAtPath(rAssetPath, typeof(GameObject)) as GameObject;
			if(rPrefab != null)
			{
				Uni2DTextureAtlas rAtlas = rPrefab.GetComponent<Uni2DTextureAtlas>();
				if(rAtlas != null)
				{
					if(rAtlas.Contains(a_rTexture))
					{
						return rAtlas;
					}
				}
			}
		}
		
		return null;
	}
	
	// Generate Texture import GUID label
	// return true if there was no GUID on the texture
	public static string GenerateTextureImportGUID(Texture2D a_rTexture)
	{
		//Debug.Log("Generate texture import GUID");
		int iLabelIndex;
		string[] oLabels = AssetDatabase.GetLabels(a_rTexture);
		string oTextureImportGUID = Guid.NewGuid().ToString();
		string oTextureImportGUIDLabel = mc_oTextureImportGUIDPrefix + oTextureImportGUID;
		if(TryFindTextureImportGUIDLabel(oLabels, out iLabelIndex))
		{
			oLabels[iLabelIndex] = oTextureImportGUIDLabel;
			AssetDatabase.SetLabels(a_rTexture, oLabels);
		}
		else
		{
			string[] oNewLabels = new string[oLabels.Length + 1];
			Array.Copy(oLabels, oNewLabels, oLabels.Length);
			oNewLabels[oNewLabels.Length - 1] = oTextureImportGUIDLabel;
			AssetDatabase.SetLabels(a_rTexture, oNewLabels);
		}
		
		return oTextureImportGUIDLabel;
	}
	
	// Get Texture import GUID label
	public static string GetTextureImportGUID(Texture2D a_rTexture)
	{
		int iLabelIndex;
		string[] oLabels = AssetDatabase.GetLabels(a_rTexture);
		if(TryFindTextureImportGUIDLabel(oLabels, out iLabelIndex))
		{
			return oLabels[iLabelIndex].Replace(mc_oTextureImportGUIDPrefix, "");
		}
		return "";
		/*else
		{
			// Generate GUID
			string oTextureImportGUID = GenerateTextureImportGUID(a_rTexture);
			
			// Reimport
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(a_rTexture));
			AssetDatabase.Refresh();
			
			return oTextureImportGUID;
		}*/
	}
	
	// Generate Texture import GUID label
	private static bool TryFindTextureImportGUIDLabel(string[] a_rLabels, out int a_iIndex)
	{
		a_iIndex = -1;
		int iIndex = 0;
		foreach(string rLabel in a_rLabels)
		{
			if(rLabel.Contains(mc_oTextureImportGUIDPrefix))
			{
				a_iIndex = iIndex;
				return true;
			}
			iIndex++;
		}
		return false;
	}
	
	// It'is the first time we use the texture?
	public static bool ItIsTheFirstTimeWeUseTheTexture(Texture2D a_rTexture)
	{
		if(a_rTexture != null)
		{
			string[] rLabels = AssetDatabase.GetLabels(a_rTexture);
			foreach(string rLabel in rLabels)
			{
				if(rLabel.Contains(mc_oTextureImportGUIDPrefix))
				{
					return false;
				}
			}
			return true;
		}
		else
		{
			return false;
		}
	}
	
	// Mark as atlas
	public static void MarkAsTextureAtlas(Texture2D a_rTexture)
	{
		int iLabelIndex;
		string[] oLabels = AssetDatabase.GetLabels(a_rTexture);
		if(TryFindTextureAtlasLabel(oLabels, out iLabelIndex))
		{
			oLabels[iLabelIndex] = mc_oTextureAtlasLabel;
			AssetDatabase.SetLabels(a_rTexture, oLabels);
		}
		else
		{
			string[] oNewLabels = new string[oLabels.Length + 1];
			Array.Copy(oLabels, oNewLabels, oLabels.Length);
			oNewLabels[oNewLabels.Length - 1] = mc_oTextureAtlasLabel;
			AssetDatabase.SetLabels(a_rTexture, oNewLabels);
		}
	}
	
	// Generate Texture import GUID label
	private static bool TryFindTextureAtlasLabel(string[] a_rLabels, out int a_iIndex)
	{
		a_iIndex = -1;
		int iIndex = 0;
		foreach(string rLabels in a_rLabels)
		{
			if(rLabels.Contains(mc_oTextureAtlasLabel))
			{
				a_iIndex = iIndex;
				return true;
			}
			iIndex++;
		}
		return false;
	}
	
	// Contains at least a sprite
	public static bool IsPrefabContainsAtLeastASprite(GameObject a_rPrefab)
	{
		return IsPrefabContainsAtLeastASprite(a_rPrefab.transform);
	}
	
	// Contains at least a sprite
	private static bool IsPrefabContainsAtLeastASprite(Transform a_rRoot)
	{
		Uni2DSprite rSprite = a_rRoot.GetComponent<Uni2DSprite>();
		if(rSprite != null)
		{
			return true;
		}
		
		// Recursive call
		foreach(Transform rChild in a_rRoot)
		{	
			if(IsPrefabContainsAtLeastASprite(rChild))
			{
				return true;
			}
		}
		
		return false;
	}
	
	// Ping a prefab or a visible parent in project view
	public static void PingPrefabInProjectView(GameObject a_rGameObject)
	{
		GameObject rGameObjectToPing = a_rGameObject;
		if(a_rGameObject.transform.parent != null && a_rGameObject.transform.parent.parent != null)
		{
			rGameObjectToPing = PrefabUtility.FindPrefabRoot(a_rGameObject);
		}
		
		EditorGUIUtility.PingObject(rGameObjectToPing);
	}
}
#endif