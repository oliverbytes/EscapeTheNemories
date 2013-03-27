#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditorInternal;

using Uni2DTextureImporterSettingsPair = System.Collections.Generic.KeyValuePair<UnityEngine.Texture2D, UnityEditor.TextureImporterSettings>;

/**
 * Static util class to create a mesh collider from the edges
 * of a sprite (texture2D)
 */
public static class Uni2DEditorUtilsSpriteBuilder 
{
	public const string mc_oSpriteDefaultShader = "Particles/Alpha Blended";
	
	// Editor window GUI
	private const string mc_oGUILabelScale            = "Sprite Scale";
	private const string mc_oGUILabelPivot            = "Pivot";
	private const string mc_oGUILabelCustomPivotPoint = "Custom pivot point";
	private const string mc_oGUILabelCollider         = "Advanced Collider Settings";
	private const string mc_oGUILabelAlphaCutOff      = "Alpha Cut Off";
	private const string mc_oGUILabelAccuracy         = "Edge simplicity";
	private const string mc_oGUILabelExtrusion        = "Extrusion Depth";
	private const string mc_oGUILabelHoles            = "Polygonize Holes";

	// Parameter
	private const float mc_fToUnit = 0.01f;
	private const float mc_fSpriteScaleMin = 1e-1f;
	private const float mc_fSpriteExtrusionMin = 1e-1f;
	
	// Undo
	private static bool ms_bUndoEnabled = true;
	
	// Parse a 2D texture and make mesh collider from edges
	public static GameObject CreateSpriteFromTexture( 
		Texture2D a_rTextureToPolygonize,
		Color a_oVertexColor,
		float a_fAlphaCutOff,
		bool a_bPolygonizeHoles,
		float a_fPolygonizationAccuracy,
		float a_fExtrusionDepth,
		float a_fScale,
		Vector2 a_f2CustomPivotPoint,
		Uni2DSprite.PivotPointType a_ePivotPoint,
		Uni2DSprite.PhysicMode a_ePhysicMode,
		Uni2DSprite.CollisionType a_eCollisionType,
		bool a_bIsKinematic
		)
	{
		GameObject oSpriteMeshGameObject = null;
			
		string oTexturePath = AssetDatabase.GetAssetPath(a_rTextureToPolygonize);
		TextureImporter rTextureImporter = TextureImporter.GetAtPath(oTexturePath) as TextureImporter;
		if(rTextureImporter != null)
		{			
			TextureImporterSettings oTextureImporterSettings = Uni2DEditorUtilsSpriteBuilder.TextureProcessingBegin(rTextureImporter);
			
			// Compute pivot point
			Vector2 f2PivotPoint = ComputePivotPoint( a_rTextureToPolygonize, a_ePivotPoint, a_f2CustomPivotPoint );
	
			// Create mesh from texture
			List<Mesh> oMeshesList = null;
	
			if(a_ePhysicMode != Uni2DSprite.PhysicMode.NoPhysic)
			{
				oMeshesList = CreateMeshFromTexture(
					a_rTextureToPolygonize,
					a_fAlphaCutOff,
					a_fPolygonizationAccuracy,
					a_fExtrusionDepth,
					a_fScale,
					a_bPolygonizeHoles,
					f2PivotPoint,
					a_ePhysicMode,
					a_eCollisionType, 
					a_bIsKinematic);
			}
	
			// Create sprite mesh game object
			oSpriteMeshGameObject = CreateSpriteGameObject(
				a_rTextureToPolygonize,
				a_oVertexColor,
				oMeshesList,
				a_fPolygonizationAccuracy,
				a_fAlphaCutOff,
				a_fExtrusionDepth,
				a_fScale,
				a_bPolygonizeHoles,
				f2PivotPoint,
				a_ePivotPoint,
				a_ePhysicMode,
				a_eCollisionType, 
				a_bIsKinematic);
			
			Uni2DEditorUtilsSpriteBuilder.TextureProcessingEnd(rTextureImporter, oTextureImporterSettings);
		}
		
		if(oSpriteMeshGameObject != null)
		{
			if(ms_bUndoEnabled)
			{
				// Allow undo the creation
				Undo.RegisterCreatedObjectUndo(oSpriteMeshGameObject, "Create sprite");
			}
		}

		return oSpriteMeshGameObject;
	}

	private static GameObject CreateSpriteGameObject(
		Texture2D a_rSpriteTexture,
		Color a_oVertexColor,
		List<Mesh> a_rMeshCollidersList,
		float a_fPolygonizationAccuracy,
		float a_fAlphaCutOff,
		float a_fExtrusionDepth,
		float a_fScale,
		bool a_bPolygonizeHoles,
		Vector2 a_f2PivotPoint,
		Uni2DSprite.PivotPointType a_ePivotPoint,
		Uni2DSprite.PhysicMode a_ePhysicMode,
		Uni2DSprite.CollisionType a_eCollisionType,
		bool a_bIsKinematic)
	{
		
		float fScale = a_fScale * mc_fToUnit;

		// Game object + components creation
		GameObject oSpriteQuadGameObject = new GameObject( "Sprite_" + a_rSpriteTexture.name );
		MeshFilter oSpriteQuadMeshFilterComponent = oSpriteQuadGameObject.AddComponent<MeshFilter>( );
		MeshRenderer oSpriteQuadMeshRendererComponent = oSpriteQuadGameObject.AddComponent<MeshRenderer>( );

		// Scale texture width/height
		float fScaledWidth  = fScale * a_rSpriteTexture.width;
		float fScaledHeight = fScale * a_rSpriteTexture.height;

		// (Scaled) Quad mesh creation
		Mesh rSpriteQuadMesh = CreateTexturedQuadMesh( fScaledWidth, fScaledHeight, a_f2PivotPoint * fScale, a_oVertexColor);
		rSpriteQuadMesh.name = "mesh_SpriteQuad" + a_rSpriteTexture.name;

		// Quad mesh material creation
		Material oSpriteQuadMeshMaterial = new Material( Shader.Find( mc_oSpriteDefaultShader ) );
		oSpriteQuadMeshMaterial.name = "mat_SpriteQuad" + a_rSpriteTexture.name;
		oSpriteQuadMeshMaterial.mainTexture = a_rSpriteTexture;

		// Sprite components init.
		oSpriteQuadMeshFilterComponent.sharedMesh = rSpriteQuadMesh;
		oSpriteQuadMeshRendererComponent.sharedMaterial = oSpriteQuadMeshMaterial;

		List<MeshCollider> oMeshColliderComponentsList = new List<MeshCollider>( );

		// Add collider children
		// Attach a mesh collider collider to current game object
		// if collider is not compound
		GameObject oColliderParentGameObject = null;

		// Components creation
		if(a_ePhysicMode != Uni2DSprite.PhysicMode.NoPhysic)
		{
			if( a_eCollisionType != Uni2DSprite.CollisionType.Compound)
			{
				MeshCollider oMeshColliderComponent = oSpriteQuadGameObject.AddComponent<MeshCollider>( );
				oMeshColliderComponent.sharedMesh = a_rMeshCollidersList[ 0 ];
				oMeshColliderComponentsList.Add( oMeshColliderComponent );
	
				// Set whether or not mesh collider is convex
				if(a_eCollisionType == Uni2DSprite.CollisionType.Concave)
				{
					oMeshColliderComponent.convex = false;
				}
				else
				{
					oMeshColliderComponent.convex = true;
				}
			}
			else // Dynamic Compound mode
			{
				oColliderParentGameObject = new GameObject( "root_Colliders" );
	
				// Create a game object for each mesh collider and attach them to sprite game object
				for( int iColliderIndex = 0; iColliderIndex < a_rMeshCollidersList.Count; ++iColliderIndex )
				{
					GameObject oMeshColliderGameObject = new GameObject( "mesh_Collider" + iColliderIndex );
					MeshCollider oMeshColliderComponent = oMeshColliderGameObject.AddComponent<MeshCollider>( );
					oMeshColliderComponent.sharedMesh = a_rMeshCollidersList[ iColliderIndex ];
					oMeshColliderComponent.convex = true;
	
					oMeshColliderComponentsList.Add( oMeshColliderComponent );
	
					// Child -> parent attachment
					oMeshColliderGameObject.transform.parent = oColliderParentGameObject.transform;
				}
	
				oColliderParentGameObject.transform.parent = oSpriteQuadGameObject.transform;
				oColliderParentGameObject.transform.localPosition = Vector3.zero;
				oColliderParentGameObject.transform.localRotation = Quaternion.identity;
			}

			// Add rigidbody to sprite game object if any dynamic mode is specified
			if(a_ePhysicMode != Uni2DSprite.PhysicMode.Static)
			{
				SetupRigidbodyFor2D(oSpriteQuadGameObject.AddComponent<Rigidbody>(), a_bIsKinematic);
			}
		}

		// Sprite component init.
		Uni2DSprite oSpriteMeshComponent 				 = Uni2DSprite.Create(oSpriteQuadGameObject);
		oSpriteMeshComponent.spriteTexture               = a_rSpriteTexture;
		oSpriteMeshComponent.textureAtlas                = Uni2DEditorUtils.FindFirstTextureAtlas(a_rSpriteTexture);
		oSpriteMeshComponent.VertexColor				 = a_oVertexColor;
		oSpriteMeshComponent.alphaCutOff                 = a_fAlphaCutOff;
		oSpriteMeshComponent.polygonizationAccuracy      = a_fPolygonizationAccuracy;
		oSpriteMeshComponent.polygonizeHoles             = a_bPolygonizeHoles;
		oSpriteMeshComponent.meshCollidersList           = a_rMeshCollidersList;
		oSpriteMeshComponent.meshColliderComponentsList  = oMeshColliderComponentsList;
		oSpriteMeshComponent.meshCollidersRootGameObject = oColliderParentGameObject;
		oSpriteMeshComponent.spriteScale                 = a_fScale;
		oSpriteMeshComponent.extrusionDepth              = a_fExtrusionDepth;
		oSpriteMeshComponent.spriteQuadMesh              = rSpriteQuadMesh;
		oSpriteMeshComponent.spriteQuadMaterial          = oSpriteQuadMeshMaterial;
		oSpriteMeshComponent.physicMode    				 = a_ePhysicMode;
		oSpriteMeshComponent.collisionType    			 = a_eCollisionType;
		oSpriteMeshComponent.isKinematic    			 = a_bIsKinematic;
		oSpriteMeshComponent.spriteTextureWidth          = a_rSpriteTexture.width;
		oSpriteMeshComponent.spriteTextureHeight         = a_rSpriteTexture.height;
		oSpriteMeshComponent.pivotPointType = a_ePivotPoint;
		oSpriteMeshComponent.pivotPointCoords = a_f2PivotPoint;

		oSpriteMeshComponent.isPhysicDirty                     	= false;

		
		/*SceneView rSceneView = SceneView.lastActiveSceneView;
		if( rSceneView != null )
		{
			oSpriteQuadGameObject.transform.position = rSceneView.pivot;
		}*/
		
		UpdateUVs(oSpriteMeshComponent);
		
		oSpriteMeshComponent.AfterBuild();

		return oSpriteQuadGameObject;
	}

	////////////////////////////////////////////////////////////////////////////////
	public static void UpdateAllDirtySpritesPhysic(List<Uni2DSprite> a_rSprites)
	{
		// Prepare textures for processing
		List<Uni2DSprite> oDirtySprites = new List<Uni2DSprite>();
		HashSet<Texture> oAlreadyPreparedTextures = new HashSet<Texture>();
		List<Uni2DTextureImporterSettingsPair> oTextureImportersSettings = new List<Uni2DTextureImporterSettingsPair>();
		foreach(Uni2DSprite rSprite in a_rSprites)
		{
			if(rSprite != null && rSprite.isPhysicDirty)
			{
				// If we haven't yet prepare this sprite texture
				Texture2D rTexture = rSprite.spriteTexture;
				if(oAlreadyPreparedTextures.Contains(rTexture) == false)
				{
					oAlreadyPreparedTextures.Add(rTexture);
					TextureImporterSettings rTextureImporterSettings = Uni2DEditorUtilsSpriteBuilder.TextureProcessingBegin(rTexture);
					oTextureImportersSettings.Add(new Uni2DTextureImporterSettingsPair(rTexture, rTextureImporterSettings));
				}
				
				// We have to update this sprite
				oDirtySprites.Add(rSprite);
			}
		}
					
		// Loop through all the dirty sprites and update them
		foreach(Uni2DSprite rSprite in oDirtySprites)
		{
			rSprite.RebuildInABatch();
		}

		// Restore textures settings
		foreach(Uni2DTextureImporterSettingsPair rTextureImporterSettings in oTextureImportersSettings)
		{
			Uni2DEditorSpriteAssetPostProcessor.Enabled = false;
			Uni2DEditorUtilsSpriteBuilder.TextureProcessingEnd(rTextureImporterSettings.Key, rTextureImporterSettings.Value);
		}
		Uni2DEditorSpriteAssetPostProcessor.Enabled = true;
	}
	
	
	public static void UpdateAllSceneSpritesOfTexture(Texture2D a_rTextureToPolygonize)
	{
		if(a_rTextureToPolygonize == null)
		{
			return;
		}
		
		string oTexturePath = AssetDatabase.GetAssetPath(a_rTextureToPolygonize);
		TextureImporter rTextureImporter = TextureImporter.GetAtPath(oTexturePath) as TextureImporter;
		if(rTextureImporter != null)
		{	
			TextureImporterSettings oTextureImporterSettings = Uni2DEditorUtilsSpriteBuilder.TextureProcessingBegin(rTextureImporter);
		
			DoUpdateAllSceneSpritesOfTexture(a_rTextureToPolygonize);
			
			Uni2DEditorUtilsSpriteBuilder.TextureProcessingEnd(rTextureImporter, oTextureImporterSettings);
			EditorUtility.UnloadUnusedAssets( );
		}
	}
	
	public static void DoUpdateAllSceneSpritesOfTexture(Texture2D a_rTexture)
	{
		// Loop through all the scene sprites
		foreach(Uni2DSprite rSprite in MonoBehaviour.FindObjectsOfType(typeof(Uni2DSprite)))
		{
			if(rSprite.spriteTexture == a_rTexture)
			{
				rSprite.RebuildInABatch();
			}
		}
	}
	
	public static GameObject UpdateSpriteFromTexture(
		GameObject a_rOutdatedSpriteMeshGameObject,
		Texture2D a_rTextureToPolygonize,
		Uni2DTextureAtlas a_rTextureAtlas,
		Color a_oVertexColor,
		float a_fAlphaCutOff,
		float a_fPoligonizationAccuracy,
		float a_fExtrusionDepth,
		float a_fScale,
		bool a_bPolygonizeHoles,
		Vector2 a_f2CustomPivotPoint,
		Uni2DSprite.PivotPointType a_ePivotPoint,
		Uni2DSprite.PhysicMode a_ePhysicMode,
		Uni2DSprite.CollisionType a_eCollisionType,
		bool a_bIsKinematic)
	{
		if(a_rTextureToPolygonize == null)
		{
			DisplayNoTextureWarning(a_rOutdatedSpriteMeshGameObject);
			return a_rOutdatedSpriteMeshGameObject;
		}
		
		string oTexturePath = AssetDatabase.GetAssetPath(a_rTextureToPolygonize);
		TextureImporter rTextureImporter = TextureImporter.GetAtPath(oTexturePath) as TextureImporter;
		if(rTextureImporter != null)
		{	
			TextureImporterSettings oTextureImporterSettings = Uni2DEditorUtilsSpriteBuilder.TextureProcessingBegin(rTextureImporter);
		
			GameObject oUpdatedSpriteMeshGameObject = DoUpdateSpriteFromTexture(	a_rOutdatedSpriteMeshGameObject, 
																		a_rTextureToPolygonize,
																		a_rTextureAtlas,
																		a_oVertexColor,
																		a_fAlphaCutOff,
																		a_fPoligonizationAccuracy,
																		a_fExtrusionDepth,
																		a_fScale,
																		a_bPolygonizeHoles,
																		a_f2CustomPivotPoint,
																		a_ePivotPoint,
																		a_ePhysicMode,
																		a_eCollisionType, 
																		a_bIsKinematic
																	);
			
			Uni2DEditorUtilsSpriteBuilder.TextureProcessingEnd(rTextureImporter, oTextureImporterSettings);
			EditorUtility.UnloadUnusedAssets( );
			
			return oUpdatedSpriteMeshGameObject;
		}
		else
		{
			return null;
		}
	}
	
	public static GameObject DoUpdateSpriteFromTexture(
		GameObject a_rOutdatedSpriteMeshGameObject,
		Texture2D a_rTextureToPolygonize,
		Uni2DTextureAtlas a_rTextureAtlas,
		Color a_oVertexColor,
		float a_fAlphaCutOff,
		float a_fPoligonizationAccuracy,
		float a_fExtrusionDepth,
		float a_fScale,
		bool a_bPolygonizeHoles,
		Vector2 a_f2CustomPivotPoint,
		Uni2DSprite.PivotPointType a_ePivotPoint,
		Uni2DSprite.PhysicMode a_ePhysicMode,
		Uni2DSprite.CollisionType a_eCollisionType,
		bool a_bIsKinematic)
	{	
		if(ms_bUndoEnabled)
		{
			// Allow undo this update
			Undo.RegisterSceneUndo("Update Sprite");
		}
		
		Uni2DSprite rOutdatedSpriteMeshComponent = a_rOutdatedSpriteMeshGameObject.GetComponent<Uni2DSprite>();

		if(rOutdatedSpriteMeshComponent != null)
		{
			// Delete resources
			// Delete game objects in excess
			foreach(MeshCollider rMeshColliderComponent in rOutdatedSpriteMeshComponent.meshColliderComponentsList)
			{
				if(rMeshColliderComponent != null)
				{
					// If the mesh collider component is attached to another game object
					// than the sprite itself, we destroy it too (because that component was its unique purpose)
					if(rMeshColliderComponent.gameObject != a_rOutdatedSpriteMeshGameObject)
					{
						// Destroy game object AND component
						GameObject.DestroyImmediate(rMeshColliderComponent.gameObject);
					}
					else
					{
						// Destroy component
						MonoBehaviour.DestroyImmediate(rMeshColliderComponent);
					}
				}
			}

			// Destroy mesh collider root game object (if any)
			if(rOutdatedSpriteMeshComponent.meshCollidersRootGameObject != null)
			{
				GameObject.DestroyImmediate(rOutdatedSpriteMeshComponent.meshCollidersRootGameObject);
			}

			// Delete components in excess
			Rigidbody rRigidbodyComponent = rOutdatedSpriteMeshComponent.GetComponent<Rigidbody>();
			if(rRigidbodyComponent != null)
			{
				// Destroy component
				MonoBehaviour.DestroyImmediate(rRigidbodyComponent);
			}
		}

		Vector2 f2PivotPoint = ComputePivotPoint(a_rTextureToPolygonize, a_ePivotPoint, a_f2CustomPivotPoint);

		List<Mesh> oMeshesList = null;
		if(a_ePhysicMode != Uni2DSprite.PhysicMode.NoPhysic)
		{
			// Rebuild mesh collider
			oMeshesList = CreateMeshFromTexture(
				a_rTextureToPolygonize,
				a_fAlphaCutOff,
				a_fPoligonizationAccuracy,
				a_fExtrusionDepth,
				a_fScale,
				a_bPolygonizeHoles,
				f2PivotPoint,
				a_ePhysicMode,
				a_eCollisionType,
				a_bIsKinematic);
		}

		// Update game object
		GameObject oUpdatedSpriteMeshGameObject = UpdateSpriteGameObject(
			a_rOutdatedSpriteMeshGameObject,
			a_rTextureToPolygonize,
			a_rTextureAtlas,
			a_oVertexColor,
			oMeshesList,
			a_fAlphaCutOff,
			a_fPoligonizationAccuracy,
			a_fExtrusionDepth,
			a_fScale,
			a_bPolygonizeHoles,
			f2PivotPoint,
			a_ePivotPoint,
			a_ePhysicMode,
			a_eCollisionType,
			a_bIsKinematic);
		
		return oUpdatedSpriteMeshGameObject;
	}

	private static GameObject UpdateSpriteGameObject(
		GameObject a_rOutdatedSpriteGameObject,
		Texture2D a_rSpriteTexture,
		Uni2DTextureAtlas a_rTextureAtlas,
		Color a_oVertexColor,
		List<Mesh> a_rMeshCollidersList,
		float a_fAlphaCutOff,
		float a_fPolygonizationAccuracy,
		float a_fExtrusionDepth,
		float a_fScale,
		bool a_bPolygonizeHoles,
		Vector2 a_f2PivotPoint,
		Uni2DSprite.PivotPointType a_ePivotPoint,
		Uni2DSprite.PhysicMode a_ePhysicMode,
		Uni2DSprite.CollisionType a_eCollisionType,
		bool a_bIsKinematic )
	{
		//Debug.Log("UpdateSpriteGameObject");
		
		float fScale = a_fScale * mc_fToUnit;
		if(a_rSpriteTexture == null)
		{
			return a_rOutdatedSpriteGameObject;
		}
		
		// Sprite component
		Uni2DSprite rSpriteMeshComponent = a_rOutdatedSpriteGameObject.GetComponent<Uni2DSprite>( );
		if( rSpriteMeshComponent == null )
		{
			rSpriteMeshComponent = Uni2DSprite.Create(a_rOutdatedSpriteGameObject);
		}

		MeshFilter rSpriteQuadMeshFilterComponent = a_rOutdatedSpriteGameObject.GetComponent<MeshFilter>( );
		if( rSpriteQuadMeshFilterComponent == null )
		{
			rSpriteQuadMeshFilterComponent = a_rOutdatedSpriteGameObject.AddComponent<MeshFilter>( );
		}
		
		MeshRenderer rSpriteQuadMeshRendererComponent = a_rOutdatedSpriteGameObject.GetComponent<MeshRenderer>( );
		if( rSpriteQuadMeshRendererComponent == null )
		{
			rSpriteQuadMeshRendererComponent = a_rOutdatedSpriteGameObject.AddComponent<MeshRenderer>( );
		}

		Vector2 f2PivotPoint = ComputePivotPoint( a_rSpriteTexture, a_ePivotPoint, a_f2PivotPoint );

		// Scale texture width/height
		float fScaledWidth  = fScale * a_rSpriteTexture.width;
		float fScaledHeight = fScale * a_rSpriteTexture.height;

		// (Scaled) Quad mesh creation
		Mesh oSpriteQuadMesh = CreateTexturedQuadMesh(fScaledWidth, fScaledHeight, fScale * f2PivotPoint, rSpriteMeshComponent.VertexColor);
		oSpriteQuadMesh.name = "mesh_SpriteQuad" + a_rSpriteTexture.name;
		
		Material oSpriteRendererMaterial;
		Material oSpriteQuadMeshMaterial;
		
		if(rSpriteMeshComponent.spriteQuadMaterial != null)
		{
			oSpriteQuadMeshMaterial = new Material(rSpriteMeshComponent.spriteQuadMaterial);
		}
		else
		{
			// Quad mesh material creation
			oSpriteQuadMeshMaterial = new Material( Shader.Find( mc_oSpriteDefaultShader ) );
		}
		oSpriteQuadMeshMaterial.name = "mat_SpriteQuad" + a_rSpriteTexture.name;
		oSpriteQuadMeshMaterial.mainTexture = a_rSpriteTexture;
		
		if(a_rTextureAtlas == null)
		{
			oSpriteRendererMaterial = oSpriteQuadMeshMaterial;
		}
		else
		{
			oSpriteRendererMaterial = a_rTextureAtlas.atlasMaterial;
		}
		

		// Sprite components init.
		rSpriteQuadMeshFilterComponent.sharedMesh = oSpriteQuadMesh;
		rSpriteQuadMeshRendererComponent.sharedMaterial = oSpriteRendererMaterial;

		List<MeshCollider> oMeshColliderComponentsList = new List<MeshCollider>( );
		GameObject rColliderParentGameObject = null;

		// Add collider children
		// Attach a mesh collider collider to current game object
		// if collider is not compound
		if(a_ePhysicMode != Uni2DSprite.PhysicMode.NoPhysic)
		{
			if(a_eCollisionType != Uni2DSprite.CollisionType.Compound)
			{
				MeshCollider rMeshColliderComponent = a_rOutdatedSpriteGameObject.GetComponent<MeshCollider>( );
				if( rMeshColliderComponent == null )
				{
					rMeshColliderComponent = a_rOutdatedSpriteGameObject.AddComponent<MeshCollider>( );
				}
	
				rMeshColliderComponent.sharedMesh = a_rMeshCollidersList[ 0 ];
				oMeshColliderComponentsList.Add( rMeshColliderComponent );
	
				// Set whether or not mesh collider is convex
				if(a_eCollisionType == Uni2DSprite.CollisionType.Concave)
				{
					rMeshColliderComponent.convex = false;
				}
				else
				{
					rMeshColliderComponent.convex = true;
				}
			}
			else // Dynamic Compound mode
			{
				rColliderParentGameObject = rSpriteMeshComponent.meshCollidersRootGameObject;
				if( rColliderParentGameObject == null )
				{
					rColliderParentGameObject = new GameObject( "root_Colliders" );
				}
	
				// Create a game object for each mesh collider and attach them to sprite game object
				for( int iColliderIndex = 0; iColliderIndex < a_rMeshCollidersList.Count; ++iColliderIndex )
				{
					GameObject oMeshColliderGameObject = new GameObject( "mesh_Collider" + iColliderIndex );
					MeshCollider oMeshColliderComponent = oMeshColliderGameObject.AddComponent<MeshCollider>( );
					oMeshColliderComponent.sharedMesh = a_rMeshCollidersList[ iColliderIndex ];
					oMeshColliderComponent.convex = true;
	
					oMeshColliderComponentsList.Add( oMeshColliderComponent );
	
					// Child -> parent attachment
					oMeshColliderGameObject.transform.parent = rColliderParentGameObject.transform;
					oMeshColliderGameObject.transform.localPosition = Vector3.zero;
					oMeshColliderGameObject.transform.localRotation = Quaternion.identity;
					oMeshColliderGameObject.transform.localScale = Vector3.one;
				}
	
				rColliderParentGameObject.transform.parent = a_rOutdatedSpriteGameObject.transform;
				rColliderParentGameObject.transform.localPosition = Vector3.zero;
				rColliderParentGameObject.transform.localRotation = Quaternion.identity;
				rColliderParentGameObject.transform.localScale = Vector3.one;
			}
	
			// Add rigidbody to sprite game object if any dynamic mode is specified
			if(a_ePhysicMode != Uni2DSprite.PhysicMode.Static)
			{
				SetupRigidbodyFor2D(a_rOutdatedSpriteGameObject.AddComponent<Rigidbody>(), a_bIsKinematic);
			}
		}

		// Pivot sprite mesh component init.
		rSpriteMeshComponent.spriteTexture               = a_rSpriteTexture;
		rSpriteMeshComponent.alphaCutOff                 = a_fAlphaCutOff;
		rSpriteMeshComponent.polygonizationAccuracy      = a_fPolygonizationAccuracy;
		rSpriteMeshComponent.extrusionDepth              = a_fExtrusionDepth;
		rSpriteMeshComponent.spriteScale                 = a_fScale;
		rSpriteMeshComponent.polygonizeHoles             = a_bPolygonizeHoles;
		rSpriteMeshComponent.meshCollidersList           = a_rMeshCollidersList;
		rSpriteMeshComponent.meshColliderComponentsList  = oMeshColliderComponentsList;
		rSpriteMeshComponent.meshCollidersRootGameObject = rColliderParentGameObject;
		rSpriteMeshComponent.spriteQuadMesh              = oSpriteQuadMesh;
		rSpriteMeshComponent.spriteQuadMaterial          = oSpriteQuadMeshMaterial;
		rSpriteMeshComponent.textureAtlas          		 = a_rTextureAtlas;
		rSpriteMeshComponent.VertexColor				 = a_oVertexColor;
		rSpriteMeshComponent.physicMode					 = a_ePhysicMode;
		rSpriteMeshComponent.collisionType				 = a_eCollisionType;
		rSpriteMeshComponent.isKinematic				 = a_bIsKinematic;
		rSpriteMeshComponent.spriteTextureWidth          = a_rSpriteTexture.width;
		rSpriteMeshComponent.spriteTextureHeight         = a_rSpriteTexture.height;
		rSpriteMeshComponent.pivotPointType              = a_ePivotPoint;
		rSpriteMeshComponent.pivotPointCoords      		 = f2PivotPoint;
		rSpriteMeshComponent.isPhysicDirty                     	= false;
		
		UpdateUVs(rSpriteMeshComponent);
		
		rSpriteMeshComponent.AfterBuild();

		return a_rOutdatedSpriteGameObject;
	}
	
	////////////////////////////////////////////////////////////////////////////////

	private static List<Mesh> CreateMeshFromTexture(
		Texture2D a_rTextureToPolygonize,
		float a_fAlphaCutOff,
		float a_fPolygonizationAccuracy,
		float a_fExtrusionDepth,
		float a_fScale,
		bool a_bPolygonizeHoles,
		Vector2 a_f2PivotPoint,
		Uni2DSprite.PhysicMode a_ePhysicMode,
		Uni2DSprite.CollisionType a_eCollisionType,
		bool a_bIsKinematic)
	{
		// Polygonize holes?
		bool bPolygonizeHoles = a_eCollisionType != Uni2DSprite.CollisionType.Convex && a_bPolygonizeHoles;
		
		float fScale = a_fScale * mc_fToUnit;
			
		if(a_rTextureToPolygonize == null)
		{
			return null;
		}
				
		// Step 1
		// Distinguish completely transparent pixels from significant pixel by "binarizing" the texture.
		//EditorUtility.DisplayProgressBar( "Creating Mesh From Texture", "Binarizing Texture...", 0.2f );
		BinarizedImage rBinarizedImage = Uni2DEditorUtilsShapeExtraction.BinarizeTexture( a_rTextureToPolygonize, a_fAlphaCutOff );
		
		// Step 2
		// Build binarized outer/inner contours and label image regions
		List<Contour> oOuterContours;
		List<Contour> oInnerContours;
		//EditorUtility.DisplayProgressBar( "Creating Mesh From Texture", "Extracting contours...", 0.4f );
		Uni2DEditorUtilsContourExtraction.CombinedContourLabeling( rBinarizedImage, bPolygonizeHoles, out oOuterContours, out oInnerContours );


		// Step 3: vectorization (determine dominant points)
		if( bPolygonizeHoles == true )
		{
			// Step 3a: if hole support asked by user, merge inner contours into outer contours first
			//EditorUtility.DisplayProgressBar( "Creating Mesh From Texture", "Merging inner and outer contours...", 0.5f );
			oOuterContours = Uni2DEditorUtilsContourPolygonization.MergeInnerAndOuterContours( oOuterContours, oInnerContours );
		}

		// Simplify contours
		//EditorUtility.DisplayProgressBar( "Creating Mesh From Texture", "Simplifying contours...", 0.6f );
		List<Contour> oDominantContoursList = Uni2DEditorUtilsContourPolygonization.SimplifyContours( oOuterContours, a_fPolygonizationAccuracy );

		// Step 4: triangulation
		List<Mesh> oMeshesList;
		//EditorUtility.DisplayProgressBar( "Creating Mesh From Texture", "Triangulating mesh...", 0.8f );
		if(a_ePhysicMode != Uni2DSprite.PhysicMode.NoPhysic && a_eCollisionType == Uni2DSprite.CollisionType.Compound)
		{
			// Compound mesh
			oMeshesList = Uni2DEditorUtilsPolygonTriangulation.EarClippingCompound( oDominantContoursList, a_fExtrusionDepth, fScale, a_f2PivotPoint );
			int iMeshIndex = 0;
            foreach( Mesh rMesh in oMeshesList )
            {
                    rMesh.name = "mesh_Collider" + a_rTextureToPolygonize.name + iMeshIndex;
                    ++iMeshIndex;
            }
		}
		else
		{
			// Single mesh
			Mesh rMesh = Uni2DEditorUtilsPolygonTriangulation.EarClipping( oDominantContoursList, a_fExtrusionDepth, fScale, a_f2PivotPoint );
			rMesh.name = "mesh_Collider" + a_rTextureToPolygonize.name;
			oMeshesList = new List<Mesh>( 1 );
			oMeshesList.Add( rMesh );
		}

		//EditorUtility.DisplayProgressBar( "Creating Mesh From Texture", "Optimizing mesh(es)...", 0.9f );
		// Optimize meshes
		foreach( Mesh rMesh in oMeshesList )
		{
			rMesh.RecalculateBounds( );
			rMesh.RecalculateNormals( );
			rMesh.Optimize( );
		}

		//EditorUtility.DisplayProgressBar( "Creating Mesh From Texture", "Done!", 1.0f );
		//EditorUtility.ClearProgressBar( );

		return oMeshesList;
	}


	////////////////////////////////////////////////////////////////////////////////
	
	private static Mesh CreateTexturedQuadMesh( float a_fWidth, float a_fHeight, Vector3 a_f3PivotPoint, Color a_oVertexColor)
	{
		// Quad mesh
		Mesh oQuadMesh = new Mesh( );

		// Quad vertices
		oQuadMesh.vertices = GenerateQuadVertices(a_fWidth, a_fHeight, a_f3PivotPoint);

		// Quad triangles index (CW)
		oQuadMesh.triangles = new int[ 6 ]
		{
			1, 0, 2,
			2, 0, 3
		};

		// Quad mesh UV coords
		oQuadMesh.uv = new Vector2[ 4 ]
		{
			new Vector2( 0, 0 ),
			new Vector2( 1, 0 ),
			new Vector2( 1, 1 ),
			new Vector2( 0, 1 )
		};
		
		// Quad mesh vertex color
		oQuadMesh.colors = new Color[ 4 ]
		{
			a_oVertexColor,
			a_oVertexColor,
			a_oVertexColor,
			a_oVertexColor
		};

		oQuadMesh.RecalculateBounds( );
		oQuadMesh.RecalculateNormals( );
		oQuadMesh.Optimize( );

		return oQuadMesh;
	}
	
	private static Vector2 ComputePivotPoint(
		Texture2D a_rTexture,
		Uni2DSprite.PivotPointType a_eSpritePivotType,
		Vector2 a_f2CustomPivotPoint )
	{
		if(a_rTexture == null)
		{
			return ComputePivotPoint( 0, 0, a_eSpritePivotType, a_f2CustomPivotPoint );
		}
		else
		{
			return ComputePivotPoint( a_rTexture.width, a_rTexture.height, a_eSpritePivotType, a_f2CustomPivotPoint );
		}
	}
	
	private static Vector2 ComputePivotPoint(
		float a_fWidth,
		float a_fHeight,
		Uni2DSprite.PivotPointType a_eSpritePivotType,
		Vector2 a_f2CustomPivotPoint )
	{
		// Compute new pivot point from pivot type
		switch( a_eSpritePivotType )
		{
			default:
			case Uni2DSprite.PivotPointType.Custom:
			{
				return a_f2CustomPivotPoint;
			}
			
			case Uni2DSprite.PivotPointType.BottomLeft:
			{
				return new Vector2( 0.0f, 0.0f );
			}
			
			case Uni2DSprite.PivotPointType.BottomCenter:
			{
				return new Vector2( a_fWidth * 0.5f, 0.0f );
			}
			
			case Uni2DSprite.PivotPointType.BottomRight:
			{
				return new Vector2( a_fWidth, 0.0f );
			}
			
			case Uni2DSprite.PivotPointType.MiddleLeft:
			{
				return new Vector2( 0.0f, a_fHeight * 0.5f );
			}
			
			case Uni2DSprite.PivotPointType.MiddleCenter:
			{
				return new Vector2( a_fWidth * 0.5f, a_fHeight * 0.5f );
			}
			
			case Uni2DSprite.PivotPointType.MiddleRight:
			{
				return new Vector2( a_fWidth, a_fHeight * 0.5f );
			}
			
			case Uni2DSprite.PivotPointType.TopLeft:
			{
				return new Vector2( 0.0f, a_fHeight );
			}
			
			case Uni2DSprite.PivotPointType.TopCenter:
			{
				return new Vector2( a_fWidth * 0.5f, a_fHeight );
			}
			
			case Uni2DSprite.PivotPointType.TopRight:
			{
				return new Vector2( a_fWidth, a_fHeight );
			}
		}
	}
	
	public static void UpdateSpriteInteractiveParameters(
		Uni2DSprite a_rSpriteComponent,
		float a_fScale,
		float a_fExtrusionDepth,
		Uni2DSprite.PivotPointType a_eSpritePivot,
		Vector2 a_f2CustomPivotPoint,
		Uni2DTextureAtlas a_rTextureAtlas,
		Color a_oVertexColor)
	{
		// Current pivot point
		Vector2 f2CurrentPivotPoint = a_rSpriteComponent.pivotPointCoords;
		float fSpriteScale = a_rSpriteComponent.spriteScale;

		// New pivot to apply
		Vector2 f2NewPivotPoint = ComputePivotPoint(
			a_rSpriteComponent.spriteTextureWidth,
			a_rSpriteComponent.spriteTextureHeight,
			a_eSpritePivot,
			a_f2CustomPivotPoint );

		// The delta to apply
		float fScalingDelta = a_fScale/fSpriteScale;
		float fRealScale = a_fScale * mc_fToUnit;
		Vector3 f3ScaledDeltaPivot = ( f2NewPivotPoint - f2CurrentPivotPoint ) * fRealScale;

		// Apply delta to mesh colliders
		for( int iMeshIndex = 0, iMeshCount = a_rSpriteComponent.meshCollidersList.Count; iMeshIndex < iMeshCount; ++iMeshIndex )
		{
			Mesh rMesh = a_rSpriteComponent.meshCollidersList[ iMeshIndex ];
			Vector3[ ] oMeshVerticesArray = rMesh.vertices;
			for( int iVertexIndex = 0, iVertexCount = rMesh.vertexCount; iVertexIndex < iVertexCount; ++iVertexIndex )
			{
				Vector3 f3Vertex = oMeshVerticesArray[ iVertexIndex ];
				f3Vertex -= f3ScaledDeltaPivot;
				f3Vertex.x *= fScalingDelta;
				f3Vertex.y *= fScalingDelta;
				f3Vertex.z = Mathf.Sign(f3Vertex.z) * a_fExtrusionDepth * 0.5f;
				oMeshVerticesArray[ iVertexIndex ] = f3Vertex;
			}

			// Must set array again ("vertices" getter gives a copy)
			rMesh.vertices = oMeshVerticesArray;

			MeshCollider rMeshCollider = a_rSpriteComponent.meshColliderComponentsList[ iMeshIndex ];
			rMeshCollider.sharedMesh = null;
			rMeshCollider.sharedMesh = rMesh;
		}

		// Apply delta to sprite quad mesh
		Mesh rSpriteQuadMesh = a_rSpriteComponent.spriteQuadMesh;
		Vector3[ ] oSpriteQuadMeshVerticesArray = rSpriteQuadMesh.vertices;
		for( int iVertexIndex = 0, iVertexCount = rSpriteQuadMesh.vertexCount; iVertexIndex < iVertexCount; ++iVertexIndex )
		{
			Vector3 f3Vertex = oSpriteQuadMeshVerticesArray[ iVertexIndex ];
			f3Vertex -= f3ScaledDeltaPivot;
			f3Vertex.x *= fScalingDelta;
			f3Vertex.y *= fScalingDelta;
			oSpriteQuadMeshVerticesArray[ iVertexIndex ] = f3Vertex;
		}
		
		// Update UV
		UpdateUVs(a_rSpriteComponent, a_rTextureAtlas);
		
		// Must set array again ("vertices" getter gives a copy)
		rSpriteQuadMesh.vertices = oSpriteQuadMeshVerticesArray;
		rSpriteQuadMesh.RecalculateBounds();
		
		// If the pivot point has changed
		Vector3 f3NewPivotPoint = f2NewPivotPoint;
		if(a_rSpriteComponent.pivotPointCoords != f3NewPivotPoint)
		{
			// Compute the local position change
			Vector3 f3PivotMovement = (f3NewPivotPoint - a_rSpriteComponent.pivotPointCoords) * fRealScale;
			Vector3 f3SpriteTransformLocalScale = a_rSpriteComponent.transform.localScale;
			
			f3PivotMovement.x *= f3SpriteTransformLocalScale.x;
			f3PivotMovement.y *= f3SpriteTransformLocalScale.y;
			f3PivotMovement.z *= f3SpriteTransformLocalScale.z;
			
			f3PivotMovement = a_rSpriteComponent.transform.TransformDirection(f3PivotMovement);
			
			Transform rParentTransform = a_rSpriteComponent.transform.parent;
			if(rParentTransform != null)
			{
				f3PivotMovement = rParentTransform.InverseTransformDirection(f3PivotMovement);
			}
			
			a_rSpriteComponent.transform.localPosition += f3PivotMovement;
		}
		
		// Save pivot settings
		a_rSpriteComponent.pivotPointType = a_eSpritePivot;
		a_rSpriteComponent.pivotPointCoords = f2NewPivotPoint;
		a_rSpriteComponent.spriteScale = a_fScale;
		a_rSpriteComponent.extrusionDepth = a_fExtrusionDepth;
		a_rSpriteComponent.textureAtlas = a_rTextureAtlas;
		a_rSpriteComponent.VertexColor = a_oVertexColor;
	}
	
	public static void UpdateSpriteSize(Uni2DSprite a_rSpriteComponent, float a_fWidth, float a_fHeight)
	{	
		float fExtrusionDepth = a_rSpriteComponent.extrusionDepth;
		Vector2 f2OldPivotPoint = a_rSpriteComponent.pivotPointCoords;
		
		// The delta to apply
		Vector2 f2ScalingDelta = Vector2.zero;			
		f2ScalingDelta.x = a_fWidth/a_rSpriteComponent.spriteTextureWidth;
		f2ScalingDelta.y = a_fHeight/a_rSpriteComponent.spriteTextureHeight;
		
		// Apply to the pivot point
		Vector2 f2NewPivotPoint = f2OldPivotPoint;
		f2NewPivotPoint.x *= f2ScalingDelta.x;
		f2NewPivotPoint.y *= f2ScalingDelta.y;
		
		// Apply delta to mesh colliders
		if(a_rSpriteComponent.meshCollidersList != null)
		{
			for( int iMeshIndex = 0, iMeshCount = a_rSpriteComponent.meshCollidersList.Count; iMeshIndex < iMeshCount; ++iMeshIndex )
			{
				Mesh rMesh = a_rSpriteComponent.meshCollidersList[iMeshIndex];
				
				Vector3[] oMeshVerticesArray = rMesh.vertices;
				for( int iVertexIndex = 0, iVertexCount = rMesh.vertexCount; iVertexIndex < iVertexCount; ++iVertexIndex )
				{
					Vector3 f3Vertex = oMeshVerticesArray[ iVertexIndex ];
					f3Vertex.x *= f2ScalingDelta.x;
					f3Vertex.y *= f2ScalingDelta.y;
					//f3Vertex -= f3ScaledDeltaPivot;
					f3Vertex.z = Mathf.Sign(f3Vertex.z) * fExtrusionDepth * 0.5f;
					oMeshVerticesArray[ iVertexIndex ] = f3Vertex;
				}
	
				// Must set array again ("vertices" getter gives a copy)
				rMesh.vertices = oMeshVerticesArray;
				
				MeshCollider rMeshCollider = a_rSpriteComponent.meshColliderComponentsList[iMeshIndex];
				if(rMeshCollider != null)
				{
					rMeshCollider.sharedMesh = null;
					rMeshCollider.sharedMesh = rMesh;
				}
			}
		}
		
		// Apply delta to sprite quad mesh
		Mesh rSpriteQuadMesh = a_rSpriteComponent.spriteQuadMesh;
		Vector3[ ] oSpriteQuadMeshVerticesArray = rSpriteQuadMesh.vertices;
		for( int iVertexIndex = 0, iVertexCount = rSpriteQuadMesh.vertexCount; iVertexIndex < iVertexCount; ++iVertexIndex )
		{
			Vector3 f3Vertex = oSpriteQuadMeshVerticesArray[ iVertexIndex ];
			f3Vertex.x *= f2ScalingDelta.x;
			f3Vertex.y *= f2ScalingDelta.y;
			//f3Vertex -= f3ScaledDeltaPivot;
			oSpriteQuadMeshVerticesArray[iVertexIndex] = f3Vertex;
		}

		a_rSpriteComponent.pivotPointCoords = f2NewPivotPoint;
		
		// Must set array again ("vertices" getter gives a copy)
		rSpriteQuadMesh.vertices = oSpriteQuadMeshVerticesArray;
	}
	
	
	public static void UpdateQuadMeshSize(Uni2DSprite a_rSpriteComponent, float a_fWidth, float a_fHeight)
	{
		Mesh rQuadMesh = a_rSpriteComponent.spriteQuadMesh;
		if(rQuadMesh != null)
		{
			float fScale = mc_fToUnit * a_rSpriteComponent.spriteScale;
			
			float fWidth = fScale * a_fWidth;
			float fHeight = fScale * a_fHeight;
			
			Vector2 f2NewPivotPoint = ComputePivotPoint(
			fWidth,
			fHeight,
			a_rSpriteComponent.pivotPointType,
			a_rSpriteComponent.pivotPointCoords );
			
			// Quad vertices
			rQuadMesh.vertices = GenerateQuadVertices(fWidth, fHeight, f2NewPivotPoint);
			
			a_rSpriteComponent.pivotPointCoords = f2NewPivotPoint;
		}
	}
	
	public static Vector3[] GenerateQuadVertices(float a_fWidth, float a_fHeight, Vector3 a_f3PivotPoint)
	{
		// Quad vertices
		return new Vector3[ 4 ]
		{
			new Vector3( 0.0f, 0.0f, 0.0f ) - a_f3PivotPoint,
			new Vector3( a_fWidth, 0.0f, 0.0f ) - a_f3PivotPoint,
			new Vector3( a_fWidth, a_fHeight, 0.0f ) - a_f3PivotPoint,
			new Vector3( 0.0f, a_fHeight, 0.0f ) - a_f3PivotPoint
		};
	}
	
	public static void UpdateUVs(Uni2DSprite a_rSpriteComponent)
	{	
		// Handle the case where the atlas doesn't contains the texture anymore
		if(a_rSpriteComponent.textureAtlas != null && a_rSpriteComponent.textureAtlas.Contains(a_rSpriteComponent.spriteTexture) == false)
		{
			a_rSpriteComponent.textureAtlas = null;
		}
		UpdateUVs(a_rSpriteComponent, a_rSpriteComponent.textureAtlas);
	}
	
	private static void UpdateUVs(
		Uni2DSprite a_rSpriteComponent,
		Uni2DTextureAtlas a_rTextureAtlas)
	{	
		Mesh rSpriteQuadMesh = a_rSpriteComponent.spriteQuadMesh;
		
		// Update UV
		Rect oUVRect;
		Material oQuadSpriteMaterial = a_rSpriteComponent.spriteQuadMaterial;
		if(a_rTextureAtlas == null)
		{
			oUVRect = new Rect(0,0,1,1);
		}
		else
		{
			oUVRect = a_rTextureAtlas.GetUvs(a_rSpriteComponent.spriteTexture);
			oQuadSpriteMaterial = a_rTextureAtlas.atlasMaterial;
		}
		rSpriteQuadMesh.uv = new Vector2[ 4 ]
		{
			new Vector2( oUVRect.xMin, oUVRect.yMin ),
			new Vector2( oUVRect.xMax, oUVRect.yMin ),
			new Vector2( oUVRect.xMax, oUVRect.yMax ),
			new Vector2( oUVRect.xMin, oUVRect.yMax )
		};
		Renderer rRenderer = a_rSpriteComponent.renderer;
		if(rRenderer != null)
		{
			rRenderer.material = oQuadSpriteMaterial;
		}
		
		if(a_rTextureAtlas == null)
		{
			a_rSpriteComponent.atlasGenerationID = "";
		}
		else
		{
			a_rSpriteComponent.atlasGenerationID = a_rTextureAtlas.generationId;
		}
	}

	////////////////////////////////////////////////////////////////////////////////

	public static void DisplaySpriteBuilderGUI(
		ref Uni2DSprite.PhysicMode a_ePhysicMode,
		ref Uni2DSprite.CollisionType a_eCollisionType,
		ref bool a_bIsKinematic,
		ref float a_fAlphaCutOff,
		ref float a_fPolygonizationAccuracy,
		ref float a_fExtrusionDepth,
		ref float a_fScale,
		ref bool a_bPolygonizeHoles,
		ref Uni2DSprite.PivotPointType a_ePivotPoint,
		ref Vector2 a_f2CustomPivotPoint,
		ref bool a_bGUIFoldoutCollisionSettings )
	{
		EditorGUILayout.BeginVertical( );
		{
			
			a_fScale = EditorGUILayout.FloatField( mc_oGUILabelScale, a_fScale );
			a_ePivotPoint = (Uni2DSprite.PivotPointType) EditorGUILayout.EnumPopup( mc_oGUILabelPivot, a_ePivotPoint );

			EditorGUI.BeginDisabledGroup( a_ePivotPoint != Uni2DSprite.PivotPointType.Custom );
			{
				a_f2CustomPivotPoint = EditorGUILayout.Vector2Field( mc_oGUILabelCustomPivotPoint, a_f2CustomPivotPoint );
			}
			EditorGUI.EndDisabledGroup( );

			EditorGUILayout.Separator( );

			a_ePhysicMode = (Uni2DSprite.PhysicMode) EditorGUILayout.EnumPopup("Physic Mode", a_ePhysicMode);
			
			EditorGUI.BeginDisabledGroup(a_ePhysicMode == Uni2DSprite.PhysicMode.NoPhysic);
			{
				// Sprite mesh settings
				a_eCollisionType = (Uni2DSprite.CollisionType) EditorGUILayout.EnumPopup("Collision Type", a_eCollisionType);
				EditorGUI.BeginDisabledGroup(a_ePhysicMode != Uni2DSprite.PhysicMode.Dynamic);
				{
					a_bIsKinematic = EditorGUILayout.Toggle("Is Kinematic", a_bIsKinematic);
				}
				EditorGUI.EndDisabledGroup();
				
				// Collider mode
				a_bGUIFoldoutCollisionSettings = EditorGUILayout.Foldout(a_bGUIFoldoutCollisionSettings, mc_oGUILabelCollider);
				if(a_bGUIFoldoutCollisionSettings)
				{
					++EditorGUI.indentLevel;
					a_fAlphaCutOff            = EditorGUILayout.Slider( mc_oGUILabelAlphaCutOff, a_fAlphaCutOff, 0.0f, 1.0f );	// Threshold or cut-out?
					a_fPolygonizationAccuracy = EditorGUILayout.Slider( mc_oGUILabelAccuracy, a_fPolygonizationAccuracy, 1.0f, 256.0f );
					a_fExtrusionDepth         = EditorGUILayout.FloatField( mc_oGUILabelExtrusion, a_fExtrusionDepth );
					EditorGUI.BeginDisabledGroup(a_eCollisionType == Uni2DSprite.CollisionType.Convex);
					{
						a_bPolygonizeHoles     = EditorGUILayout.Toggle( mc_oGUILabelHoles, a_bPolygonizeHoles );
					}
					EditorGUI.EndDisabledGroup();
					--EditorGUI.indentLevel;
				}
			}
			EditorGUI.EndDisabledGroup();
		}
		EditorGUILayout.EndVertical( );
		
		a_fScale = Mathf.Clamp( a_fScale, mc_fSpriteScaleMin, float.MaxValue );
		a_fExtrusionDepth = Mathf.Clamp( a_fExtrusionDepth, mc_fSpriteExtrusionMin, float.MaxValue );
	}

	////////////////////////////////////////////////////////////////////////////////
	
	public static void DisplayNoTextureWarning(GameObject a_rSpriteGameObject)
	{
		Debug.LogWarning(a_rSpriteGameObject.name + " has no texture");
	}
	
	public static void DisplayNoTextureSoWeDontCreateWarning(GameObject a_rSpriteGameObject)
	{
		Debug.LogWarning(a_rSpriteGameObject.name + " has no texture. Settings has been reverted");
	}
	
	public static void OnPrefabPostProcess(GameObject a_rPrefab)
	{
		// Get all the sprites in the prefab
		List<Uni2DSprite> oSpritesPrefab = new List<Uni2DSprite>();
		GetSpritesInResourceHierarchy(a_rPrefab.transform, ref oSpritesPrefab);
		
		// Check if the prefab is a sprite
		if(oSpritesPrefab.Count > 0)
		{	
			// Check if the prefab need to be saved
			bool bNeedToBeSaved = false;
			foreach(Uni2DSprite rSprite in oSpritesPrefab)
			{
				if(rSprite.BeforePrefabPostProcess())
				{
					bNeedToBeSaved = true;
					break;
				}
			}
			
			// Save the sprite prefab
			if(bNeedToBeSaved)
			{
				// Instantiate the prefab
				GameObject rPrefabInstance = InstantiateSpritePrefab(a_rPrefab);
				
				ReplaceSpritePrefab(rPrefabInstance, a_rPrefab, ReplacePrefabOptions.ReplaceNameBased);
				
				// Clear the prefab instance
				Editor.DestroyImmediate(rPrefabInstance);
			}
		}
	}
	
	// Save prefab
	private static GameObject InstantiateSpritePrefab(GameObject a_rPrefab)
	{			
		// Instantiate the prefab
		GameObject rPrefabInstance = MonoBehaviour.Instantiate(a_rPrefab) as GameObject;
		
		rPrefabInstance.name = rPrefabInstance.name.Replace("(Clone)", "");
		
		return rPrefabInstance;
	}
	
	// Save prefab
	private static GameObject InstantiateSpritePrefabWithConnection(GameObject a_rPrefab)
	{			
		// Instantiate the prefab
		GameObject rPrefabInstance = PrefabUtility.InstantiatePrefab(a_rPrefab) as GameObject;
		PrefabUtility.DisconnectPrefabInstance(rPrefabInstance);
		
		return rPrefabInstance;
	}
	
	// Save prefab
	private static void ReplaceSpritePrefab(GameObject a_rPrefabInstance, GameObject a_rPrefab, ReplacePrefabOptions a_eReplacePrefabOption = ReplacePrefabOptions.Default)
	{
			Uni2DSprite[] oSpritesPrefabInstance = a_rPrefabInstance.GetComponentsInChildren<Uni2DSprite>();
		
			// Save its resources
			string oPrefabPath;
			string oPrefabResourcesName;
			string oPrefabResourcesPath;
			string oPrefabResourcesPathAbsolute;
			GetPrefabResourcesDirectoryPaths(a_rPrefab, out oPrefabPath, out oPrefabResourcesName, out oPrefabResourcesPath, out oPrefabResourcesPathAbsolute);
		
			// Create the resources folder
			if(Directory.Exists(oPrefabResourcesPathAbsolute))
			{
				AssetDatabase.DeleteAsset(oPrefabResourcesPath);
			}
			string oPrefabResourcesFolderPathGUID = AssetDatabase.CreateFolder(oPrefabPath, oPrefabResourcesName);
			oPrefabResourcesPath = AssetDatabase.GUIDToAssetPath(oPrefabResourcesFolderPathGUID);
			
			foreach(Uni2DSprite rSpritePrefabInstance in oSpritesPrefabInstance)
			{
				rSpritePrefabInstance.SaveSpriteAsPartOfAPrefab(oPrefabResourcesPath);
			}
			
			// Replace prefab
			PrefabUtility.ReplacePrefab(a_rPrefabInstance, a_rPrefab, a_eReplacePrefabOption);
	}
	
	// Get resource directory path local
	public static string GetPrefabResourcesDirectoryPathLocal(GameObject a_rPrefab)
	{
		// Ensure we have the root
		a_rPrefab = PrefabUtility.FindPrefabRoot(a_rPrefab);
		
		string oPrefabPath;
		string oPrefabResourcesName;
		string oPrefabResourcesPath;
		string oPrefabResourcesPathAbsolute;
		GetPrefabResourcesDirectoryPaths(a_rPrefab, out oPrefabPath, out oPrefabResourcesName, out oPrefabResourcesPath, out oPrefabResourcesPathAbsolute);
		
		return oPrefabResourcesPath;
	}
	
	// Get resource directory path absoluter
	private static void GetPrefabResourcesDirectoryPaths(GameObject a_rPrefab, out string a_rPrefabPath, out string a_rPrefabResourcesName, 
		out string a_rPrefabResourcesPath, out string a_rPrefabResourcesPathAbsolute)
	{
		a_rPrefabPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(a_rPrefab));
		a_rPrefabResourcesName = a_rPrefab.name + "_Resources";
		a_rPrefabResourcesPath = a_rPrefabPath + "/" + a_rPrefabResourcesName;
		a_rPrefabResourcesPathAbsolute = Application.dataPath.Replace("Assets", "") + a_rPrefabResourcesPath;
	}
	
	// Get all the sprite mesh components in a resource hierarchy
	public static void GetSpritesInResourceHierarchy(Transform a_rRoot, ref List<Uni2DSprite> a_rSprites)
	{
		a_rSprites.AddRange(a_rRoot.GetComponents<Uni2DSprite>());
		
		// Recursive call
		foreach(Transform rChild in a_rRoot)
		{	
			GetSpritesInResourceHierarchy(rChild, ref a_rSprites);
		}
	}
	
	// Is there at least one sprite in a resource hierarchy
	public static bool IsThereAtLeastOneSpriteContainingTheTextureInResourceHierarchy(Transform a_rRoot, Texture2D a_rTexture)
	{
		Uni2DSprite rSprite = a_rRoot.GetComponent<Uni2DSprite>();
		if(rSprite != null)
		{
			if(rSprite.spriteTexture == a_rTexture)
			{
				return true;
			}
		}
		
		// Recursive call
		foreach(Transform rChild in a_rRoot)
		{	
			if(IsThereAtLeastOneSpriteContainingTheTextureInResourceHierarchy(rChild, a_rTexture))
			{
				return true;
			}
		}
		
		return false;
	}
	
	// Is there at least one sprite in a resource hierarchy
	public static bool IsThereAtLeastOneSpriteContainingTheAtlasInResourceHierarchy(Transform a_rRoot, Uni2DTextureAtlas a_rTextureAtlas)
	{
		Uni2DSprite rSprite = a_rRoot.GetComponent<Uni2DSprite>();
		if(rSprite != null)
		{
			if(rSprite.textureAtlas == a_rTextureAtlas)
			{
				return true;
			}
		}
		
		// Recursive call
		foreach(Transform rChild in a_rRoot)
		{	
			if(IsThereAtLeastOneSpriteContainingTheAtlasInResourceHierarchy(rChild, a_rTextureAtlas))
			{
				return true;
			}
		}
		
		return false;
	}
	
	// Setup rigidbody 2d
	private static void SetupRigidbodyFor2D(Rigidbody a_rRigidbody, bool a_bIsKinematic)
	{
		a_rRigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
		a_rRigidbody.isKinematic = a_bIsKinematic;
	}
	
	// Texture processing Begin
	public static TextureImporterSettings TextureProcessingBegin(Texture2D a_rTexture)
	{
		TextureImporter rTextureImporter = TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(a_rTexture)) as TextureImporter;
		if(rTextureImporter == null)
		{
			return null;
		}
		else
		{
			return TextureProcessingBegin(rTextureImporter);
		}
	}
	
	// Texture processing End
	public static void TextureProcessingEnd(Texture2D a_rTexture, TextureImporterSettings a_rTextureImporterSettings)
	{
		TextureImporter rTextureImporer = TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(a_rTexture)) as TextureImporter;
		TextureProcessingEnd(rTextureImporer, a_rTextureImporterSettings);
	}
	
	// Texture processing Begin
	public static TextureImporterSettings TextureProcessingBegin(TextureImporter a_rTextureImporter)
	{
		Uni2DEditorSpriteAssetPostProcessor.Enabled = false;
		
		// If it's the first time Uni2d use this texture
		// Set the default texture importer settings
		Texture2D rTexture = AssetDatabase.LoadAssetAtPath(a_rTextureImporter.assetPath, typeof(Texture2D)) as Texture2D;
		if(Uni2DEditorUtils.ItIsTheFirstTimeWeUseTheTexture(rTexture))
		{
			SetDefaultTextureImporterSettings(a_rTextureImporter);
			Uni2DEditorUtils.GenerateTextureImportGUID(rTexture);
		}
		
		TextureImporterSettings rTextureImporterSettings = new TextureImporterSettings();
		a_rTextureImporter.ReadTextureSettings(rTextureImporterSettings);
			
		// Reimport texture as readable and at original size
		a_rTextureImporter.isReadable = true;
		a_rTextureImporter.npotScale = TextureImporterNPOTScale.None;
		a_rTextureImporter.maxTextureSize = 4096;
		a_rTextureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
		
		if(AssetDatabase.WriteImportSettingsIfDirty(a_rTextureImporter.assetPath))
		{
			AssetDatabase.ImportAsset(a_rTextureImporter.assetPath);
			AssetDatabase.Refresh();
		}
		
		return rTextureImporterSettings;
	}
	
	// Texture processing End
	public static void TextureProcessingEnd(TextureImporter a_rTextureImporter, TextureImporterSettings a_rTextureImporterSettings)
	{
		if(a_rTextureImporter != null)
		{
			a_rTextureImporter.SetTextureSettings(a_rTextureImporterSettings);
			
			if(AssetDatabase.WriteImportSettingsIfDirty(a_rTextureImporter.assetPath))
			{
				AssetDatabase.ImportAsset(a_rTextureImporter.assetPath);
				AssetDatabase.Refresh();
			}
		}
		
		Uni2DEditorSpriteAssetPostProcessor.Enabled = true;
	}
	
	// Set default texture importer settings
	public static void SetDefaultTextureImporterSettings(Texture2D a_rTexture)
	{
		TextureImporter rTextureImporter = TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(a_rTexture)) as TextureImporter;
		SetDefaultTextureImporterSettings(rTextureImporter);
	}
	
	// Texture processing End
	public static void SetDefaultTextureImporterSettings(TextureImporter a_rTextureImporter)
	{
		//Debug.Log("Set Deafult Importer Settings");
		a_rTextureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
	}
	
	// Processing begin for multiple textures
	public static List<Uni2DTextureImporterSettingsPair> TexturesProcessingBegin(Texture2D[] a_rTextures)
	{
		// Prepare textures for processing
		HashSet<Texture> oAlreadyPreparedTextures = new HashSet<Texture>();
		List<Uni2DTextureImporterSettingsPair> oTextureImportersSettingPairs = new List<Uni2DTextureImporterSettingsPair>();
		foreach(Texture2D rTexture in a_rTextures)
		{
			// If we haven't yet prepare this sprite texture
			if(oAlreadyPreparedTextures.Contains(rTexture) == false)
			{
				oAlreadyPreparedTextures.Add(rTexture);
				TextureImporterSettings rTextureImporterSettings = Uni2DEditorUtilsSpriteBuilder.TextureProcessingBegin(rTexture);
				oTextureImportersSettingPairs.Add(new Uni2DTextureImporterSettingsPair(rTexture, rTextureImporterSettings));
			}
		}

		return oTextureImportersSettingPairs;
	}
	
	// Processing end for multiple textures
	public static void TexturesProcessingEnd(List<Uni2DTextureImporterSettingsPair> a_rTextureImporterSettingsPairs)
	{
		// Restore textures settings
		foreach(Uni2DTextureImporterSettingsPair rTextureImporterSettingsPair in a_rTextureImporterSettingsPairs)
		{
			Uni2DEditorSpriteAssetPostProcessor.Enabled = false;
			Uni2DEditorUtilsSpriteBuilder.TextureProcessingEnd(rTextureImporterSettingsPair.Key, rTextureImporterSettingsPair.Value);
		}
		Uni2DEditorSpriteAssetPostProcessor.Enabled = true;
	}
	
	//----------------
	// Update sprite
	//----------------
	
	// Update the sprite in current scene and resources accordingly to texture change
	public static void UpdateSpriteInCurrentSceneAndResourcesAccordinglyToTextureChange(Texture2D a_rTexture, string a_oNewTextureImportGUID)
	{
		string oTexturePath = AssetDatabase.GetAssetPath(a_rTexture);
		TextureImporter rTextureImporter = TextureImporter.GetAtPath(oTexturePath) as TextureImporter;
		if(rTextureImporter != null)
		{	
			TextureImporterSettings oTextureImporterSettings = Uni2DEditorUtilsSpriteBuilder.TextureProcessingBegin(rTextureImporter);
	
			Uni2DEditorUtilsSpriteBuilder.DoUpdateAllResourcesSpritesAccordinglyToTextureChange(a_rTexture, a_oNewTextureImportGUID);
			Uni2DEditorUtilsSpriteBuilder.DoUpdateAllSceneSpritesAccordinglyToTextureChange(a_rTexture, a_oNewTextureImportGUID);
			
			Uni2DEditorUtilsSpriteBuilder.TextureProcessingEnd(rTextureImporter, oTextureImporterSettings);
	
			EditorUtility.UnloadUnusedAssets( );
		}
	}
	
	// Do Update all scene sprites accordingly to texture change for a texture change
	public static void DoUpdateAllSceneSpritesAccordinglyToTextureChange(Texture2D a_rTexture, string a_oNewTextureImportGUID)
	{	
		// Loop through all the scene sprites
		foreach(Uni2DSprite rSprite in MonoBehaviour.FindObjectsOfType(typeof(Uni2DSprite)))
		{
			if(rSprite.spriteTexture == a_rTexture)
			{
				rSprite.UpdateAccordinglyToTextureChange(a_oNewTextureImportGUID);
			}
		}
	}
	
	// Do Update all scene sprites accordingly to texture change for a texture change
	private static void DoUpdateAllResourcesSpritesAccordinglyToTextureChange(Texture2D a_rTexture, string a_oNewTextureImportGUID)
	{	
		// Loop through all the prefab containing at least a sprite
		foreach(string rAssetPath in AssetDatabase.GetAllAssetPaths())
		{
			GameObject rPrefab = AssetDatabase.LoadAssetAtPath(rAssetPath, typeof(GameObject)) as GameObject;
			if(rPrefab != null)
			{
				// Is there at least one sprite in the prefab
				if(IsThereAtLeastOneSpriteContainingTheTextureInResourceHierarchy(rPrefab.transform, a_rTexture))
				{
					DoUpdatePrefabContainingSpriteAccordinglyToTextureChange(rPrefab, a_rTexture, a_oNewTextureImportGUID);
				}
			}
		}
	}
	
	// Do Update all scene prefab containing at least a sprite accordingly to texture change for a texture change
	private static void DoUpdatePrefabContainingSpriteAccordinglyToTextureChange(GameObject a_rPrefab, Texture2D a_rTexture, string a_oNewTextureImportGUID)
	{		
		DoUpdatePrefabContainingSpriteAccordinglyToTextureChangeRecursively(a_rPrefab.transform, a_rTexture, a_oNewTextureImportGUID);
	}
	
	// Do Update all scene prefab containing at least a sprite accordingly to texture change for a texture change
	private static void DoUpdatePrefabContainingSpriteAccordinglyToTextureChangeRecursively(Transform a_rRoot, Texture2D a_rTexture, string a_oNewTextureImportGUID)
	{	
		// loop through the sprite containing the changed texture
		foreach(Uni2DSprite rSpritePrefabInstance in a_rRoot.GetComponents<Uni2DSprite>())
		{
			if(rSpritePrefabInstance.spriteTexture == a_rTexture)
			{
				rSpritePrefabInstance.UpdateAccordinglyToTextureChange(a_oNewTextureImportGUID);
			}
		}
		
		// Recursive call
		foreach(Transform rChild in a_rRoot)
		{	
			DoUpdatePrefabContainingSpriteAccordinglyToTextureChangeRecursively(rChild, a_rTexture, a_oNewTextureImportGUID);
		}
	}
	
	//----------------
	// Update Uvs
	//----------------
	
	// Update the sprite in current scene and resources accordingly to texture change
	public static void UpdateSpriteInCurrentSceneAndResourcesAccordinglyToAtlasChange(Uni2DTextureAtlas a_rAtlas)
	{
		Uni2DEditorUtilsSpriteBuilder.DoUpdateAllResourcesSpritesAccordinglyToAtlasChange(a_rAtlas);
		Uni2DEditorUtilsSpriteBuilder.DoUpdateAllSceneSpritesAccordinglyToAtlasChange(a_rAtlas);
	}
	
	// Do Update all scene sprites accordingly to texture change for a texture change
	private static void DoUpdateAllSceneSpritesAccordinglyToAtlasChange(Uni2DTextureAtlas a_rAtlas)
	{	
		// Loop through all the scene sprites
		foreach(Uni2DSprite rSprite in MonoBehaviour.FindObjectsOfType(typeof(Uni2DSprite)))
		{
			if(rSprite.textureAtlas == a_rAtlas)
			{
				rSprite.UpdateUvs();
			}
		}
	}
	
	// Do Update all scene sprites accordingly to texture change for a texture change
	private static void DoUpdateAllResourcesSpritesAccordinglyToAtlasChange(Uni2DTextureAtlas a_rAtlas)
	{	
		// Loop through all the prefab containing at least a sprite
		foreach(string rAssetPath in AssetDatabase.GetAllAssetPaths())
		{
			GameObject rPrefab = AssetDatabase.LoadAssetAtPath(rAssetPath, typeof(GameObject)) as GameObject;
			if(rPrefab != null)
			{
				// Is there at least one sprite containing the atlas in the prefab
				if(IsThereAtLeastOneSpriteContainingTheAtlasInResourceHierarchy(rPrefab.transform, a_rAtlas))
				{
					DoUpdatePrefabContainingSpriteAccordinglyToAtlasChange(rPrefab, a_rAtlas);
				}
			}
		}
	}
	
	// Do Update all scene prefab containing at least a sprite accordingly to atlas change for a texture change
	private static void DoUpdatePrefabContainingSpriteAccordinglyToAtlasChange(GameObject a_rPrefab, Uni2DTextureAtlas a_rAtlas)
	{		
		DoUpdatePrefabContainingSpriteAccordinglyToAtlasChangeRecursively(a_rPrefab.transform, a_rAtlas);
	}
	
	// Do Update all scene prefab containing at least a sprite accordingly to atlas change for a texture change
	private static void DoUpdatePrefabContainingSpriteAccordinglyToAtlasChangeRecursively(Transform a_rRoot, Uni2DTextureAtlas a_rAtlas)
	{	
		// loop through the sprite containing the changed texture
		foreach(Uni2DSprite rSpritePrefabInstance in a_rRoot.GetComponents<Uni2DSprite>())
		{
			if(rSpritePrefabInstance.textureAtlas == a_rAtlas)
			{
				rSpritePrefabInstance.UpdateUvs();
			}
		}
		
		// Recursive call
		foreach(Transform rChild in a_rRoot)
		{	
			DoUpdatePrefabContainingSpriteAccordinglyToAtlasChangeRecursively(rChild, a_rAtlas);
		}
	}
	
	//----------------
	// Update phyisc
	//----------------
	
	// Update the dirty physic sprites in current scene and resources
	/*public static void UpdateDirtyPhyiscSpritesInCurrentSceneAndResources()
	{
		string oTexturePath = AssetDatabase.GetAssetPath(a_rTexture);
		TextureImporter rTextureImporter = TextureImporter.GetAtPath(oTexturePath) as TextureImporter;
		if(rTextureImporter != null)
		{	
			TextureImporterSettings oTextureImporterSettings = Uni2DEditorUtilsSpriteBuilder.TextureProcessingBegin(rTextureImporter);
	
			Uni2DEditorUtilsSpriteBuilder.DoUpdateAllResourcesSpritesAccordinglyToTextureChange(a_rTexture, a_oNewTextureImportGUID);
			Uni2DEditorUtilsSpriteBuilder.DoUpdateAllSceneSpritesAccordinglyToTextureChange(a_rTexture, a_oNewTextureImportGUID);
			
			Uni2DEditorUtilsSpriteBuilder.TextureProcessingEnd(rTextureImporter, oTextureImporterSettings);
	
			EditorUtility.UnloadUnusedAssets( );
		}
	}
	
	// Do Update all scene sprites accordingly to texture change for a texture change
	public static void DoUpdateAllSceneSpritesAccordinglyToTextureChange(Texture2D a_rTexture, string a_oNewTextureImportGUID)
	{	
		// Loop through all the scene sprites
		foreach(Uni2DSprite rSprite in MonoBehaviour.FindObjectsOfType(typeof(Uni2DSprite)))
		{
			if(rSprite.spriteTexture == a_rTexture)
			{
				rSprite.UpdateAccordinglyToTextureChange(a_oNewTextureImportGUID);
			}
		}
	}
	
	// Do Update all scene sprites accordingly to texture change for a texture change
	private static void DoUpdateAllResourcesSpritesAccordinglyToTextureChange(Texture2D a_rTexture, string a_oNewTextureImportGUID)
	{	
		// Loop through all the prefab containing at least a sprite
		foreach(string rAssetPath in AssetDatabase.GetAllAssetPaths())
		{
			GameObject rPrefab = AssetDatabase.LoadAssetAtPath(rAssetPath, typeof(GameObject)) as GameObject;
			if(rPrefab != null)
			{
				// Is there at least one sprite in the prefab
				if(IsThereAtLeastOneSpriteContainingTheTextureInResourceHierarchy(rPrefab.transform, a_rTexture))
				{
					DoUpdatePrefabContainingSpriteAccordinglyToTextureChange(rPrefab, a_rTexture, a_oNewTextureImportGUID);
				}
			}
		}
	}
	
	// Do Update all scene prefab containing at least a sprite accordingly to texture change for a texture change
	private static void DoUpdatePrefabContainingSpriteAccordinglyToTextureChange(GameObject a_rPrefab, Texture2D a_rTexture, string a_oNewTextureImportGUID)
	{		
		DoUpdatePrefabContainingSpriteAccordinglyToTextureChangeRecursively(a_rPrefab.transform, a_rTexture, a_oNewTextureImportGUID);
	}
	
	// Do Update all scene prefab containing at least a sprite accordingly to texture change for a texture change
	private static void DoUpdatePrefabContainingSpriteAccordinglyToTextureChangeRecursively(Transform a_rRoot, Texture2D a_rTexture, string a_oNewTextureImportGUID)
	{	
		// loop through the sprite containing the changed texture
		foreach(Uni2DSprite rSpritePrefabInstance in a_rRoot.GetComponents<Uni2DSprite>())
		{
			if(rSpritePrefabInstance.spriteTexture == a_rTexture)
			{
				rSpritePrefabInstance.UpdateAccordinglyToTextureChange(a_oNewTextureImportGUID);
			}
		}
		
		// Recursive call
		foreach(Transform rChild in a_rRoot)
		{	
			DoUpdatePrefabContainingSpriteAccordinglyToTextureChangeRecursively(rChild, a_rTexture, a_oNewTextureImportGUID);
		}
	}*/
	
	// Update a sprite in resource 
	public static void UpdateSpriteInResource(Uni2DSprite a_rSprite)
	{
		ms_bUndoEnabled = false;
		//Uni2DSprite.prefabUpdateInProgress = true;
		GameObject rPrefab = PrefabUtility.FindPrefabRoot(a_rSprite.gameObject);
			
		// Instantiate the prefab
		GameObject rPrefabInstance = InstantiateSpritePrefabWithConnection(rPrefab);
		Uni2DSprite[] oSpritesPrefabInstance = rPrefabInstance.GetComponentsInChildren<Uni2DSprite>();
		
		// Retrieve the instance of the sprite
		Uni2DSprite rSpriteInstance = null;
		foreach(Uni2DSprite rSpritePrefabInstance in oSpritesPrefabInstance)
		{
			if(PrefabUtility.GetPrefabParent(rSpritePrefabInstance) == a_rSprite)
			{
				rSpriteInstance = rSpritePrefabInstance;
			}
		}
		
		if(rSpriteInstance != null)
		{
			// Rebuild the sprite
			rSpriteInstance.Rebuild();
			
			// Replace prefab
			ReplaceSpritePrefab(rPrefabInstance, rPrefab);
		}
			
		// Clear the prefab instance
		Editor.DestroyImmediate(rPrefabInstance);
		//Uni2DSprite.prefabUpdateInProgress = false;
		
		AssetDatabase.SaveAssets();
		
		ms_bUndoEnabled = true;
	}
	
	// Update a sprite in resource 
	public static void UpdateSpriteInResourceInABatch(Uni2DSprite a_rSprite)
	{
		ms_bUndoEnabled = false;
		
		//Uni2DSprite.prefabUpdateInProgress = true;
		GameObject rPrefab = PrefabUtility.FindPrefabRoot(a_rSprite.gameObject);
			
		// Instantiate the prefab
		GameObject rPrefabInstance = InstantiateSpritePrefabWithConnection(rPrefab);
		Uni2DSprite[] oSpritesPrefabInstance = rPrefabInstance.GetComponentsInChildren<Uni2DSprite>();
		
		// Retrieve the instance of the sprite
		Uni2DSprite rSpriteInstance = null;
		foreach(Uni2DSprite rSpritePrefabInstance in oSpritesPrefabInstance)
		{
			if(PrefabUtility.GetPrefabParent(rSpritePrefabInstance) == a_rSprite)
			{
				rSpriteInstance = rSpritePrefabInstance;
			}
		}
		
		if(rSpriteInstance != null)
		{
			// Rebuild the sprite
			rSpriteInstance.RebuildInABatch();
			
			// Replace prefab
			ReplaceSpritePrefab(rPrefabInstance, rPrefab);
		}
			
		// Clear the prefab instance
		Editor.DestroyImmediate(rPrefabInstance);
		//Uni2DSprite.prefabUpdateInProgress = false;
		
		AssetDatabase.SaveAssets();
		
		ms_bUndoEnabled = true;
	}
}
#endif