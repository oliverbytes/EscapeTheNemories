using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor( typeof( Uni2DSprite ) )]
public class Uni2DEditorSpriteInspector : Editor 
{
	private static string ms_GUIWarningMessagePrefabEdition = "Prefab edition is restricted.\nOnly in-scene sprite objects can be edited.";
	private static string ms_GUIWarningMessageInAnimationModeEdition = "Sprite can't be edited in animation mode.";
	//private static string ms_GUIWarningMessageSettingsChanged = "Settings have been changed.\nPress \"Apply\" to save them.";

	private static string ms_GUIDialogTitleUnappliedSettings = "Unapplied sprite settings";
	private static string ms_GUIDialogMessageUnappliedSettings = "Unapplied settings for '{0}'";
	private static string ms_GUIDialogOKUnappliedSettings = "Apply";
	private static string ms_GUIDialogCancelUnappliedSettings = "Revert";

	private static string ms_GUIButtonLabelOK = "Apply";
	private static string ms_GUIButtonLabelCancel = "Revert";

	private static string ms_GUILabelSpriteTexture = "Sprite Texture";

	private static string ms_GUILabelInfoTriangleCount = "Collider Triangles";
	
	private const string mc_GUISelectAtlasLabel = "Select";
	private const int mc_GUISelectAtlasWidth = 80;

	private static bool ms_bGUIFoldoutColliderSettings = false;
	
	private bool ms_bIgnoreInspectorChanges;
	
	private void OnEnable( )
	{
		this.InitInspector( );
	}
	
	public void OnDestroy( )
	{
		OnLeaveInspector();
	}

	private void InitInspector()
	{
		Uni2DSprite rSpriteMeshComponent = (Uni2DSprite) target;
		Uni2DEditorSpriteMeshInspectorParameters rSpriteMeshInspectorParameters = rSpriteMeshComponent.SpriteMeshInspectorParameter;

		rSpriteMeshInspectorParameters.OnInitInspector(rSpriteMeshComponent);
	}
	
	private void OnLeaveInspector()
	{
		if(ms_bIgnoreInspectorChanges)
		{
			return;
		}
		
		Uni2DSprite rSpriteMeshComponent = (Uni2DSprite) target;
		Uni2DEditorSpriteMeshInspectorParameters rSpriteMeshInspectorParameters = rSpriteMeshComponent.SpriteMeshInspectorParameter;
		
		if( rSpriteMeshInspectorParameters.settingsChanged == true )
		{
			string oGUIDialogMessage = string.Format( ms_GUIDialogMessageUnappliedSettings, target.name );
			bool bApply = EditorUtility.DisplayDialog( ms_GUIDialogTitleUnappliedSettings,
				oGUIDialogMessage,
				ms_GUIDialogOKUnappliedSettings,
				ms_GUIDialogCancelUnappliedSettings );

			rSpriteMeshInspectorParameters.settingsChanged = false;

			if( bApply == true )
			{
				if(rSpriteMeshInspectorParameters.ApplySettings(rSpriteMeshComponent) == false)
				{
					Uni2DEditorUtilsSpriteBuilder.DisplayNoTextureSoWeDontCreateWarning(rSpriteMeshComponent.gameObject);
				}
			}
		}
	}
	
	public override void OnInspectorGUI()
	{
		Uni2DSprite rSpriteMeshComponent = (Uni2DSprite) target;
		Uni2DEditorSpriteMeshInspectorParameters rSpriteMeshInspectorParameters = rSpriteMeshComponent.SpriteMeshInspectorParameter;
		
		bool bIsPersistent = EditorUtility.IsPersistent(rSpriteMeshComponent);
		bool bIsInAnimationMode = AnimationUtility.InAnimationMode();
			
		// Message not editable in animation mode
		if(bIsInAnimationMode)
		{
			EditorGUILayout.BeginVertical( );
			{
				EditorGUILayout.HelpBox( ms_GUIWarningMessageInAnimationModeEdition, MessageType.Warning, true );
			}
			EditorGUILayout.EndVertical( );
		}
		
		// Update physic?
		EditorGUI.BeginDisabledGroup(bIsInAnimationMode);
		{
			string oUpdatePhysicButtonLabel = "Force Update";
			if(rSpriteMeshComponent.isPhysicDirty)
			{
				EditorGUILayout.HelpBox("The texture has changed since the last physic computation. Press the \"Update Physic\" button to update the physic shape.", MessageType.Warning, true);
				oUpdatePhysicButtonLabel = "Update Physic";
			}
			if(GUILayout.Button(oUpdatePhysicButtonLabel))
			{
				rSpriteMeshComponent.Rebuild();
			}
		}
		
		// Message not editable in resource
		if(bIsPersistent && bIsInAnimationMode == false)
		{
			EditorGUILayout.BeginVertical();
			{
				EditorGUILayout.HelpBox(ms_GUIWarningMessagePrefabEdition, MessageType.Warning, true);
			}
			EditorGUILayout.EndVertical();
		}
		
		bool bNotEditable = bIsPersistent || bIsInAnimationMode;
		EditorGUI.BeginDisabledGroup(bNotEditable);
		{
			bool bApplySettings;
			bool bRevertSettings;
			
			// Temporary changed parameters
			
			// Need apply
			Texture2D rSpriteTexture = rSpriteMeshInspectorParameters.spriteTexture;
			Uni2DSprite.PhysicMode ePhysicMode = rSpriteMeshInspectorParameters.physicMode;
			Uni2DSprite.CollisionType eCollisionType = rSpriteMeshInspectorParameters.collisionType;
			float fAlphaCutOff = rSpriteMeshInspectorParameters.alphaCutOff;
			float fPolygonizationAccuracy = rSpriteMeshInspectorParameters.polygonizationAccuracy;
			bool bPolygonizeHoles = rSpriteMeshInspectorParameters.polygonizeHoles;
			
			// Interactive
			Uni2DSprite.PivotPointType ePivotPointType = rSpriteMeshInspectorParameters.pivotPointType;
			Vector2 f2PivotPoint = rSpriteMeshInspectorParameters.pivotPointCoords;
			float fScale = rSpriteMeshInspectorParameters.spriteScale;
			float fExtrusionDepth = rSpriteMeshInspectorParameters.extrusionDepth;
			Uni2DTextureAtlas textureAtlas = rSpriteMeshInspectorParameters.textureAtlas;
			Color vertexColor = rSpriteMeshComponent.VertexColor;
			bool bIsKinematic = rSpriteMeshComponent.isKinematic;
	
			EditorGUILayout.BeginVertical( );
			{
				EditorGUILayout.HelpBox( GetHelpMessage(ePhysicMode, eCollisionType), MessageType.Info, true );
	
				/*if( m_bSettingsChanged == true )
				{
					EditorGUILayout.HelpBox( ms_GUIWarningMessageSettingsChanged, MessageType.Warning, true );
				}*/
				
				EditorGUILayout.Space( );
	
				rSpriteTexture = (Texture2D) EditorGUILayout.ObjectField( ms_GUILabelSpriteTexture, rSpriteMeshInspectorParameters.spriteTexture, typeof( Texture2D ), false );
				
				EditorGUILayout.BeginHorizontal();
				{
					textureAtlas = DisplayAtlasPopUp(rSpriteMeshInspectorParameters, bNotEditable);
					EditorGUI.BeginDisabledGroup(textureAtlas == null);
					{
						if(GUILayout.Button(mc_GUISelectAtlasLabel, GUILayout.Width(mc_GUISelectAtlasWidth)))
						{
							EditorGUIUtility.PingObject(textureAtlas);
						}
					}
					EditorGUI.EndDisabledGroup();
				}
				EditorGUILayout.EndHorizontal();
				
				vertexColor = EditorGUILayout.ColorField("Vertex Color", vertexColor);
				
				EditorGUILayout.LabelField( ms_GUILabelInfoTriangleCount, rSpriteMeshInspectorParameters.colliderTriangleCount.ToString( ) );
				
				Uni2DEditorUtilsSpriteBuilder.DisplaySpriteBuilderGUI(
					ref ePhysicMode,
					ref eCollisionType,
					ref bIsKinematic,
					ref fAlphaCutOff,
					ref fPolygonizationAccuracy,
					ref fExtrusionDepth,
					ref fScale,
					ref bPolygonizeHoles,
					ref ePivotPointType,
					ref f2PivotPoint,
					ref ms_bGUIFoldoutColliderSettings );
				
				EditorGUILayout.Space( );
	
				EditorGUILayout.BeginHorizontal( );
				{
					EditorGUI.BeginDisabledGroup( rSpriteMeshInspectorParameters.settingsChanged == false );
					{
						bApplySettings = GUILayout.Button( ms_GUIButtonLabelOK );
						bRevertSettings = GUILayout.Button( ms_GUIButtonLabelCancel );
					}
					EditorGUI.EndDisabledGroup( );
				}
				EditorGUILayout.EndHorizontal( );
			}
			EditorGUILayout.EndVertical( );
	
			EditorGUILayout.Space( );
	
			rSpriteMeshInspectorParameters.settingsChanged = rSpriteMeshInspectorParameters.settingsChanged ||
				(rSpriteMeshInspectorParameters.physicMode != ePhysicMode
				|| rSpriteMeshInspectorParameters.collisionType != eCollisionType
				|| rSpriteMeshInspectorParameters.alphaCutOff != fAlphaCutOff 
				|| rSpriteMeshInspectorParameters.polygonizationAccuracy != fPolygonizationAccuracy || rSpriteMeshInspectorParameters.polygonizeHoles != bPolygonizeHoles 
				|| rSpriteMeshInspectorParameters.spriteTexture != rSpriteTexture);
	
			if( bApplySettings == true )
			{
				if(rSpriteMeshInspectorParameters.ApplySettings(rSpriteMeshComponent) == false)
				{
					Uni2DEditorUtilsSpriteBuilder.DisplayNoTextureWarning(rSpriteMeshComponent.gameObject);
				}
			}
	
			if( bRevertSettings == true )
			{
				rSpriteMeshInspectorParameters.settingsChanged = false;
				this.InitInspector( );
				return;
			}
			
			// Apply
			rSpriteMeshInspectorParameters.physicMode = ePhysicMode;
			rSpriteMeshInspectorParameters.collisionType = eCollisionType;
			rSpriteMeshInspectorParameters.alphaCutOff = fAlphaCutOff;
			rSpriteMeshInspectorParameters.polygonizationAccuracy = fPolygonizationAccuracy;
			rSpriteMeshInspectorParameters.polygonizeHoles = bPolygonizeHoles;
			rSpriteMeshInspectorParameters.spriteTexture = rSpriteTexture;
			
			// Update the is kinematic settings
			if(bIsKinematic != rSpriteMeshInspectorParameters.isKinematic)
			{		
				Rigidbody rRigidbody = rSpriteMeshComponent.GetComponent<Rigidbody>();
				if(rRigidbody != null)
				{
					rRigidbody.isKinematic = bIsKinematic;
				}
				
				rSpriteMeshComponent.isKinematic = bIsKinematic;
				rSpriteMeshInspectorParameters.isKinematic = bIsKinematic;
			}
			
			
			// Don't update interactivly the atlasing if the texture has changed
			bool bUpdateAtlasingInteractivly = (rSpriteMeshInspectorParameters.spriteTexture == rSpriteMeshComponent.spriteTexture);
			if(bUpdateAtlasingInteractivly == false)
			{
				rSpriteMeshInspectorParameters.textureAtlas = textureAtlas;
				textureAtlas = rSpriteMeshComponent.textureAtlas;
			}
			
			if( ePivotPointType != rSpriteMeshInspectorParameters.pivotPointType || f2PivotPoint != rSpriteMeshInspectorParameters.pivotPointCoords 
				|| fScale != rSpriteMeshInspectorParameters.spriteScale || fExtrusionDepth != rSpriteMeshInspectorParameters.extrusionDepth
				|| (bUpdateAtlasingInteractivly && textureAtlas != rSpriteMeshInspectorParameters.textureAtlas)
				|| vertexColor != rSpriteMeshInspectorParameters.vertexColor)
			{
				Uni2DSprite rSpriteComponent = (Uni2DSprite) target;
				
				Material rRendererMaterial = null;
				if(rSpriteMeshComponent.renderer != null)
				{
					rRendererMaterial = rSpriteMeshComponent.renderer.sharedMaterial;
				}
						
				Uni2DEditorUtilsSpriteBuilder.UpdateSpriteInteractiveParameters( (Uni2DSprite) target, fScale, fExtrusionDepth, ePivotPointType, f2PivotPoint, textureAtlas, vertexColor);
				
				// Patch the fact that when the renderer change
				// The inspector is reseted
				if(rSpriteMeshComponent.renderer != null)
				{
					if(rSpriteMeshComponent.renderer.sharedMaterial != rRendererMaterial)
					{
						ms_bIgnoreInspectorChanges = true;
					}
				}
				
				rSpriteMeshInspectorParameters.pivotPointType = rSpriteComponent.pivotPointType;
				rSpriteMeshInspectorParameters.pivotPointCoords = rSpriteComponent.pivotPointCoords;
				rSpriteMeshInspectorParameters.spriteScale = fScale;
				rSpriteMeshInspectorParameters.extrusionDepth = fExtrusionDepth;
				if(bUpdateAtlasingInteractivly)
				{
					rSpriteMeshInspectorParameters.textureAtlas = textureAtlas;
				}
				rSpriteMeshInspectorParameters.vertexColor = vertexColor;
			}
		}
		EditorGUI.EndDisabledGroup();
	}
	
	// Get help message
	private string GetHelpMessage(Uni2DSprite.PhysicMode a_ePhysicMode, Uni2DSprite.CollisionType a_eCollisionType)
	{
		string oHelpMessage = "";
		if(a_ePhysicMode == Uni2DSprite.PhysicMode.NoPhysic)
		{
			oHelpMessage = "In no physic mode, there is no collider attached to the sprite.";
		}
		else if(a_ePhysicMode == Uni2DSprite.PhysicMode.Static)
		{
			if(a_eCollisionType == Uni2DSprite.CollisionType.Convex)
			{
				oHelpMessage = "In static convex mode, the mesh collider does not respond to collisions (e.g. not a rigidbody) as a convex mesh.\n" +
						"Unity computes a convex hull if the mesh collider is not convex.";
			}
			else if(a_eCollisionType == Uni2DSprite.CollisionType.Concave)
			{
				oHelpMessage = "In static concave mode, mesh collider does not respond to collisions (e.g. not a rigidbody) as a concave mesh.\n" +
						"A mesh collider marked as concave only interacts with primitive colliders (boxes, spheres...) and convex meshes.";
			}
			else if(a_eCollisionType == Uni2DSprite.CollisionType.Compound)
			{
				oHelpMessage = "In static compound mode, mesh collider does not respond to collisions (e.g. not a rigidbody) as a concave mesh composed of small convex meshes.\n" +
						"It allows the collider to block any other collider at the expense of performances.";
			}
		}
		else if(a_ePhysicMode == Uni2DSprite.PhysicMode.Dynamic)
		{
			if(a_eCollisionType == Uni2DSprite.CollisionType.Convex)
			{
				oHelpMessage = "In dynamic convex mode, mesh collider does respond to collisions (e.g. rigidbody) as a convex mesh.\n" +
						"Unity computes a convex hull if the mesh collider is not convex.";
			}
			else if(a_eCollisionType == Uni2DSprite.CollisionType.Concave)
			{
				oHelpMessage = "In dynamic concave mode, mesh collider does respond to collisions (e.g. rigidbody) as a concave mesh.\n" +
						"A mesh collider marked as concave only interacts with primitive colliders (boxes, spheres...).";
			}
			else if(a_eCollisionType == Uni2DSprite.CollisionType.Compound)
			{
				oHelpMessage = "In dynamic compound mode, mesh collider does respond to collisions (e.g. rigidbody) as a concave mesh composed of small convex meshes.\n" +
						"It allows the collider to interact with any other collider at the expense of performances.";
			}
		}
		
		return oHelpMessage;
	}
	
	// Fill selectable atlas names
	private Uni2DTextureAtlas DisplayAtlasPopUp(Uni2DEditorSpriteMeshInspectorParameters a_rSpriteInspectorParameters, bool a_bNotEditable)
	{
		if(a_bNotEditable)
		{
			Uni2DTextureAtlas rAtlas = a_rSpriteInspectorParameters.textureAtlas;
			string oAtlasName = "None";
			if(rAtlas != null)
			{
				oAtlasName = rAtlas.name;
			}
			EditorGUILayout.Popup("Atlas", 0, new string[1]{oAtlasName});
			return rAtlas;
		}
		
		List<Uni2DTextureAtlas> oSelectableAtlases = new List<Uni2DTextureAtlas>();
		string[] oSelectableAtlasNames;
		int iSelectedAtlasIndex;
		
		// Find the selectable atlas
		bool bCurrentSelectedAtlasAvailable = (a_rSpriteInspectorParameters.textureAtlas == null);
		foreach(string rAssetPath in AssetDatabase.GetAllAssetPaths())
		{
			GameObject rPrefab = AssetDatabase.LoadAssetAtPath(rAssetPath, typeof(GameObject)) as GameObject;
			if(rPrefab != null)
			{
				Uni2DTextureAtlas rAtlas = rPrefab.GetComponent<Uni2DTextureAtlas>();
				if(rAtlas != null)
				{
					if(rAtlas.Contains(a_rSpriteInspectorParameters.spriteTexture))
					{
						bCurrentSelectedAtlasAvailable = bCurrentSelectedAtlasAvailable || (a_rSpriteInspectorParameters.textureAtlas == rAtlas);
						oSelectableAtlases.Add(rAtlas);
					}
				}
			}
		}
		
		oSelectableAtlases.Add(null);
		oSelectableAtlases.Sort();
		
		// Default atlas selection
		if(bCurrentSelectedAtlasAvailable == false)
		{
			//Debug.Log("Not available");
			a_rSpriteInspectorParameters.textureAtlas = Uni2DEditorUtils.FindFirstTextureAtlas(a_rSpriteInspectorParameters.spriteTexture);
		}
		
		// Fill the selectable Atlas Names
		oSelectableAtlasNames = new string[oSelectableAtlases.Count];
		Dictionary<string, int> oNamesOccurrences = new Dictionary<string, int>();
		int iTextureAtlasIndex = 0;
		iSelectedAtlasIndex = -1;
		foreach(Uni2DTextureAtlas rTextureAtlas in oSelectableAtlases)
		{
			if(rTextureAtlas == a_rSpriteInspectorParameters.textureAtlas)
			{
				iSelectedAtlasIndex = iTextureAtlasIndex;
			}
			
			string oAtlasName;
			if(rTextureAtlas == null)
			{
				oAtlasName = "None";
			}
			else
			{
				oAtlasName = rTextureAtlas.gameObject.name;
			}
	
			// Get the atlas name occurence count
			int iAtlasNameOccurenceCount;
			if(oNamesOccurrences.TryGetValue(oAtlasName, out iAtlasNameOccurenceCount))
			{
				iAtlasNameOccurenceCount++;
				oNamesOccurrences[oAtlasName] = iAtlasNameOccurenceCount;
			}
			else
			{
				iAtlasNameOccurenceCount = 1;
				oNamesOccurrences.Add(oAtlasName, iAtlasNameOccurenceCount);
			}
			
			// Set the names
			if(iAtlasNameOccurenceCount > 1)
			{
				oAtlasName += " (" + iAtlasNameOccurenceCount + ")";
			}
			oSelectableAtlasNames[iTextureAtlasIndex] = oAtlasName;
			
			iTextureAtlasIndex++;
		}
		
		// Display pop up
		iSelectedAtlasIndex = EditorGUILayout.Popup("Atlas", iSelectedAtlasIndex, oSelectableAtlasNames);
		
		// Return the pop up
		return oSelectableAtlases[iSelectedAtlasIndex];
	}
}
