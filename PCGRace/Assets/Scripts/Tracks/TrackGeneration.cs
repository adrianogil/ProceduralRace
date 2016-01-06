using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BezierSpline))]
public class TrackGeneration : MonoBehaviour {

	public int curvePoints = 32;

	public float radiusSizeFactor = 10f;

	public float roadSize = 20f;

	public float roadWallSize = 3f;

	public Vector2 radius = 20f * Vector2.one, frequency = Vector2.one;

	public Material trackMaterial;

	private Vector3[] centerTrackPoints;

	private List<Vector3> trackPoints = new List<Vector3>();

	private BezierSpline bezierSpline;

	public void GenerateTrack() {
		trackPoints.Clear();

		Vector3 point = Vector3.zero;

		float radiusAmount = 0f;

		centerTrackPoints = new Vector3[curvePoints];

		// Set Curves on Bezier Spline
		bezierSpline = GetComponent<BezierSpline> ();
		bezierSpline.Reset ();
//		bezierSpline.ClearControlPoints ();
		bezierSpline.Loop = true;
		bezierSpline.OnBezierPointChanged = UpdateTrackMesh;

		for (int i = 0; i < curvePoints; i++)
		{
			radiusAmount = ((float)i / curvePoints) * (2*Mathf.PI);
			point = new Vector3(radius.x * Mathf.Cos (frequency.x * radiusAmount), 0f, radius.y * Mathf.Sin (frequency.y * radiusAmount));

			point = point * radiusSizeFactor;
			centerTrackPoints[i] = point;

			trackPoints.Add (point);

			// Set points in the Bezier curve
			if (i > 0 && i % 3 == 0)
				bezierSpline.AddCurve ();
			
			bezierSpline.SetControlPoint (i, point);
		}

		UpdateTrackMesh ();
	}

	void UpdateTrackMesh()
	{
		int totalPoints = 10 * curvePoints;

		centerTrackPoints = new Vector3[totalPoints];

		for (int i = 0; i < totalPoints; i++)
			centerTrackPoints [i] = bezierSpline.GetPoint (((float)i)/(totalPoints));

		GameObject trackObject = GenerateTrackMesh (centerTrackPoints);
		trackObject.transform.parent = transform;

		GameObject wallObject = GenerateRoadWallsMeshes (centerTrackPoints);
		wallObject.transform.parent = trackObject.transform;
	}

	GameObject GenerateTrackMesh(Vector3[] points) {

		MeshBuilder meshBuilder = new MeshBuilder();

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
		
		return GenerateObject( GenerateCurveTriangles (meshBuilder), "Track");
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

		GameObject roadWallsObject;

		Transform roadWallsTransform = transform.Find ("Track").Find ("Road Walls");

		if (roadWallsTransform == null) {
			roadWallsObject = new GameObject ("Road Walls");
		} else {
			roadWallsObject = roadWallsTransform.gameObject;
		}

		GameObject roadWallSideA = GenerateObject( GenerateCurveTriangles (meshBuilderWall1, true), "Road Wall Side A");
		GameObject roadWallSideB = GenerateObject(  GenerateCurveTriangles (meshBuilderWall2, true), "Road Wall Side B" );

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

	GameObject GenerateObject(MeshBuilder meshBuilder, string objectName) {

		GameObject go; // Object to be created or modified

		Transform to = transform.FindDeepChild(objectName); // Verify if object already exists

		if (to == null) {
			go = new GameObject (objectName);

			go.AddComponent<MeshFilter> ().mesh = meshBuilder.CreateMesh ();
			go.AddComponent<MeshRenderer> ().material = trackMaterial;
		} else {
			go = to.gameObject;

			go.GetComponent<MeshFilter> ().mesh = meshBuilder.CreateMesh ();
			go.GetComponent<MeshRenderer> ().material = trackMaterial;
		}


		
		return go;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		
		for (int i = 0; i < trackPoints.Count; i++)
			Gizmos.DrawSphere(transform.TransformPoint(trackPoints[i]), 0.5f);
	}
}


public static class TransformDeepChildExtension
{
	//Breadth-first search
	public static Transform FindDeepChild(this Transform aParent, string aName)
	{
		var result = aParent.Find(aName);
		if (result != null)
			return result;
		foreach(Transform child in aParent)
		{
			result = child.FindDeepChild(aName);
			if (result != null)
				return result;
		}
		return null;
	}
}