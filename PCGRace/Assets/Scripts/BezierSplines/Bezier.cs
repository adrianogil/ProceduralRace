using UnityEngine;

public static class Bezier 
{

	public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,  float t)
	{
		//return Vector3.Lerp (Vector3.Lerp (p0, p1, t), Vector3.Lerp (p1, p2, t), t);

		t = Mathf.Clamp01 (t);

		return (1 - t) * (1 - t) * (1 - t) * p0 +
			3 * (1 - t) * (1 - t) * t * p1 + 
			3 * (1 - t) * t * t * p2 + 
			t * t * t * p3;
	}

	public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,  float t)
	{
		//return Vector3.Lerp (Vector3.Lerp (p0, p1, t), Vector3.Lerp (p1, p2, t), t);
		
		t = Mathf.Clamp01 (t);
		
		return 3f * (1 - t) * (1 - t) * (p1 - p0) +
			   6f * (1 - t) * t * (p2 - p1) + 
			   3f * t * t * (p3 - p2);
	}
}

public static class CatmullRom 
{
	public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,  float t)
	{
		t = Mathf.Clamp01 (t);

		Vector3 a = 0.5f * (2f * p1);
		Vector3 b = 0.5f * (p2 - p0);
		Vector3 c = 0.5f * (2f * p0 - 5f * p1 + 4f * p2 - p3);
		Vector3 d = 0.5f * (-p0 + 3f * p1 - 3f * p2 + p3);

		Vector3 pos = a + (b * t) + (c * t * t) + (d * t * t * t);

		return pos;
	}

	public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,  float t)
	{
		t = Mathf.Clamp01 (t);

		Vector3 b = 0.5f * (p2 - p0);
		Vector3 c = 0.5f * (2f * p0 - 5f * p1 + 4f * p2 - p3);
		Vector3 d = 0.5f * (-p0 + 3f * p1 - 3f * p2 + p3);

		Vector3 pos = b + (2 * c * t) + (3 * d * t * t);

		return pos;
	}
}
