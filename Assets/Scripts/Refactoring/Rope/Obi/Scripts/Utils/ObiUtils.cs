using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Obi
{

public static class Constants{
	public const int maxVertsPerMesh = 65000;
}

public static class ObiUtils
{

	public static void DrawArrowGizmo(float bodyLenght, float bodyWidth, float headLenght, float headWidth){

		float halfBodyLenght = bodyLenght*0.5f;
		float halfBodyWidth = bodyWidth*0.5f;

		// arrow body:
		Gizmos.DrawLine(new Vector3(halfBodyWidth,0,-halfBodyLenght),new Vector3(halfBodyWidth,0,halfBodyLenght));
		Gizmos.DrawLine(new Vector3(-halfBodyWidth,0,-halfBodyLenght),new Vector3(-halfBodyWidth,0,halfBodyLenght));
		Gizmos.DrawLine(new Vector3(-halfBodyWidth,0,-halfBodyLenght),new Vector3(halfBodyWidth,0,-halfBodyLenght));

		// arrow head:
		Gizmos.DrawLine(new Vector3(halfBodyWidth,0,halfBodyLenght),new Vector3(headWidth,0,halfBodyLenght));
		Gizmos.DrawLine(new Vector3(-halfBodyWidth,0,halfBodyLenght),new Vector3(-headWidth,0,halfBodyLenght));
		Gizmos.DrawLine(new Vector3(0,0,halfBodyLenght+headLenght),new Vector3(headWidth,0,halfBodyLenght));
		Gizmos.DrawLine(new Vector3(0,0,halfBodyLenght+headLenght),new Vector3(-headWidth,0,halfBodyLenght));
	}

	public static Bounds Transform(this Bounds b, Matrix4x4 m)
	{
	    var xa = m.GetColumn(0) * b.min.x;
	    var xb = m.GetColumn(0) * b.max.x;
	 
	    var ya = m.GetColumn(1) * b.min.y;
	    var yb = m.GetColumn(1) * b.max.y;
	 
	    var za = m.GetColumn(2) * b.min.z;
	    var zb = m.GetColumn(2) * b.max.z;
	 
		Bounds result = new Bounds();
		Vector3 pos = m.GetColumn(3);
		result.SetMinMax(Vector3.Min(xa, xb) + Vector3.Min(ya, yb) + Vector3.Min(za, zb) + pos,
						 Vector3.Max(xa, xb) + Vector3.Max(ya, yb) + Vector3.Max(za, zb) + pos);
					

		return result;
	}

	public static float Remap (this float value, float from1, float to1, float from2, float to2) {
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

	/**
	 * Modulo operator that also follows intuition for negative arguments. That is , -1 mod 3 = 2, not -1.
	 */
	public static float Mod(float a,float b)
	{
		return a - b * Mathf.Floor(a / b);
	}

	/**
	 * Calculates the area of a triangle.
	 */
	public static float TriangleArea(Vector3 p1, Vector3 p2, Vector3 p3){
		return Mathf.Sqrt(Vector3.Cross(p2-p1,p3-p1).sqrMagnitude) / 2f;
	}
}
}

