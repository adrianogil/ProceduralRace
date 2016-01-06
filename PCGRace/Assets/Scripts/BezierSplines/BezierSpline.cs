using UnityEngine;
using System;
using System.Collections;

public enum BezierControlPointMode {
	Free,
	Aligned,
	Mirrored
}

public class BezierSpline : MonoBehaviour {

	[SerializeField]
	private Vector3[] points;

	[SerializeField]
	private BezierControlPointMode[] modes;

	[SerializeField]
	private bool loop;

	public Action OnBezierPointChanged;

	public bool Loop
	{
		get {
			return loop;
		}
		set {
			loop = value;

			if (loop)
			{
				modes[modes.Length - 1] = modes[0];
				SetControlPoint(0, points[0]);
			}
		}
	}

	public int ControlPointCount
	{
		get
		{
			return points.Length;
		}
	}

	public Vector3 GetControlPoint(int index)
	{
		return points [index];
	}

	public void SetControlPoint(int index, Vector3 point)
	{
		if (loop)
		{
			if (index == 0)
			{
				points[points.Length - 1] = point;
			}
			else if (index == points.Length - 1)
			{
				points[0] = point;
			}
		}

		points [index] = point;
	}

	public void Reset()
	{
		points = new Vector3[]
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(3f, 0f, 0f),
			new Vector3(4f, 0f, 0f)
		};

		modes = new BezierControlPointMode[]
		{
			BezierControlPointMode.Free,
			BezierControlPointMode.Free
		};
	}


	public int CurveCount
	{
		get
		{
			if (points == null) {
				return 0;
			}
			
			return (points.Length - 1) / 3;
		}
	}

	public Vector3 GetPoint(float t)
	{
		int i;

		if (t >= 1f) {
			t = 1f;

			i = points.Length - 4;
		} else {
			t = Mathf.Clamp01(t) * CurveCount;

			i = (int)t;
			t -= i; // [0, 1]

			i *= 3;
		}

		return transform.TransformPoint(CatmullRom.GetPoint (points[i], points[i+1], points[i+2], points[i+3], t));
	}

	public Vector3 GetVelocity(float t) {
		int i;

		if (t >= 1f) {
			t = 1f;
			
			i = points.Length - 4;
		} else {
			t = Mathf.Clamp01(t) * CurveCount;
			
			i = (int)t;
			t -= i; // [0, 1]
			
			i *= 3;
		}
		
		return transform.TransformPoint(
			CatmullRom.GetFirstDerivative (points[i], points[i+1], points[i+2], points[i+3], t)
			);
	}

	public Vector3 GetDirection(float t) {
		return GetVelocity (t).normalized;
	}

	public void AddCurve()
	{
		Vector3 point = points[points.Length - 1];
		Array.Resize (ref points, points.Length + 3);
		point.x += 1f;
		points [points.Length - 3] = point;
		point.x += 1f;
		points [points.Length - 2] = point;
		point.x += 1f;
		points [points.Length - 1] = point;

		Array.Resize (ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];

		if (loop) {
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
		}
	}

	public void ClearControlPoints()
	{
		points = null;
	}
		

	public void AddControlPoint(Vector3 point) 
	{
		if (points == null) {
			points = new Vector3[1];

			points [0] = point;
		} else {
			Array.Resize (ref points, points.Length + 1);

			if (loop) {
				points [points.Length - 1] = points [points.Length - 2];
				points [points.Length - 2] = point;
			} else {
				points [points.Length - 1] = point;
			}
		}
	}

	public BezierControlPointMode GetControlPointMode(int index)
	{
		return modes[(index + 1)/3];
	}

	public void SetControlPointMode(int index, BezierControlPointMode mode)
	{
		int modeIndex = (index + 1) / 3;
		modes [modeIndex] = mode;

		if (loop) {
			if (modeIndex == 0)
			{
				modes[modes.Length - 1] = mode;
			}
			else if (modeIndex == modes.Length - 1)
			{
				modes[0] = mode;
			}
		}
			
	}
}
