using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Uni2DTextureAtlas))]
public class Uni2DTextureAtlasInspector : Editor 
{
	// Generated data fold out?
	private static bool ms_bGeneratedDataFoldout = true;
	
	// On enable
	private void OnEnable()
	{
		OnInitInspector();
	}
	
	// On destroy
	public void OnDestroy()
	{
		OnLeaveInspector();
	}
	
	// On inspector gui
	public override void OnInspectorGUI()
	{	
		Uni2DTextureAtlas rAtlas = target as Uni2DTextureAtlas;
		
		EditorGUIUtility.LookLikeInspector();
		
		DrawDefaultInspector();
		
		EditorGUI.indentLevel = -1;
		ms_bGeneratedDataFoldout = EditorGUILayout.Foldout(ms_bGeneratedDataFoldout, "GeneratedData");
		if(ms_bGeneratedDataFoldout)
		{
			EditorGUI.indentLevel = 1;
			{	
				EditorGUILayout.ObjectField("Atlas Texture", rAtlas.atlasTexture, typeof(Texture2D), false);
				EditorGUILayout.ObjectField("Atlas Material", rAtlas.atlasMaterial, typeof(Material), false);
			}
			EditorGUI.indentLevel = 0;
		}
		
		bool bUnappliedSettings = rAtlas.UnappliedSettings;
		EditorGUI.BeginDisabledGroup(bUnappliedSettings == false);
		{	
			// Apply/Revert
			EditorGUILayout.BeginHorizontal( );
			{
				if(GUILayout.Button("Apply"))
				{
					rAtlas.ApplySettings();
				}
				
				if(GUILayout.Button("Revert"))
				{
					rAtlas.RevertSettings();
				}
			}
			EditorGUILayout.EndHorizontal( );
		}
		EditorGUI.EndDisabledGroup();
		
		EditorGUI.BeginDisabledGroup(bUnappliedSettings);
		{
			// Generate
			if(GUILayout.Button("Force atlas regeneration"))
			{
				rAtlas.Generate();
			}
		}
		EditorGUI.EndDisabledGroup();
	}
	
	// On init inspector
	private void OnInitInspector()
	{
		Uni2DTextureAtlas rAtlas = target as Uni2DTextureAtlas;

		rAtlas.RevertSettings();
	}
	
	// On leave inspector
	private void OnLeaveInspector()
	{	
		Uni2DTextureAtlas rAtlas = target as Uni2DTextureAtlas;
		
		if(rAtlas.UnappliedSettings)
		{
			string oGUIDialogMessage = string.Format("Unapplied settings for '{0}'", target.name);
			bool bApply = EditorUtility.DisplayDialog( "Unapplied sprite settings",
				oGUIDialogMessage,
				"Apply",
				"Revert" );

			if(bApply)
			{
				rAtlas.ApplySettings();
			}
			else
			{
				rAtlas.RevertSettings();
			}
		}
	}	
}
