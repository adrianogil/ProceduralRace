using UnityEngine;
using System.Collections;

public class TrackGeneration : MonoBehaviour {

	public int curvePoints = 30;

	public float radiusSize = 10f;
	
	public float roadSize = 20f;

	public float roadWallSize = 3f;

	public Material trackMaterial;
	
	private MeshBuilder meshBuilder = new MeshBuilder ();

	private Vector3[] centerTrackPoints, sideATrackPoints, sideBTrackPoints;

	void Start() {
		GenerateTrack ();
	}

	public void GenerateTrack() {
		Vector3 point = Vector3.zero;
		float radiusX = 10f, radiusY = 20f;
		float radiusAmount = 0f;

		centerTrackPoints = new Vector3[curvePoints];
		sideATrackPoints = new Vector3[curvePoints];
		sideBTrackPoints = new Vector3[curvePoints];

		for (int i = 0; i < curvePoints; i++)
		{
			radiusAmount = ((float)i / curvePoints) * (2*Mathf.PI);
			point = new Vector3(radiusX * Mathf.Cos (radiusAmount), 0f, radiusY * Mathf.Sin (radiusAmount));//+ 
			//new Vector3(Random.Range (-0.5f, 0.5f), 0f, Random.Range (-0.5f, 0.5f));
			
			point = radiusSize * point;
		
//			Vector3 crossVector = Vector3.Cross(point - lastPoint, Vector3.up);

			centerTrackPoints[i] = point;
//			sideATrackPoints[i] = point + 0.5f * roadSize * crossVector.normalized;
//			sideBTrackPoints[i] = point - 0.5f * roadSize * crossVector.normalized;

		}
		
		GameObject trackObject = GenerateTrackMesh (centerTrackPoints);
		trackObject.name = "Track";
		trackObject.transform.parent = transform;

		GameObject wallObject = GenerateRoadWallsMeshes (centerTrackPoints);
		wallObject.transform.parent = trackObject.transform;
	}

	GameObject GenerateTrackMesh(Vector3[] points) {

		Vector3 lastPoint = points[points.Length - 1];

		Vector3 point;

		for (int i = 0; i < points.Length; i++) {
			point = points[i];
			
			Vector3 crossVector = Vector3.Cross(point - lastPoint, Vector3.up);
			
			Vector3 p1 = point + 0.5f * roadSize * crossVector.normalized;
			Vector3 p2 = point - 0.5f * roadSize * crossVector.normalized;
			
			meshBuilder.Vertices.Add(p1);
			meshBuilder.Vertices.Add(point);
			meshBuilder.Vertices.Add(p2);
			
			lastPoint = point;
		}
		
		return GenerateObject( GenerateCurveTriangles (meshBuilder) );
	}

	GameObject GenerateRoadWallsMeshes(Vector3[] points) {
		
		Vector3 lastPoint = points[points.Length - 1];
		
		Vector3 point;

		MeshBuilder meshBuilderWall1 = new MeshBuilder();
		MeshBuilder meshBuilderWall2 = new MeshBuilder();

		for (int i = 0; i < points.Length; i++) {
			point = points[i];
			
			Vector3 crossVector = Vector3.Cross(point - lastPoint, Vector3.up);
			
			Vector3 p1 = point + 0.5f * roadSize * crossVector.normalized;
			Vector3 p2 = point - 0.5f * roadSize * crossVector.normalized;

			Vector3 p1Lower = new Vector3(p1.x, p1.y - roadWallSize, p1.z);
			Vector3 p1Upper = new Vector3(p1.x, p1.y + roadWallSize, p1.z);

			Vector3 p2Lower = new Vector3(p2.x, p2.y - roadWallSize, p2.z);
			Vector3 p2Upper = new Vector3(p2.x, p2.y + roadWallSize, p2.z);

			meshBuilderWall1.Vertices.Add(p1Lower);
			meshBuilderWall1.Vertices.Add(p1);
			meshBuilderWall1.Vertices.Add(p1Upper);

			meshBuilderWall2.Vertices.Add(p2Lower);
			meshBuilderWall2.Vertices.Add(p2);
			meshBuilderWall2.Vertices.Add(p2Upper);
			
			lastPoint = point;
		}

		GameObject roadWallsObject = new GameObject ("Road Walls");

		GameObject roadWallSideA = GenerateObject( GenerateCurveTriangles (meshBuilderWall1, true) );
		GameObject roadWallSideB = GenerateObject(  GenerateCurveTriangles (meshBuilderWall2, true) );

		roadWallSideA.name = "Road Wall Side A";
		roadWallSideB.name = "Road Wall Side B";

		roadWallSideA.transform.parent = roadWallsObject.transform;
		roadWallSideB.transform.parent = roadWallsObject.transform;

		return roadWallsObject;
	}

	MeshBuilder GenerateCurveTriangles(MeshBuilder meshBuilder, bool doubleTriangles = false) {
		int baseIndex = 0;
		int sizeX = 2;
		int sizeY = meshBuilder.Vertices.Count / 3 - 1;
		
		int vi = baseIndex;
		
		for (int y = 0; y < sizeY; y++, vi++) {
			for (int x = 0; x < sizeX; x++, vi++) {
				meshBuilder.AddTriangle (vi, vi + sizeX + 1, vi + 1);
				meshBuilder.AddTriangle (vi + 1, vi + sizeX + 1, vi + sizeX + 2);
				
				if (y == sizeY - 1) {
					meshBuilder.AddTriangle (vi + sizeX + 1, baseIndex + x, vi + sizeX + 2);
					meshBuilder.AddTriangle (baseIndex + x + 1, vi + sizeX + 2, baseIndex + x);
				}

				if (doubleTriangles) {
					meshBuilder.AddTriangle (vi + 1, vi + sizeX + 1, vi);
					meshBuilder.AddTriangle (vi + sizeX + 2, vi + sizeX + 1, vi + 1);
					
					if (y == sizeY - 1) {
						meshBuilder.AddTriangle (vi + sizeX + 2, baseIndex + x, vi + sizeX + 1);
						meshBuilder.AddTriangle (baseIndex + x, vi + sizeX + 2, baseIndex + x + 1);
					}
				}
			}
		}
		return meshBuilder;

	}

	GameObject GenerateObject(MeshBuilder meshBuilder) {
		GameObject go = new GameObject ();
		go.AddComponent<MeshFilter> ().mesh = meshBuilder.CreateMesh ();
		go.AddComponent<MeshRenderer> ().material = trackMaterial;
		
		return go;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		
		for (int i = 0; i < meshBuilder.Vertices.Count; i++)
			Gizmos.DrawSphere(transform.TransformPoint(meshBuilder.Vertices[i]), 0.5f);
	}
}
