using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor {

	private BezierSpline spline;
	private Transform handleTransform;
	private Quaternion handleRotation;
	
	private int lineSteps = 100;

	private int selectedIndex = -1;

	private const float handleSize = 0.06f;
	private const float pickSize = 0.1f;

	private static Color[] modeColors = {
		Color.green,
		Color.yellow,
		Color.cyan
	};

	public override void OnInspectorGUI ()
	{
		spline = target as BezierSpline;

		EditorGUI.BeginChangeCheck ();
		bool loop = EditorGUILayout.Toggle ("Loop", spline.Loop);

		EditorGUILayout.IntField ("Selected point: ", selectedIndex);

		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject(spline, "Toggle Loop");
			EditorUtility.SetDirty(spline);
			spline.Loop = loop;

		}

		if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount) {
			DrawSelectedPointInspector();
		}

		if (GUILayout.Button ("Add Curve")) {
			Undo.RecordObject(spline, "Add Curve");
			spline.AddCurve();
			EditorUtility.SetDirty(spline);
		}
	}

	private void DrawSelectedPointInspector()
	{
		GUILayout.Label ("Selected point");

		EditorGUI.BeginChangeCheck ();
		Vector3 point = EditorGUILayout.Vector3Field ("Position", spline.GetControlPoint(selectedIndex));
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject(spline, "Move point");
			EditorUtility.SetDirty(spline);
			spline.SetControlPoint(selectedIndex, point);
		}

		EditorGUI.BeginChangeCheck ();
		BezierControlPointMode mode = (BezierControlPointMode)
			EditorGUILayout.EnumPopup ("Mode", spline.GetControlPointMode(selectedIndex));
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RecordObject(spline, "Move point");
			EditorUtility.SetDirty(spline);
			spline.SetControlPointMode(selectedIndex, mode);
		}
	}

	private void OnSceneGUI() {
		spline = target as BezierSpline;
		
		handleTransform = spline.transform;
		handleRotation = handleTransform.rotation;

		Vector3 p0 = ShowPoint (0);
		for (int i = 1; i < spline.ControlPointCount; i += 3) {
			Vector3 p1 = ShowPoint (i);
			Vector3 p2 = ShowPoint (i+1);
			Vector3 p3 = ShowPoint (i+2);
		
			Handles.color = Color.gray;
			Handles.DrawLine (p0, p1);
			Handles.DrawLine (p2, p3);
		
			//Handles.DrawBezier (p0, p3, p1, p2, Color.white, null, 2f);
			p0 = p3;
		}
		Handles.color = Color.white;
		Vector3 lineStart = spline.GetPoint (0f);

		for (int i = 1; i <= lineSteps; i++) {
			Vector3 lineEnd = spline.GetPoint((float) i/lineSteps);
			Handles.DrawLine(lineStart, lineEnd);
			lineStart = lineEnd;
		}
	}
	
	private Vector3 ShowPoint(int index)
	{
		Vector3 point = handleTransform.TransformPoint (spline.GetControlPoint(index));
		Handles.color = modeColors[(int) spline.GetControlPointMode(index)];

		float size = HandleUtility.GetHandleSize(point);

		if (index == 0) {
			size *= 3f;
		} else if (index % 3 == 0) {
			size *= 2f;
		}

		if (Handles.Button (point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap)) {
			selectedIndex = index;
			Repaint(); // Redesenha inspector
		}

		if (selectedIndex == index) {
			EditorGUI.BeginChangeCheck ();
			Vector3 newPoint = Handles.DoPositionHandle(point, handleRotation);
			
			if (EditorGUI.EndChangeCheck ()) {
				Undo.RecordObject(spline, "Move point");
				EditorUtility.SetDirty(spline);
				if (newPoint != point && spline.OnBezierPointChanged != null) {
					spline.OnBezierPointChanged ();
				}
				Vector3 newPointValue = handleTransform.InverseTransformPoint (newPoint);
				newPointValue.y = spline.GetControlPoint (index).y;
				spline.SetControlPoint(index, newPointValue);

			}
		}

		return point;
	}
}
