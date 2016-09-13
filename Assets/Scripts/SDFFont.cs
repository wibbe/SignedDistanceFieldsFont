using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;


[CreateAssetMenuAttribute(order = 2200)]
public class SDFFont : ScriptableObject
{
	[Serializable]
	public struct Character
	{
		public static Character identity;

		public char id;
		public int x;
		public int y;
		public int width;
		public int height;
		public int xoffset;
		public int yoffset;
		public int advance;
	}

	[SerializeField] private Texture2D m_Texture = null;
	[SerializeField] private Character[] m_Characters = null;
	[SerializeField] private int m_LineHeight = 0;
	[SerializeField] private int m_Base = 0;
	
	private Dictionary<char, Character> m_CharDict = new Dictionary<char, Character>();


	public int lineHeight { get { return m_LineHeight; } }
	public int baseLine { get { return m_Base; } }
	public Texture2D texture { get { return m_Texture; } }


	public Character GetCharacter(char ch)
	{
		Character value;
		if (m_CharDict.TryGetValue(ch, out value))
			return value;
		return Character.identity;
	}

	private void OnEnable()
	{
		m_CharDict = new Dictionary<char, Character>();
		foreach (var ch in m_Characters)
			m_CharDict.Add(ch.id, ch);
	}

	#if UNITY_EDITOR
	private int ReadInt(string value)
	{
		int val;
		if (int.TryParse(value, out val))
			return val;
		return -1;
	}

	public void ImportFontDefinition(string filename)
	{
		Debug.LogFormat("Importing file {0}", filename);

		List<Character> chars = new List<Character>();

		StreamReader stream = new StreamReader(filename);
		string line;
		while ((line = stream.ReadLine()) != null)
		{
			string[] items = line.Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);

			if (items.Length > 0)
			{
				// We are only interested in the char items
				if (items[0] == "char")
				{
					int id = -1, x = -1, y = -1, w = -1, h = -1, xo = -1, yo = -1, a = -1;

					for (int i = 1; i < items.Length; i += 2)
					{
						bool hasData = i < (items.Length - 1);

						if (items[i] == "id" && hasData)
							id = ReadInt(items[i + 1]);
						else if (items[i] == "x" && hasData)
							x = ReadInt(items[i + 1]);
						else if (items[i] == "y" && hasData)
							y = ReadInt(items[i + 1]);
						else if (items[i] == "width" && hasData)
							w = ReadInt(items[i + 1]);
						else if (items[i] == "height" && hasData)
							h = ReadInt(items[i + 1]);
						else if (items[i] == "xoffset" && hasData)
							xo = ReadInt(items[i + 1]);
						else if (items[i] == "yoffset" && hasData)
							yo = ReadInt(items[i + 1]);
						else if (items[i] == "xadvance" && hasData)
							a = ReadInt(items[i + 1]);
					}

					if (id != -1 && x != -1 && y != -1 && w != -1 && h != -1 && xo != -1 && yo != -1 && a != -1)
					{
						string c = Char.ConvertFromUtf32(id);
						if (c.Length != 1)
							continue;
					
						Character ch = new Character();
						ch.id = c[0];
						ch.x = x;
						ch.y = y;
						ch.width = w;
						ch.height = h;
						ch.xoffset = xo;
						ch.yoffset = yo;
						ch.advance = a;
						chars.Add(ch);
					}
				}
				else if (items[0] == "common")
				{
					for (int i = 1; i < items.Length; i += 2)
					{
						bool hasData = i < (items.Length - 1);

						if (items[i] == "lineHeight" && hasData)
							m_LineHeight = ReadInt(items[i + 1]);
						else if (items[i] == "base" && hasData)
							m_Base = ReadInt(items[i + 1]);
					}
				}
			}

			if (chars.Count > 0)
			{
				m_Characters = chars.ToArray();
			}
			else
			{
				m_Characters = new Character[0];
				Debug.LogWarning("Did not find any character definitions in the file");
			}
		}
	}
	#endif
}
