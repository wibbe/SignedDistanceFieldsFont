using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SDFFont))]
public class SDFFontInspector : Editor
{
	private SerializedProperty m_Texture;
	private SerializedProperty m_LineHeight;

	private void OnEnable()
	{
		m_Texture = serializedObject.FindProperty("m_Texture");
		m_LineHeight = serializedObject.FindProperty("m_LineHeight");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(m_Texture, new GUIContent("Font Texture"));

		bool enabled = GUI.enabled;
		GUI.enabled = false;
		EditorGUILayout.PropertyField(m_LineHeight, new GUIContent("Line Height"));
		GUI.enabled = enabled;

		EditorGUILayout.Space();
		if (GUILayout.Button("Import Font"))
		{
			string file = EditorUtility.OpenFilePanel("Font to import", "", "fnt");
			if (!string.IsNullOrEmpty(file))
				(target as SDFFont).ImportFontDefinition(file);
		}

		serializedObject.ApplyModifiedProperties();
	}
}