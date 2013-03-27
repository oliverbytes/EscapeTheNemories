#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Uni2DEditorSpriteBuilderWindow : EditorWindow
{
	private enum SpriteLibraryDisplay
	{
		AllSprites,
		OutdatedSprites,
	}
	
	private enum SpriteLibraryDisplayScope
	{
		CurrentScene,
		Project,
		ProjectAndCurrentScene,
	}
	
	private const string mc_oTextureAtlasDefaultName = "TextureAtlas";
	
	// Editor window settings
	private const string mc_oWindowTitle    = "Sprite Editor";
	private const bool mc_bWindowUtility    = false;	// Floating window?
	private const bool mc_bWindowTakeFocus  = false;
	private const float mc_fWindowMinWidth  = 250.0f;
	private const float mc_fWindowMinHeight = 150.0f;
	
	// section
	private const string mc_oGUILabelBuildSection = "Builder";
	private const string mc_oGUILabelLibrarySection = "Library";
	
	// Editor window GUI
	private const string mc_oGUILabelCreateSprite = "Create Sprite";
	private const string mc_oGUILabelCreatePhysicSprite = "Create Physic Sprite";
	private const string mc_oGUILabelSaveSprite = "Save as Prefab";
	
	// Default sprite parameters
	private static Color m_oVertexColorSprite = Color.white;
	private const Uni2DSprite.PhysicMode m_ePhysicModeSprite = Uni2DSprite.PhysicMode.NoPhysic;
	private const Uni2DSprite.CollisionType m_eCollisionTypeSprite = Uni2DSprite.CollisionType.Convex;
	private const bool m_bIsKinematicSprite = false;
	private const float m_fScaleSprite = 1.0f;
	private const Uni2DSprite.PivotPointType m_ePivotPointSprite = Uni2DSprite.PivotPointType.MiddleCenter;
	private static Vector2 m_f2CustomPivotPointSprite = Vector2.zero;
	
	// Default physic sprite parameters
	private const Uni2DSprite.PhysicMode m_ePhysicModePhysicSprite = Uni2DSprite.PhysicMode.Dynamic;
	private const Uni2DSprite.CollisionType m_eCollisionTypePhysicSprite = Uni2DSprite.CollisionType.Convex;
	private const bool m_bIsKinematicPhysicSprite = false;
	private const float m_fAlphaCutOffPhysicSprite = 0.75f;
	private const float m_fPolygonizationAccuracyPhysicSprite = 5.0f;
	private const bool m_bPolygonizeHolesPhysicSprite = false;
	private const float m_fExtrusionDepthPhysicSprite = 0.5f;

	private Object[ ] m_rSelectedObjects = null;
	
	// Library
	
	// Editor window GUI
	private const string mc_oGUILabelFilter = "Filter";
	private const string mc_oGUILabelUpdate = "Update Physic";

	// Scroll view position
	private Vector2 m_f2GUIScrollingPosition = Vector2.zero;
	// Prefab filter
	private SpriteLibraryDisplay m_eDisplayMode = SpriteLibraryDisplay.AllSprites;
	private SpriteLibraryDisplayScope m_eDisplayModeScope = SpriteLibraryDisplayScope.CurrentScene;
	
	// Lock list update?
	private static bool ms_bLockListUpdate = false;
	
	// Current scene sprites
	private List<Uni2DSprite> m_oCurrentSceneSprites = new List<Uni2DSprite>();
	
	// Project sprites
	private List<Uni2DSprite> m_oProjectSprites = new List<Uni2DSprite>();
	
	// Project and current scene sprites
	private List<Uni2DSprite> m_oProjectAndCurrentSceneSprites = new List<Uni2DSprite>();

	[MenuItem( "Uni2D/" + mc_oWindowTitle )]
	public static void CreateEditorWindow( )
	{
		Uni2DEditorSpriteBuilderWindow oEditorWindow = EditorWindow.GetWindow<Uni2DEditorSpriteBuilderWindow>( mc_bWindowUtility, mc_oWindowTitle, mc_bWindowTakeFocus );
		oEditorWindow.minSize = new Vector2( mc_fWindowMinWidth, mc_fWindowMinHeight );
	}
	
	[MenuItem("Assets/Create/Uni2D/Texture Atlas")]
    static void DoCreateTextureAtlasAssetsMenu()
    {
		DoCreateTextureAtlas();
	}
	
	[MenuItem("Uni2D/Create/Texture Atlas")]
    static void DoCreateTextureAtlasMainMenu()
    {
		DoCreateTextureAtlas();
	}
	
	
	static void DoCreateTextureAtlas()
    {
		// Get the selected path
		string oNewPrefabPath = Uni2DEditorUtils.GenerateNewPrefabLocalPath(mc_oTextureAtlasDefaultName);
		
		// Create model
        GameObject oPrefabModel = new GameObject();
        oPrefabModel.AddComponent<Uni2DTextureAtlas>();
		
		// Save it as a prefab
		PrefabUtility.CreatePrefab(oNewPrefabPath, oPrefabModel);
		
		// Destroy model
        GameObject.DestroyImmediate(oPrefabModel);
	}
	
	private void OnEnable( )
	{
		this.UpdateSpriteMeshList( );
		Uni2DEditorSpriteAssetPostProcessor.OnPostprocessHandler += this.Repaint;
		SceneView.onSceneGUIDelegate += OnSceneGUI;
		UpdateSelection();
		this.Repaint( );
	}

	private void OnDestroy( )
	{
		Uni2DEditorSpriteAssetPostProcessor.OnPostprocessHandler -= this.Repaint;
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
	}
	
	private void OnSelectionChange( )
	{
		// Default Unity selection mode
		UpdateSelection();
		this.Repaint( );
	}
	
	private void UpdateSelection()
	{	
		m_rSelectedObjects = Selection.GetFiltered( typeof( Texture2D ), SelectionMode.Assets );
	}
	
	private void OnGUI( )
	{
		UpdateSelection();
		DisplayBuilder();
		EditorGUILayout.Separator();
		DisplayLibrary();
	}
	
	private void OnSceneGUI(SceneView a_rSceneView)
    {
       if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
	    {
			List<Texture2D> oDraggedTextures = new List<Texture2D>();
	        foreach(Object rObject in DragAndDrop.objectReferences)
			{
				if(rObject is Texture2D)
				{
					oDraggedTextures.Add(rObject as Texture2D);
				}
			}
			
			if(oDraggedTextures.Count > 0)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			
				if(Event.current.type == EventType.DragPerform)
				{	
					DragAndDrop.AcceptDrag();					
					
					// Drop
					List<GameObject> oCreatedGameObjects = new List<GameObject>();
					foreach(Texture2D rTexture in oDraggedTextures)
					{
						oCreatedGameObjects.Add(CreateSprite(rTexture, ComputeDropPositionWorld(a_rSceneView), Event.current.alt));
					}
					
					// Update editor selection
			 		Selection.objects = oCreatedGameObjects.ToArray();	
				}
	
				Event.current.Use();
			}
	    }
    }
	
	// Compute the drop position world
	private Vector3 ComputeDropPositionWorld(SceneView a_rSceneView)
	{
		// compute mouse position on the world y=0 plane
		float fOffsetY = 30.0f;
        Ray mouseRay = a_rSceneView.camera.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - fOffsetY, 0.0f));
		
        float t = -mouseRay.origin.z / mouseRay.direction.z;
        Vector3 mouseWorldPos = mouseRay.origin + t * mouseRay.direction;
        mouseWorldPos.z = 0.0f;

       	return mouseWorldPos;
	}
	
	// Compute the drop position world
	private Vector3 ComputeCreateFromButtonPositionWorld(SceneView a_rSceneView)
	{
		// compute mouse position on the world y=0 plane
		float fOffsetY = 30.0f;
        Ray mouseRay = a_rSceneView.camera.ScreenPointToRay(new Vector3(Screen.width  * 0.5f, Screen.width * 0.5f - fOffsetY, 0.0f));
		
        float t = -mouseRay.origin.z / mouseRay.direction.z;
        Vector3 mouseWorldPos = mouseRay.origin + t * mouseRay.direction;
        mouseWorldPos.z = 0.0f;

       	return mouseWorldPos;
	}
	
	private void DisplayBuilder( )
	{
		bool bCreateSprite = false;
		bool bCreatePhysicSprite = false;

		EditorGUILayout.BeginVertical( );
		{
			EditorGUILayout.LabelField(mc_oGUILabelBuildSection);
			
			EditorGUILayout.BeginHorizontal( );
			{
				EditorGUI.BeginDisabledGroup( m_rSelectedObjects == null || m_rSelectedObjects.Length == 0 );
				{
					bCreateSprite = GUILayout.Button( mc_oGUILabelCreateSprite );
					bCreatePhysicSprite = GUILayout.Button( mc_oGUILabelCreatePhysicSprite );
				}
				EditorGUI.EndDisabledGroup( );
			}
			EditorGUILayout.EndHorizontal( );
		}
		EditorGUILayout.EndVertical( );

		if( bCreateSprite == true || bCreatePhysicSprite == true )
		{
			if( m_rSelectedObjects != null )
			{
				List<GameObject> oCreatedGameObjects = new List<GameObject>();
				foreach( Object rObject in m_rSelectedObjects )
				{
					Texture2D rTexture = (Texture2D) rObject;
					
					// Create sprite
					GameObject oSpriteMeshGameObject = CreateSprite(rTexture, ComputeCreateFromButtonPositionWorld(SceneView.lastActiveSceneView), bCreatePhysicSprite);
					
					oCreatedGameObjects.Add(oSpriteMeshGameObject);
				}
				
				// Update editor selection
			 	Selection.objects = oCreatedGameObjects.ToArray();			
				EditorUtility.UnloadUnusedAssets( );
			}
		}
	}
	
	private GameObject CreateSprite(Texture2D a_rTexture, Vector3 a_f3CreationPosition, bool a_bPhysic)
	{
		// Create sprite
		GameObject rSpriteGameObject = CreateSprite(a_rTexture, a_bPhysic);
		rSpriteGameObject.transform.position = a_f3CreationPosition;
		
		return rSpriteGameObject;
	}
	
	private GameObject CreateSprite(Texture2D a_rTexture, bool a_bPhysic)
	{
		if(a_bPhysic)
		{
			// Create sprite with physic
			return Uni2DEditorUtilsSpriteBuilder.CreateSpriteFromTexture(
							a_rTexture,
							m_oVertexColorSprite,
							m_fAlphaCutOffPhysicSprite,
							m_bPolygonizeHolesPhysicSprite,
							m_fPolygonizationAccuracyPhysicSprite,
							m_fExtrusionDepthPhysicSprite,
							m_fScaleSprite,
							m_f2CustomPivotPointSprite,
							m_ePivotPointSprite,
							m_ePhysicModePhysicSprite,
							m_eCollisionTypePhysicSprite,
							m_bIsKinematicPhysicSprite);
		}
		else
		{
			// Create sprite without physic
			return Uni2DEditorUtilsSpriteBuilder.CreateSpriteFromTexture(
				a_rTexture,
				m_oVertexColorSprite,
				m_fAlphaCutOffPhysicSprite,
				m_bPolygonizeHolesPhysicSprite,
				m_fPolygonizationAccuracyPhysicSprite,
				m_fExtrusionDepthPhysicSprite,
				m_fScaleSprite,
				m_f2CustomPivotPointSprite,
				m_ePivotPointSprite,
				m_ePhysicModeSprite, 
				m_eCollisionTypeSprite,
				m_bIsKinematicSprite);
		}
	}

	private void OnHierarchyChange()
	{
		this.UpdateSpriteMeshList();
		this.Repaint();
	}

	private void OnProjectChange()
	{
		this.UpdateSpriteMeshList();
		this.Repaint();
	}

	public void UpdateSpriteMeshList()
	{
		if(ms_bLockListUpdate == false)
		{
			// Current scene
			m_oCurrentSceneSprites.Clear();
			m_oCurrentSceneSprites.AddRange(FindObjectsOfType(typeof(Uni2DSprite)) as Uni2DSprite[]);
			
			// Project
			m_oProjectSprites.Clear();
			foreach(string oAssetPath in AssetDatabase.GetAllAssetPaths())
			{
				GameObject rPrefab = AssetDatabase.LoadAssetAtPath(oAssetPath, typeof(GameObject)) as GameObject;
				if(rPrefab != null)
				{
					Uni2DEditorUtilsSpriteBuilder.GetSpritesInResourceHierarchy(rPrefab.transform, ref m_oProjectSprites);
				}
			}
			
			// Project and current scene
			m_oProjectAndCurrentSceneSprites.Clear();
			m_oProjectAndCurrentSceneSprites.AddRange(m_oProjectSprites);
			m_oProjectAndCurrentSceneSprites.AddRange(m_oCurrentSceneSprites);
		}
	}
	
	private List<Uni2DSprite> GetCurrentDisplayList()
	{
		switch(m_eDisplayModeScope)
		{
			case SpriteLibraryDisplayScope.CurrentScene:
			{
				return m_oCurrentSceneSprites;
			}
			
			case SpriteLibraryDisplayScope.Project:
			{
				return m_oProjectSprites;
			}
			
			case SpriteLibraryDisplayScope.ProjectAndCurrentScene:
			{
				return m_oProjectAndCurrentSceneSprites;
			}
		}
		
		return null;
	}
	
	private bool IsCurrentDisplayedListDirty()
	{
		ms_bLockListUpdate = true;
		bool bIsDirty = IsDirty(GetCurrentDisplayList());
		ms_bLockListUpdate = false;
		
		return bIsDirty;
	}
	
	private void UpdateSpriteWithDirtyPhysicInCurrentDisplayedList()
	{
		ms_bLockListUpdate = true;
		Uni2DEditorUtilsSpriteBuilder.UpdateAllDirtySpritesPhysic(GetCurrentDisplayList());
		ms_bLockListUpdate = false;
	}
	
	private bool DisplayAllMode()
	{
		return m_eDisplayMode == SpriteLibraryDisplay.AllSprites;
	}
	
	private void DisplayLibrary()
	{
		EditorGUILayout.BeginVertical( );
		{
			EditorGUILayout.LabelField(mc_oGUILabelLibrarySection);
			
			m_eDisplayMode = (SpriteLibraryDisplay) EditorGUILayout.EnumPopup("Display", m_eDisplayMode );
			m_eDisplayModeScope = (SpriteLibraryDisplayScope) EditorGUILayout.EnumPopup("Scope", m_eDisplayModeScope );
			
			
			bool bGUIEnabledSaved = GUI.enabled; 
			GUI.enabled = IsCurrentDisplayedListDirty();
			GUILayout.BeginHorizontal();
			{
				if(GUILayout.Button("Update physic for all"))
				{
					UpdateSpriteWithDirtyPhysicInCurrentDisplayedList();
				}
			}
			GUILayout.EndHorizontal();
			GUI.enabled = bGUIEnabledSaved;
			
			m_f2GUIScrollingPosition = EditorGUILayout.BeginScrollView(m_f2GUIScrollingPosition, false, false);
			{
				ms_bLockListUpdate = true;
				foreach(Uni2DSprite rSpriteMeshComponent in GetCurrentDisplayList())
				{
					if(rSpriteMeshComponent == null)
					{
						continue;
					}
					
					bool bIsDirty = rSpriteMeshComponent.isPhysicDirty;
	
					if(DisplayAllMode() || bIsDirty)
					{
						EditorGUILayout.BeginHorizontal( );
						{
							bool bPingObject = GUILayout.Button( rSpriteMeshComponent.name );
	
							// Highlight prefab in project view if the user click on the button
							if( bPingObject == true )
							{
								Selection.activeGameObject = rSpriteMeshComponent.gameObject;
								if(EditorUtility.IsPersistent(rSpriteMeshComponent))
								{
									Uni2DEditorUtils.PingPrefabInProjectView(rSpriteMeshComponent.gameObject);
								}
							}
	
							EditorGUI.BeginDisabledGroup( bIsDirty == false );
							{
								bool bUpdateObject = GUILayout.Button( mc_oGUILabelUpdate, GUILayout.Width( 100.0f ) );
								if( bUpdateObject == true )
								{
									rSpriteMeshComponent.Rebuild();
								}
							}
							EditorGUI.EndDisabledGroup( );
						}
						EditorGUILayout.EndHorizontal( );
					}
				}
			}
			EditorGUILayout.EndScrollView( );
			ms_bLockListUpdate = false;
		}
		EditorGUILayout.EndVertical( );
	}
	
	private bool IsDirty(List<Uni2DSprite> a_rSpriteMeshes)
	{
		foreach(Uni2DSprite rSpriteMesh in a_rSpriteMeshes)
		{
			if(rSpriteMesh.isPhysicDirty)
			{
				return true;
			}
		}
		return false;
	}
	
	// Reset a sprite
	public static void ResetSpriteParameters(	ref Color a_rVertexColor,
												ref float a_fAlphaCutOff,
												ref float a_fPoligonizationAccuracy,
												ref float a_fExtrusionDepth,
												ref float a_fScale,
												ref bool a_bPolygonizeHoles,
												ref Vector2 a_f2CustomPivotPoint,
												ref Uni2DSprite.PivotPointType a_ePivotPoint,
												ref Uni2DSprite.PhysicMode a_ePhysicMode,
												ref Uni2DSprite.CollisionType a_eCollisionType,
												ref bool a_bIsKinematic)
	{
		// Default sprite parameters
		a_rVertexColor = m_oVertexColorSprite;
		a_ePhysicMode = m_ePhysicModeSprite;
		a_eCollisionType = m_eCollisionTypeSprite;
		a_bIsKinematic = m_bIsKinematicSprite;
		a_fScale = m_fScaleSprite;
		a_ePivotPoint = m_ePivotPointSprite;
		a_f2CustomPivotPoint = m_f2CustomPivotPointSprite;
		
		// Default physic sprite parameters
		a_fAlphaCutOff = m_fAlphaCutOffPhysicSprite;
		a_fPoligonizationAccuracy = m_fPolygonizationAccuracyPhysicSprite;
		a_bPolygonizeHoles = m_bPolygonizeHolesPhysicSprite;
		a_fExtrusionDepth = m_fExtrusionDepthPhysicSprite;
	}
}
#endif