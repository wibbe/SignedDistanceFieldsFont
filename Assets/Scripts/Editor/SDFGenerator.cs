using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

public class SDFGenerator : EditorWindow
{
	private struct Point
	{
		public int dx;
		public int dy;

		public Point(int x, int y)
		{
			dx = x;
			dy = y;
		}

		public int DistSq()
		{
			return dx * dx + dy * dy;
		}

		public static Point inside = new Point(0, 0);
		public static Point empty = new Point(9999, 9999);
	}


	private Texture2D m_SourceTexture = null;
	private int m_GridWidth = 0;
	private int m_GridHeight = 0;

	[MenuItem("Window/SDF Generator")]
	public static void OpenWindow()
	{
		EditorWindow.GetWindow<SDFGenerator>(true, "SDF Generator");
	}

	private void OnGUI()
	{
		m_SourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", m_SourceTexture, typeof(Texture2D), false);

		var enabled = GUI.enabled;
		if (m_SourceTexture == null)
			GUI.enabled = false;

		EditorGUILayout.Space();
		if (GUILayout.Button("Generate"))
			Generate();

		GUI.enabled = enabled;
	}

	private void Generate()
	{
		m_GridWidth = m_SourceTexture.width + 2;
		m_GridHeight = m_SourceTexture.height + 2;
		Point[] grid1 = new Point[m_GridWidth * m_GridHeight];
		Point[] grid2 = new Point[m_GridWidth * m_GridHeight];

		Color32[] pixels = m_SourceTexture.GetPixels32();

		for (int y = 0; y < m_GridWidth; y++)
			for (int x = 0; x < m_GridHeight; x++)
			{
				int gIdx = y * m_GridWidth + x;
				int sIdx = (y - 1) * m_SourceTexture.width + (x - 1);
				if (x == 0 || y == 0 || x == (m_GridWidth - 1) || y == (m_GridHeight - 1))
				{
					grid1[gIdx] = Point.empty;
					grid2[gIdx] = Point.empty;
				}
				else
				{
					if (pixels[sIdx].r < 128)
					{
						grid1[gIdx] = Point.inside;
						grid2[gIdx] = Point.empty;
					}
					else
					{
						grid1[gIdx] = Point.empty;
						grid2[gIdx] = Point.inside;
					}
				}
			}
						
		GenerateSDF(grid1);
		GenerateSDF(grid2);

		string filePath = EditorUtility.SaveFilePanel("Save Signed Distance Field", 
		                                              new FileInfo(AssetDatabase.GetAssetPath(m_SourceTexture)).DirectoryName,
													  m_SourceTexture.name + " SDF", 
		                                              "png");
		if (!string.IsNullOrEmpty(filePath))
		{
			// Generate final texture
			pixels = new Color32[m_SourceTexture.width * m_SourceTexture.height];
			for (int y = 0; y < m_SourceTexture.height; y++)
				for (int x = 0; x < m_SourceTexture.width; x++)
				{
					int dist1 = (int)Mathf.Sqrt((float)Get(grid1, x + 1, y + 1).DistSq());
					int dist2 = (int)Mathf.Sqrt((float)Get(grid2, x + 1, y + 1).DistSq());
					int dist = Mathf.Clamp(128 + (dist1 - dist2), 0, 255);

					pixels[y * m_SourceTexture.width + x] = new Color32((byte)dist, (byte)dist, (byte)dist, 255);
				}

			Texture2D dest = new Texture2D(m_SourceTexture.width, m_SourceTexture.height);
			dest.SetPixels32(pixels);
			dest.Apply();

			File.WriteAllBytes(filePath, dest.EncodeToPNG());
			AssetDatabase.Refresh();

			DestroyImmediate(dest);
		}
	}

	private void GenerateSDF(Point[] grid)
	{
		// Pass 0
		for (int y = 1; y < (m_GridHeight - 1); y++)
		{
			for (int x = 1; x < (m_GridWidth - 1); x++)
			{
				Point p = Get(grid, x, y);
				p = Compare(grid, p, x, y, -1,  0);
				p = Compare(grid, p, x, y,  0, -1);
				p = Compare(grid, p, x, y, -1, -1);
				p = Compare(grid, p, x, y,  1, -1);
				Put(grid, x, y, p);
			}

			for (int x = (m_GridWidth - 2); x >= 1; x--)
			{
				Point p = Get(grid, x, y);
				p = Compare(grid, p, x, y, 1, 0);
				Put(grid, x, y, p);
			}
		}

		// Pass 1
		for (int y = (m_GridHeight - 2); y >= 1; y--)
		{
			for (int x = (m_GridWidth - 2); x >= 0; x--)
			{
				Point p = Get(grid, x, y);
				p = Compare(grid, p, x, y,  1,  0);
				p = Compare(grid, p, x, y,  0,  1);
				p = Compare(grid, p, x, y, -1,  1);
				p = Compare(grid, p, x, y,  1,  1);
				Put(grid, x, y, p);
			}

			for (int x = 1; x < (m_GridWidth - 1); x++)
			{
				Point p = Get(grid, x, y);
				p = Compare(grid, p, x, y, -1, 0);
				Put(grid, x, y, p);
			}
		}
	}

	private Point Get(Point[] grid, int x, int y)
	{
		return grid[y * m_GridWidth + x];
	}

	private void Put(Point[] grid, int x, int y, Point point)
	{
		grid[y * m_GridWidth + x] = point;
	}

	private Point Compare(Point[] grid, Point point, int x, int y, int offsetx, int offsety)
	{
		Point other = Get(grid, x + offsetx, y + offsety);
		other.dx += offsetx;
		other.dy += offsety;
		
		if (other.DistSq() < point.DistSq())
			return other;
		return point;
	}
}
