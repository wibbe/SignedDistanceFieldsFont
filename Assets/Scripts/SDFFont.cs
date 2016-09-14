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
	[SerializeField] private int m_Width = 0;
	[SerializeField] private int m_Height = 0;
	
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
		UpdateDict();
	}

	private void UpdateDict()
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

		byte[] data = File.ReadAllBytes(filename);
		if (data == null || data.Length == 0)
		{
			Debug.LogErrorFormat("Could not load font '{0}'", filename);
			return;
		}

		// Make sure we are reading a BmFont
		if (data.Length < 4 || data[0] != 66 || data[1] != 77 || data[2] != 70 || data[3] != 3)
		{
			Debug.LogError("Not a valid BMFont file");
			return;
		}

		Debug.LogFormat("Read {0} bytes", data.Length);

		// Parse the different blocks
		int pos = 4;
		while (pos < data.Length)
		{
			int blockId = data[pos];
			pos++;

			switch (blockId)
			{
				case 1:	// info block
					pos = SkipBlock(1, data, pos);
					break;

				case 2: // common block
					pos = ReadCommonBlock(data, pos);
					break;

				case 3: // pages block
					pos = SkipBlock(3, data, pos);
					break;

				case 4: // chars block
					pos = ReadCharsBlock(data, pos, chars);
					break;

				case 5: // kerning pairs block
					pos = SkipBlock(5, data, pos);
					break;

				default:
					Debug.LogErrorFormat("Encountered unknown block typ {0}, aborting", blockId);
					pos = data.Length + 1;
					break;
			}
		}

		Debug.LogFormat("Read {0} charaters from the font", chars.Count);

		if (chars.Count > 0)
			m_Characters = chars.ToArray();
		else
			m_Characters = new Character[0];

		UpdateDict();
	}

	private int SkipBlock(int blockId, byte[] data, int pos)
	{
		// Make sure we can read the block size
		if ((pos + 4) >= data.Length)
		{
			Debug.LogErrorFormat("Could not read size of block {0}, aborting", blockId);
			return data.Length + 1;
		}

		int size = BitConverter.ToInt32(data, pos);
		pos += 4;
		pos += size;

		Debug.LogFormat("Skiping block {0} of size {1} bytes", blockId, size);
		return pos;
	}

	private int ReadCommonBlock(byte[] data, int pos)
	{
		// Make sure we can read the block size
		if ((pos + 4) >= data.Length)
		{
			Debug.LogError("Could not read size of block 2, aborting");
			return data.Length + 1;
		}

		int size = BitConverter.ToInt32(data, pos);
		pos += 4;

		// Make sure we have enough data left to read
		if ((pos + size) >= data.Length)
		{
			Debug.LogError("Not enough data left to read block 2");
			return data.Length + 1;
		}

		// Read data
		m_LineHeight = (int)BitConverter.ToUInt16(data, pos); pos += 2;	// lineHeight
		m_Base = (int)BitConverter.ToUInt16(data, pos); pos += 2;		// base
		m_Width = (int)BitConverter.ToUInt16(data, pos); pos += 2; 		// scaleW
		m_Height = (int)BitConverter.ToUInt16(data, pos); pos += 2; 	// scaleH;
		pos += 2; // pages
		pos += 1; // bitfield
		pos += 1; // alphaChnl
		pos += 1; // redChnl
		pos += 1; // greenChnl
		pos += 1; // blueChnl

		if (size != 15)
		{
			Debug.LogError("Invalid block size for block 2");
			return data.Length + 1;
		}

		return pos;
	}

	private int ReadCharsBlock(byte[] data, int pos, List<Character> chars)
	{
		// Make sure we can read the block size
		if ((pos + 4) >= data.Length)
		{
			Debug.LogError("Could not read size of block 4, aborting");
			return data.Length + 1;
		}

		int size = BitConverter.ToInt32(data, pos);
		pos += 4;
	
		// Make sure we have enough data left to read
		if ((pos + size) >= data.Length)
		{
			Debug.LogError("Not enough data left to read block 4");
			return data.Length + 1;
		}

		int numChars = size / 20;
		for (int i = 0; i < numChars; i++)
		{
			var ch = new Character();

			ch.id = Char.ConvertFromUtf32((int)BitConverter.ToUInt32(data, pos))[0]; pos += 4;
			ch.x = (int)BitConverter.ToUInt16(data, pos); pos += 2;
			ch.y = (int)BitConverter.ToUInt16(data, pos); pos += 2;
			ch.width = (int)BitConverter.ToUInt16(data, pos); pos += 2;
			ch.height = (int)BitConverter.ToUInt16(data, pos); pos += 2;
			ch.xoffset = (int)BitConverter.ToInt16(data, pos); pos += 2;
			ch.yoffset = (int)BitConverter.ToInt16(data, pos); pos += 2;
			ch.advance = (int)BitConverter.ToInt16(data, pos); pos += 2;
			int p = (int)BitConverter.ToUInt16(data, pos); pos += 1;
			int c = (int)BitConverter.ToUInt16(data, pos); pos += 1;

			Debug.LogFormat("Char '{0}' x:{1} y:{2} xo:{3} yo:{4} width:{5} height:{6}", ch.id, ch.x, ch.y, ch.xoffset, ch.yoffset, ch.width, ch.height);

			chars.Add(ch);
		}

		return pos;
	}

	#endif
}
