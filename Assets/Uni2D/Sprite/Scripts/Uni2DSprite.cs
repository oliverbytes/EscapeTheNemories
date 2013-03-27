using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

[AddComponentMenu("Uni2D/Sprite/Uni2DSprite")]
[ExecuteInEditMode()]
public class Uni2DSprite : MonoBehaviour 
{	
	public enum PhysicMode
	{
		NoPhysic,
		Static,
		Dynamic,
	};
	
	public enum CollisionType
	{
		Convex,
		Concave,
		Compound,
	};

	public enum PivotPointType
	{
		Custom,
		TopLeft,
		TopCenter,
		TopRight,
		MiddleLeft,
		MiddleCenter,
		MiddleRight,
		BottomLeft,
		BottomCenter,
		BottomRight
	}
	
	// The sprite texture used to build the mesh
	// and used by the quad game object
	public Texture2D spriteTexture = null;
	public float spriteTextureWidth = 0.0f;
	public float spriteTextureHeight = 0.0f;
	
	// The texture atlas
	public Uni2DTextureAtlas textureAtlas;
	
	// The quad mesh used by the quad game object
	public Mesh spriteQuadMesh = null;

	// The material used by the quad mesh
	public Material spriteQuadMaterial = null;

	// The mesh(es) built from the sprite texture
	public List<Mesh> meshCollidersList = new List<Mesh>();

	// (Compound mode only)
	// The game object parent of mesh collider game objects
	public GameObject meshCollidersRootGameObject = null;

	// The mesh collider components
	public List<MeshCollider> meshColliderComponentsList = new List<MeshCollider>();

	public PivotPointType pivotPointType;

	public Vector3 pivotPointCoords;

	// Scale of the mesh and quad
	public float spriteScale = 1.0f;

	// Depth of the built mesh
	public float extrusionDepth = 1.0f;

	// The accuracy the sprite contour was simplified and polygonized 
	public float polygonizationAccuracy = 5.0f;

	// The alpha threshold the sprite contour was extracted and poligonized
	public float alphaCutOff = 0.125f;

	// Wether or not holes were taken in account
	public bool polygonizeHoles = false;

	// The sprite physic modes
	public PhysicMode physicMode;
	
	// The collision type modes
	public CollisionType collisionType;
	
	// Kinematic?
	public bool isKinematic;
	
	// Up to date sprite mesh state: if the texture associated with this sprite mesh
	// has been modified, the sprite mesh of this texture may not correspond and need
	// to be rebuild
	public bool isPhysicDirty = false;
	
	// Atlas generation ID
	public string atlasGenerationID = "";
	
	// Texture import guid
	public string m_oTextureImportGUID = "";
	
	// The vertex color
	[SerializeField]
	[HideInInspector]
	private Color m_fVertexColor;
	public Color VertexColor
	{
		get
		{
			return m_fVertexColor;
		}
		
		set
		{
			if(m_fVertexColor != value)
			{
				m_fVertexColor = value;
				UpdateVertexColor(m_fVertexColor);
			}
		}
	}
	
	// Update Vertex Color
	private void UpdateVertexColor(Color a_oColor)
	{
		if(spriteQuadMesh != null)
		{
			Color[] oColors = spriteQuadMesh.colors;
			
			for(int i = 0; i<oColors.Length; i++)
			{
				oColors[i] = a_oColor;
			}
			
			spriteQuadMesh.colors = oColors;
		}
	}
	
#if UNITY_EDITOR
	
	// The inspector
	private Uni2DEditorSpriteMeshInspectorParameters m_rSpriteMeshInspectorParameter = new Uni2DEditorSpriteMeshInspectorParameters();
	
	// Creation in progress	
	private static bool ms_bCreationInProgress;
	
	// Creation in progress	
	public static bool prefabUpdateInProgress;
	
	// The start frame
	private const int mc_iAwakeFrame = 2;
	
	// The inspector
	public Uni2DEditorSpriteMeshInspectorParameters SpriteMeshInspectorParameter
	{
		get
		{
			return m_rSpriteMeshInspectorParameter;
		}
	}
	
	// Create
	public static Uni2DSprite Create(GameObject a_oSpriteGameObject)
	{
		ms_bCreationInProgress = true;
		Uni2DSprite rSprite = a_oSpriteGameObject.AddComponent<Uni2DSprite>();
		ms_bCreationInProgress = false;
		
		return rSprite;
	}
	
	// Get collider triangle count
	public int GetColliderTriangleCount( )
	{
		int iTriangleCount = 0;

		if( meshCollidersList != null )
		{
			foreach( Mesh rMesh in meshCollidersList )
			{
				if( rMesh != null )
				{
					iTriangleCount += rMesh.triangles.Length;
				}
			}

			iTriangleCount /= 3;
		}

		return iTriangleCount;
	}
	
	// On a texture change
	public void OnTextureChange(string a_oNewTextureImportGUID)
	{
		//Debug.Log("On texture change");
		
		// Update sprite size
		if(spriteTexture != null)
		{
			string oTexturePath = AssetDatabase.GetAssetPath( spriteTexture.GetInstanceID( ) );
			TextureImporter rTextureImporter = TextureImporter.GetAtPath( oTexturePath ) as TextureImporter;
			if(rTextureImporter != null)
			{	
				TextureImporterSettings oTextureImporterSettings = Uni2DEditorUtilsSpriteBuilder.TextureProcessingBegin(rTextureImporter);
		
				Uni2DEditorUtilsSpriteBuilder.DoUpdateAllSceneSpritesAccordinglyToTextureChange(spriteTexture, a_oNewTextureImportGUID);
				
				Uni2DEditorUtilsSpriteBuilder.TextureProcessingEnd(rTextureImporter, oTextureImporterSettings);
		
				EditorUtility.UnloadUnusedAssets( );
			}
		}
	}
	
	// Update for a texture change
	public void UpdateAccordinglyToTextureChange(string a_oNewTextureImportGUID)
	{
		if(m_oTextureImportGUID == a_oNewTextureImportGUID)
		{
			return;
		}
		
		// Update sprite size
		float fSpriteTextureWidthNew = spriteTexture.width;
		float fSpriteTextureHeightNew = spriteTexture.height;
		
		if(spriteTextureWidth != 0.0f && spriteTextureHeight != 0.0f)
		{						
			Uni2DEditorUtilsSpriteBuilder.UpdateSpriteSize(this, fSpriteTextureWidthNew, fSpriteTextureHeightNew);
		}
		else
		{
			Uni2DEditorUtilsSpriteBuilder.UpdateQuadMeshSize(this, fSpriteTextureWidthNew, fSpriteTextureHeightNew);
		}
		
		spriteTextureWidth = fSpriteTextureWidthNew;
		spriteTextureHeight = fSpriteTextureHeightNew;
		
		// Texture change end
		m_oTextureImportGUID = a_oNewTextureImportGUID;
		if(physicMode != PhysicMode.NoPhysic)
		{
			isPhysicDirty = true;
		}
		
		EditorUtility.SetDirty(this);
	}
	
	// Rebuild
	public void Rebuild()
	{
		if(EditorUtility.IsPersistent(this))
		{
			Uni2DEditorUtilsSpriteBuilder.UpdateSpriteInResource(this);
		}
		else
		{
			Uni2DEditorUtilsSpriteBuilder.UpdateSpriteFromTexture(gameObject, spriteTexture, textureAtlas, VertexColor, alphaCutOff, polygonizationAccuracy,
			extrusionDepth, spriteScale, polygonizeHoles, pivotPointCoords, pivotPointType, physicMode, collisionType, isKinematic);
		}
		
		EditorUtility.SetDirty(this);
	}
	
	// Rebuild mesh in a batch
	public void RebuildInABatch()
	{
		if(EditorUtility.IsPersistent(this))
		{
			Uni2DEditorUtilsSpriteBuilder.UpdateSpriteInResourceInABatch(this);
		}
		else
		{
			Uni2DEditorUtilsSpriteBuilder.DoUpdateSpriteFromTexture(gameObject, spriteTexture, textureAtlas, VertexColor, alphaCutOff, polygonizationAccuracy,
			extrusionDepth, spriteScale, polygonizeHoles, pivotPointCoords, pivotPointType, physicMode, collisionType, isKinematic);
		}
		
		EditorUtility.SetDirty(this);
	}
	
	// Rebuild mesh
	public void UpdateUvs()
	{
		Uni2DEditorUtilsSpriteBuilder.UpdateUVs(this);
		
		EditorUtility.SetDirty(this);
	}
	
	// Save sprite as part of a prefab
	public void SaveSpriteAsPartOfAPrefab(string a_rPrefabResourcesFolderPath)
	{
		if(NeedToBeSaved())
		{
			Rebuild();
		}
		SaveResources(a_rPrefabResourcesFolderPath);
	}
	
	// Need to be saved?
	public bool NeedToBeSaved()
	{
		return spriteQuadMesh == null
			|| spriteQuadMaterial == null
			|| 
			(meshCollidersList == null || meshCollidersList.Contains(null))
			||
			(meshColliderComponentsList == null || meshColliderComponentsList.Contains(null));
	}
	
	// Before prefab post process
	public bool BeforePrefabPostProcess()
	{
		if(NeedToBeSaved())
		{
			return true;
		}
		else
		{
			// Check if all the sprite resources belong to the good folder
			if(IsResourcesInTheCorrectFolder() == false)
			{
				// Nullify the quad mesh to force to resave the prefab
				spriteQuadMesh = null;
				
				return true;
			}
		}
		
		return false;
	}
	
	// Before prefab post process
	private bool IsResourcesInTheCorrectFolder()
	{
		string oResourceDirectory = Uni2DEditorUtilsSpriteBuilder.GetPrefabResourcesDirectoryPathLocal(this.gameObject);
		if(AssetDatabase.GetAssetPath(spriteQuadMesh).Contains(oResourceDirectory) == false)
		{
			return false;	
		}
		
		if(AssetDatabase.GetAssetPath(spriteQuadMaterial).Contains(oResourceDirectory) == false)
		{
			return false;	
		}
		
		foreach(Mesh rMeshCollider in meshCollidersList)
		{
			if(AssetDatabase.GetAssetPath(rMeshCollider).Contains(oResourceDirectory) == false)
			{
				return false;	
			}
		}
		
		return true;
	}
	
	// Has shared resources
	public bool HasSharedResources()
	{
		bool bMeshCollidersContainAtLeastOneResource = false;
		if(meshCollidersList != null)
		{
			foreach(Mesh rMesh in meshCollidersList)
			{
				if(EditorUtility.IsPersistent(rMesh))
				{
					bMeshCollidersContainAtLeastOneResource = true;
					break;
				}
			}
		}
		
		return EditorUtility.IsPersistent(spriteQuadMesh) || EditorUtility.IsPersistent(spriteQuadMaterial) || bMeshCollidersContainAtLeastOneResource;
	}
	
	// After build
	public void AfterBuild()
	{
		m_oTextureImportGUID = Uni2DEditorUtils.GetTextureImportGUID(spriteTexture);
	}
	
	// Awake
	private void Awake()
	{
		if(prefabUpdateInProgress == false)
		{
			// Is a prefab instance
			if(IsADuplicate())
			{
				//Debug.Log("Duplicate");
				DuplicateResources();
			}
		}
	}
	
	// Update
	private void Update()
	{
		if(Application.isPlaying == false)
		{
			CheckIfTextureChange();
			
			if(PrefabUtility.GetPrefabObject(gameObject) != null)
			{
				BreakResourcesConnection();
			}
			else if(NeedToBeRebuild())
			{	
				//Debug.Log("Rebuild");
				Rebuild();
			}
			else if(HasSharedResources())
			{
				//Debug.Log("Break prefab resources connection");
				Rebuild();
			}
			else if(HasAtlasBeenRegenerated())
			{
				//Debug.Log("Update UVs");
				UpdateUvs();
			}
			else if(IsAtlasValid() == false)
			{
				//Debug.Log("Atlas invalid");
				SetDefaultAtlas();
				UpdateUvs();
			}
		}
	}
	
	// Check if dirty
	private void CheckIfTextureChange()
	{
		if(spriteTexture != null)
		{
	 		string oTextureImportGUID = Uni2DEditorUtils.GetTextureImportGUID(spriteTexture);
			if(oTextureImportGUID != m_oTextureImportGUID)
			{				
				// On texture change
				OnTextureChange(oTextureImportGUID);
			}
		}
	}
	
	// Is a duplicate
	private bool IsADuplicate()
	{
		return ms_bCreationInProgress == false && Time.renderedFrameCount > mc_iAwakeFrame;
	}
	
	// Save Mesh resources
	private void BreakResourcesConnection()
	{
		//Debug.Log("Break Resources");
		DuplicateResources();
		PrefabUtility.DisconnectPrefabInstance(gameObject);
	}
	
	// Save Mesh resources
	private void DuplicateResources()
	{
		// Quad mesh
		if(spriteQuadMesh != null)
		{
			spriteQuadMesh = DuplicateMesh(spriteQuadMesh);
			MeshFilter rMeshFilter = GetComponent<MeshFilter>();
			if(rMeshFilter != null)
			{
				rMeshFilter.mesh = spriteQuadMesh;
			}
		}
		
		// Quad material
		if(spriteQuadMaterial != null)
		{
			Material rNewMaterial = DuplicateMaterial(spriteQuadMaterial);
			if(renderer != null)
			{
				ReplaceMaterial(renderer, spriteQuadMaterial, rNewMaterial);
			}
			spriteQuadMaterial = rNewMaterial;
		}
		
		// Mesh collider(s)
		for(int iMeshIndex = 0; iMeshIndex < meshCollidersList.Count; ++iMeshIndex)
		{
			Mesh rDuplicatedMesh = DuplicateMesh(meshCollidersList[iMeshIndex]);
			MeshCollider rMeshCollider = meshColliderComponentsList[iMeshIndex];
			if(rMeshCollider != null)
			{
				rMeshCollider.sharedMesh = rDuplicatedMesh;
			}
			meshCollidersList[iMeshIndex] = rDuplicatedMesh;
		}
		
		EditorUtility.SetDirty(this);
	}
	
	// Duplicate mesh 
	private Mesh DuplicateMesh(Mesh a_rMeshToDuplicate)
	{
		if(a_rMeshToDuplicate == null)
		{
			return null;
		}
		
		Mesh rNewMesh = new Mesh();
		
		rNewMesh.vertices = a_rMeshToDuplicate.vertices;
		rNewMesh.triangles = a_rMeshToDuplicate.triangles;
		rNewMesh.uv = a_rMeshToDuplicate.uv;
		rNewMesh.uv2 = a_rMeshToDuplicate.uv2;
		rNewMesh.colors = a_rMeshToDuplicate.colors;
		rNewMesh.tangents = a_rMeshToDuplicate.tangents;
		rNewMesh.normals = a_rMeshToDuplicate.normals;
		rNewMesh.name = a_rMeshToDuplicate.name;
		
		rNewMesh.RecalculateBounds( );
		rNewMesh.RecalculateNormals( );
		rNewMesh.Optimize( );
		
		return rNewMesh;
	}
	
	// Duplicate material 
	private Material DuplicateMaterial(Material a_rMaterialToDuplicate)
	{			
		if(a_rMaterialToDuplicate == null)
		{
			return null;
		}
		
		Material rNewMaterial = new Material(a_rMaterialToDuplicate);
		
		return rNewMaterial;
	}
	
	// Duplicate material 
	private void ReplaceMaterial(Renderer a_rRenderer, Material a_rMaterialToReplace, Material a_rNewMaterial)
	{			
		int iMaterialIndex = 0;
		Material[] rMaterials = a_rRenderer.sharedMaterials;
		foreach(Material rMaterial in rMaterials)
		{
			if(rMaterial == a_rMaterialToReplace)
			{
				rMaterials[iMaterialIndex] = a_rNewMaterial;
			}
			iMaterialIndex++;
		}
		a_rRenderer.sharedMaterials = rMaterials;
	}
	
	// Save Mesh resources
	private void SaveResources(string a_rPrefabResourcesFolderPath)
	{
		Mesh rSpriteQuadMesh = spriteQuadMesh;
		Material rSpriteQuadMaterial = spriteQuadMaterial;
		
		// Import assets to database
		AssetDatabase.StartAssetEditing();
		string oGameObjectName = gameObject.name.Replace("(Clone)", "");
		string oPrefabResourcesSubFolderName = oGameObjectName;
		
		// Create folder
		string oPrefabResourcesSubFolderPathGUID = AssetDatabase.CreateFolder(a_rPrefabResourcesFolderPath, oPrefabResourcesSubFolderName);
		string oPrefabResourcesSubFolderPath = AssetDatabase.GUIDToAssetPath(oPrefabResourcesSubFolderPathGUID) + "/";
		
		// Quad mesh
		string oSpriteQuadMeshAssetPath = oPrefabResourcesSubFolderPath + rSpriteQuadMesh.name + ".asset";
		AssetDatabase.CreateAsset( rSpriteQuadMesh, oSpriteQuadMeshAssetPath);
		AssetDatabase.ImportAsset( oSpriteQuadMeshAssetPath );

		// Quad material
		string oSpriteQuadMeshMaterialAssetPath = oPrefabResourcesSubFolderPath + rSpriteQuadMaterial.name + ".mat";
		AssetDatabase.CreateAsset( rSpriteQuadMaterial, oSpriteQuadMeshMaterialAssetPath );
		AssetDatabase.ImportAsset( oSpriteQuadMeshMaterialAssetPath );

		// Mesh collider(s)
		string oMeshColliderAssetPath = oPrefabResourcesSubFolderPath + "mesh_SpriteCollider" + oGameObjectName + ".asset";
		if(meshCollidersList != null && meshCollidersList.Count > 0)
		{
			AssetDatabase.CreateAsset( meshCollidersList[0], oMeshColliderAssetPath );
			for( int iMeshIndex = 1; iMeshIndex < meshCollidersList.Count; ++iMeshIndex )
			{
				Mesh rMeshCollider = meshCollidersList[ iMeshIndex ];
				rMeshCollider.name = "mesh_SpriteCollider" + oGameObjectName + "_" + iMeshIndex;
				AssetDatabase.AddObjectToAsset( rMeshCollider, meshCollidersList[ 0 ] );
				AssetDatabase.ImportAsset( AssetDatabase.GetAssetPath( rMeshCollider ) );
			}
		}

		AssetDatabase.StopAssetEditing( );
	}
	
	// Reset 
	private void Reset()
	{
		SpriteMeshInspectorParameter.OnSpriteReset(this);
		
		EditorUtility.SetDirty(this);
	}
	
	// Need to be rebuild?
	private bool NeedToBeRebuild()
	{
		return spriteQuadMesh == null
			|| spriteQuadMaterial == null
			|| ( physicMode != PhysicMode.NoPhysic
				 && (meshCollidersList == null || meshCollidersList.Contains(null) || meshColliderComponentsList == null || meshColliderComponentsList.Contains(null))
				);
	}
	
	// Has atlas been regenerated ?
	public bool HasAtlasBeenRegenerated()
	{
		if(textureAtlas == null)
		{
			return false;
		}
		else
		{
			return atlasGenerationID != textureAtlas.generationId;
		}
	}
	
	// Need to be rebuild?
	private bool IsAtlasValid()
	{
		return textureAtlas == null || textureAtlas.Contains(spriteTexture);
	}
	
	// Set default atlas
	private void SetDefaultAtlas()
	{
		textureAtlas = Uni2DEditorUtils.FindFirstTextureAtlas(spriteTexture);
	}
	
#endif
}