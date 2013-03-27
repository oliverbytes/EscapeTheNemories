#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

// parameters used to temporary save sprite changes before application
public class Uni2DEditorSpriteMeshInspectorParameters 
{
	// Parameters
	public bool settingsChanged = false;
	public Uni2DSprite.PhysicMode physicMode;
	public Uni2DSprite.CollisionType collisionType;
	public bool isKinematic;
	public Uni2DSprite.PivotPointType pivotPointType;
	public Texture2D spriteTexture = null;
	public Uni2DTextureAtlas textureAtlas;
	public Color vertexColor;
	public float alphaCutOff;
	public float polygonizationAccuracy;
	public bool polygonizeHoles;
	public float spriteScale;
	public float extrusionDepth;
	public Vector2 pivotPointCoords;
	public int colliderTriangleCount;
	
	// Data
	public float spriteTextureWidth = 0.0f;
	public float spriteTextureHeight = 0.0f;
	public Mesh spriteQuadMesh = null;
	public Material spriteQuadMaterial = null;
	public List<Mesh> meshCollidersList = new List<Mesh>();
	public GameObject meshCollidersRootGameObject = null;
	public List<MeshCollider> meshColliderComponentsList = new List<MeshCollider>();
	
	// Parameters
	private Uni2DSprite.PhysicMode m_ePhysicModeInit;
	private Uni2DSprite.CollisionType m_eCollisionTypeInit;
	private Uni2DSprite.PivotPointType m_ePivotPointTypeInit;
	public bool m_bIsKinematicInit;
	private Texture2D m_rSpriteTextureInit;
	private Uni2DTextureAtlas m_rTextureAtlasInit;
	private Color m_oVertexColorInit;
	private float m_fAlphaCutOffInit;
	private float m_fPolygonizationAccuracyInit;
	private bool m_bPolygonizeHolesInit;
	private float m_fSpriteScaleInit;
	private float m_fExtrusionDepthInit;
	private Vector2 m_f2PivotPointCoordsInit;
	private int m_iColliderTriangleCountInit;
	
	private bool ms_bInspectorHasBeenInit;
	
	// On init inspector
	public void OnInitInspector(Uni2DSprite a_rSpriteMesh)
	{
		SaveInspectorInitParameter(a_rSpriteMesh);
		
		// Parameters
		spriteTexture = a_rSpriteMesh.spriteTexture;
		textureAtlas = a_rSpriteMesh.textureAtlas;
		vertexColor = a_rSpriteMesh.VertexColor;
		physicMode = a_rSpriteMesh.physicMode;
		collisionType = a_rSpriteMesh.collisionType;
		isKinematic = a_rSpriteMesh.isKinematic;
		spriteScale = a_rSpriteMesh.spriteScale;
		pivotPointType = a_rSpriteMesh.pivotPointType;
		pivotPointCoords = a_rSpriteMesh.pivotPointCoords;
		alphaCutOff = a_rSpriteMesh.alphaCutOff;
		polygonizationAccuracy = a_rSpriteMesh.polygonizationAccuracy;
		polygonizeHoles = a_rSpriteMesh.polygonizeHoles;
		extrusionDepth = a_rSpriteMesh.extrusionDepth;
		
		// Info
		colliderTriangleCount  = a_rSpriteMesh.GetColliderTriangleCount();
		
		// Data
		spriteTextureWidth = a_rSpriteMesh.spriteTextureWidth;
		spriteTextureHeight = a_rSpriteMesh.spriteTextureHeight;
		spriteQuadMesh = a_rSpriteMesh.spriteQuadMesh;
		spriteQuadMaterial = a_rSpriteMesh.spriteQuadMaterial;
		meshCollidersList.Clear();
		if(a_rSpriteMesh.meshCollidersList != null)
		{
			meshCollidersList.AddRange(a_rSpriteMesh.meshCollidersList);
		}
		meshCollidersRootGameObject = a_rSpriteMesh.meshCollidersRootGameObject;
		meshColliderComponentsList.Clear();
		if(a_rSpriteMesh.meshColliderComponentsList != null)
		{
			meshColliderComponentsList.AddRange(a_rSpriteMesh.meshColliderComponentsList);
		}
		
		ms_bInspectorHasBeenInit = true;
	}
	
	// Save inspector
	public void SaveInspectorInitParameter(Uni2DSprite a_rSpriteMesh)
	{
		// Parameters
		m_rSpriteTextureInit = a_rSpriteMesh.spriteTexture;
		m_rTextureAtlasInit = a_rSpriteMesh.textureAtlas;
		m_oVertexColorInit = a_rSpriteMesh.VertexColor;
		m_ePhysicModeInit = a_rSpriteMesh.physicMode;
		m_eCollisionTypeInit = a_rSpriteMesh.collisionType;
		m_bIsKinematicInit = a_rSpriteMesh.isKinematic;
		m_fSpriteScaleInit = a_rSpriteMesh.spriteScale;
		m_ePivotPointTypeInit = a_rSpriteMesh.pivotPointType;
		m_f2PivotPointCoordsInit = a_rSpriteMesh.pivotPointCoords;
		m_fAlphaCutOffInit = a_rSpriteMesh.alphaCutOff;
		m_fPolygonizationAccuracyInit = a_rSpriteMesh.polygonizationAccuracy;
		m_bPolygonizeHolesInit = a_rSpriteMesh.polygonizeHoles;
		m_fExtrusionDepthInit = a_rSpriteMesh.extrusionDepth;
	}
	
	// Restore inspector
	public void RestoreInspectorInitParameter(Uni2DSprite a_rSpriteMesh)
	{
		// Parameters
		a_rSpriteMesh.spriteTexture = m_rSpriteTextureInit;
		a_rSpriteMesh.textureAtlas = m_rTextureAtlasInit;
		a_rSpriteMesh.VertexColor = m_oVertexColorInit;
		a_rSpriteMesh.physicMode = m_ePhysicModeInit;
		a_rSpriteMesh.collisionType = m_eCollisionTypeInit;
		a_rSpriteMesh.isKinematic = m_bIsKinematicInit;
		a_rSpriteMesh.spriteScale = m_fSpriteScaleInit;
		a_rSpriteMesh.pivotPointType = m_ePivotPointTypeInit;
		a_rSpriteMesh.pivotPointCoords = m_f2PivotPointCoordsInit;
		a_rSpriteMesh.alphaCutOff = m_fAlphaCutOffInit;
		a_rSpriteMesh.polygonizationAccuracy = m_fPolygonizationAccuracyInit;
		a_rSpriteMesh.polygonizeHoles = m_bPolygonizeHolesInit;
		a_rSpriteMesh.extrusionDepth = m_fExtrusionDepthInit;
	}
	
	// On sprite reset
	public void OnSpriteReset(Uni2DSprite a_rSpriteMesh)
	{
		if(ms_bInspectorHasBeenInit)
		{
			// Cancel Reset
			
			// Parameters
			RestoreInspectorInitParameter(a_rSpriteMesh);
			
			// Data
			a_rSpriteMesh.spriteTextureWidth = spriteTextureWidth;
			a_rSpriteMesh.spriteTextureHeight = spriteTextureHeight;
			a_rSpriteMesh.spriteQuadMesh = spriteQuadMesh;
			a_rSpriteMesh.spriteQuadMaterial = spriteQuadMaterial;
			a_rSpriteMesh.meshCollidersList.Clear();
			a_rSpriteMesh.meshCollidersList.AddRange(meshCollidersList);
			a_rSpriteMesh.meshCollidersRootGameObject = meshCollidersRootGameObject;
			a_rSpriteMesh.meshColliderComponentsList.Clear();
			a_rSpriteMesh.meshColliderComponentsList.AddRange(meshColliderComponentsList);
			
			// Reset the temporary values
			Uni2DEditorSpriteBuilderWindow.ResetSpriteParameters(	ref vertexColor,
																	ref alphaCutOff,
																	ref polygonizationAccuracy,
																	ref extrusionDepth,
																	ref spriteScale,
																	ref polygonizeHoles,
																	ref pivotPointCoords,
																	ref pivotPointType,
																	ref physicMode,
																	ref collisionType,
																	ref isKinematic);
	
			
			settingsChanged = false;
			
			ApplySettings(a_rSpriteMesh);
		}
	}
	
	public bool ApplySettings(Uni2DSprite a_rSpriteMesh)
	{
		if(spriteTexture != null)
		{
			settingsChanged = false;
		
			GameObject rSpriteMeshGameObject = a_rSpriteMesh.gameObject;
	
			Uni2DEditorUtilsSpriteBuilder.UpdateSpriteFromTexture(
				rSpriteMeshGameObject,
				spriteTexture,
				textureAtlas,
				vertexColor,
				alphaCutOff,
				polygonizationAccuracy,
				extrusionDepth,
				spriteScale,
				polygonizeHoles,
				pivotPointCoords,
				pivotPointType,
				physicMode,
				collisionType,
				isKinematic);
			
			EditorUtility.SetDirty(a_rSpriteMesh);
			
			return true;
		}
		else
		{
			return false;
		}
	}
}
#endif