using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SDFFont))]
public class SDFFontInspector : Editor
{
	private SerializedProperty m_texture;

	private void OnEnable()
	{
		m_texture = serializedObject.FindProperty("m_Texture");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(m_texture, new GUIContent("Font Texture"));

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