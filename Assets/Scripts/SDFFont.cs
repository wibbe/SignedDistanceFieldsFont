using UnityEngine;
using System.Collections;

public class SDFFont : ScriptableObject
{
	[SerializeField] private Texture2D m_Texture = null;


	public Texture2D texture { get { return m_Texture; } }
}
