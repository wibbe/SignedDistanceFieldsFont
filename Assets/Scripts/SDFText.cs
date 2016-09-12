using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

[AddComponentMenu("UI/SDFText", 11)]
public class SDFText : Graphic
{
	[SerializeField] private SDFFont m_Font = null;

	[TextArea(3, 5), SerializeField]
	private string m_Text = "";


	public string text
	{
		get {
			return m_Text;
		}
		set {
			m_Text = value;
		}
	}

	readonly private UIVertex[] m_TempVerts = new UIVertex[4];
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		Vector2 pivot = rectTransform.pivot;
		Rect inputRect = rectTransform.rect;

		Vector3 start = new Vector3((0f - pivot.x) * inputRect.width, (0f - pivot.y) * inputRect.height, 0f);
		Vector3 end = new Vector3((1f - pivot.x) * inputRect.width, (1f - pivot.y) * inputRect.height, 0f);

		vh.Clear();

		m_TempVerts[0].position = new Vector3(start.x, start.y);
		m_TempVerts[0].color = Color.yellow;
		m_TempVerts[1].position = new Vector3(end.x, start.y);
		m_TempVerts[1].color = Color.blue;
		m_TempVerts[2].position = new Vector3(end.x, end.y);
		m_TempVerts[2].color = Color.red;
		m_TempVerts[3].position = new Vector3(start.x, end.y);
		m_TempVerts[3].color = Color.red;

		vh.AddUIVertexQuad(m_TempVerts);
	}
}
