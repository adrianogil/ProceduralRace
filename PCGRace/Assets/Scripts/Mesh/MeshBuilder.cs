using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Mesh builder utilitary class.
/// Inspired by http://jayelinda.com/modelling-by-numbers-part-1a/
/// </summary>
public class MeshBuilder {
	
	private List<Vector3> m_Vertices = new List<Vector3>();
	public List<Vector3> Vertices { get { return m_Vertices; }}
	
	private List<Vector3> m_Normals = new List<Vector3>();
	public List<Vector3> Normals { get { return m_Normals; }}
	
	private List<Vector2> m_UVs = new List<Vector2>();
	public List<Vector2> UVs { get { return m_UVs; }}
	
	private List<Vector4> m_Tangents = new List<Vector4>();
	public List<Vector4> Tangents { get { return m_Tangents; }}
	
	private List<int> m_Indices = new List<int>();
	
	public void AddTriangle(int index0, int index1, int index2)
	{
		m_Indices.Add (index0);
		m_Indices.Add (index1);
		m_Indices.Add (index2);
	}
	
	public Mesh CreateMesh()
	{
		Mesh mesh = new Mesh ();
		
		mesh.vertices = m_Vertices.ToArray ();
		mesh.triangles = m_Indices.ToArray ();
		
		// Normals sao opcionais
		if (m_Normals.Count == m_Vertices.Count) {
			mesh.normals = m_Normals.ToArray ();
		} else {
			mesh.RecalculateNormals();
		}
		
		// UVs sao opcionais
		if (m_UVs.Count == m_Vertices.Count) {
			mesh.uv = m_UVs.ToArray();
		}
		
		// Tangents sao opcionais
		if (m_Tangents.Count == m_Vertices.Count) {
			mesh.tangents = m_Tangents.ToArray();
		}
		
		mesh.RecalculateBounds ();
		
		return mesh;
	}
	
}
