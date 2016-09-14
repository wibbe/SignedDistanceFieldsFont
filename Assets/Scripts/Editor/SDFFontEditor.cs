using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SDFFont))]
public class SDFFontEditor : Editor
{
	private SerializedProperty m_Texture;
	private SerializedProperty m_LineHeight;
	private SerializedProperty m_Width;
	private SerializedProperty m_Height;

	private void OnEnable()
	{
		m_Texture = serializedObject.FindProperty("m_Texture");
		m_LineHeight = serializedObject.FindProperty("m_LineHeight");
		m_Width = serializedObject.FindProperty("m_Width");
		m_Height = serializedObject.FindProperty("m_Height");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(m_Texture, new GUIContent("Font Texture"));

		bool enabled = GUI.enabled;
		GUI.enabled = false;
		EditorGUILayout.PropertyField(m_LineHeight, new GUIContent("Line Height"));
		EditorGUILayout.PropertyField(m_Width, new GUIContent("Width"));
		EditorGUILayout.PropertyField(m_Height, new GUIContent("Height"));
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