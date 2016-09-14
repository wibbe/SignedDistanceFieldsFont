using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

[AddComponentMenu("UI/SDFText", 11)]
public class SDFText : Graphic
{
	[SerializeField] private SDFFont m_Font = null;

	[TextArea(3, 5)]
	[SerializeField] private string m_Text = "";


	public override Texture mainTexture
	{
		get
		{
			if (m_Font == null)
			{
				if (material != null && material.mainTexture != null)
				{
					return material.mainTexture;
				}
				return s_WhiteTexture;
			}

			return m_Font.texture;
		}
	}

	public string text
	{
		get {
			return m_Text;
		}
		set {
			if (String.IsNullOrEmpty(value))
			{
				if (String.IsNullOrEmpty(m_Text))
					return;
				m_Text = "";
				SetVerticesDirty();
			}
			else if (m_Text != value)
			{
				m_Text = value;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public float pixelsPerUnit
	{
		get
		{
			var localCanvas = canvas;
			if (localCanvas == null)
				return 1f;
			
			return localCanvas.scaleFactor;
		}
	}

	readonly private UIVertex[] m_TempVerts = new UIVertex[4];
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		Vector2 pivot = rectTransform.pivot;
		Rect inputRect = rectTransform.rect;

		float unitsPerPixel = 1f / pixelsPerUnit;

		//Vector3 start = new Vector3((0f - pivot.x) * inputRect.width, (0f - pivot.y) * inputRect.height, 0f);
		//Vector3 start = new Vector3(0f, 0f, 0f);
		//Vector3 end = new Vector3(10f * unitsPerPixel, 10f * unitsPerPixel, 0f);

		vh.Clear();

		int pos = 0;
		Color textColor = color;

		for (int i = 0; i < m_Text.Length; ++i)
		{
			char c = m_Text[i];
			SDFFont.Character ch = m_Font.GetCharacter(c);

			Vector3 lowerLeft = new Vector3(pos + ch.xoffset, m_Font.baseLine - ch.yoffset - ch.height, 0f);
			Vector3 topRight = new Vector3(pos + ch.xoffset + ch.width, m_Font.baseLine - ch.yoffset, 0f);

			Debug.LogFormat("char({0}) x:{1} y:{2} xo:{3} yo:{4} width:{5} height:{6} advance:{7}", c, lowerLeft.x, lowerLeft.y, ch.xoffset, ch.yoffset, ch.width, ch.height, ch.advance);

			int x = ch.x;
			int y = m_Font.texture.height - ch.y;
			float u0 = (float)x / (float)m_Font.texture.width;
			float u1 = (float)(x + ch.width) / (float)m_Font.texture.width;
			float v0 = (float)(y - ch.height) / (float)m_Font.texture.height;
			float v1 = (float)y / (float)m_Font.texture.height;

			m_TempVerts[0].position = new Vector3(lowerLeft.x, lowerLeft.y) * unitsPerPixel;
			m_TempVerts[0].color = Color.red;
			m_TempVerts[0].uv0 = new Vector2(u0, v0);
			m_TempVerts[1].position = new Vector3(topRight.x, lowerLeft.y) * unitsPerPixel;
			m_TempVerts[1].color = textColor;
			m_TempVerts[1].uv0 = new Vector2(u1, v0);
			m_TempVerts[2].position = new Vector3(topRight.x, topRight.y) * unitsPerPixel;
			m_TempVerts[2].color = textColor;
			m_TempVerts[2].uv0 = new Vector2(u1, v1);
			m_TempVerts[3].position = new Vector3(lowerLeft.x, topRight.y) * unitsPerPixel;
			m_TempVerts[3].color = textColor;
			m_TempVerts[3].uv0 = new Vector2(u0, v1);
			
			vh.AddUIVertexQuad(m_TempVerts);

			pos += ch.advance;
		}

	}
}
