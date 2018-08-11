using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDataCopy : MonoBehaviour {

  public BezierCurve bezierCurve;
  public int resolution;
  private LineRenderer lineRenderer;
  private int currentIndex;
  public float trackerLength;
  private float trackerDistance;
  public float lerpAmount;
  public int sign = 1; //should be 1 (forward) or -1 (backward)

  public void ReachIndex(int index)
  {
    currentIndex = index;
  }

  public int GetIndex()
  {
    return currentIndex;
  }

  private Vector3 GetClosestPoint(Vector3 position, bool ahead)
  {
    float bestT = 0;
    float bestDistance = 10000f;

    int i = currentIndex;
    BezierPoint p0 = bezierCurve[i];
    BezierPoint p1 = bezierCurve[i + sign];
    for (int j = 1; j < resolution + 1; j++)
    {
      float t = (float)j / resolution;
      Vector3 currentPoint = GetCurvePoint(p0, p1, t);
      float distance = Vector3.Distance(position, currentPoint);
      if (distance < bestDistance)
      {
        bestDistance = distance;
        bestT = t;
      }
    }
    if (ahead)
    {
      bestT = Mathf.Clamp(bestT + trackerLength, 0, 1);
      trackerDistance = Mathf.Lerp(trackerDistance, bestT, lerpAmount);
    }
    return GetCurvePoint(p0, p1, bestT);
  }

  public Vector3 CalculateTrackerEndpoint(Vector3 position, Vector3 currentEndPoint)
  {
    Vector3 goalPoint = GetClosestPoint(position, true);
    return Vector3.Lerp(currentEndPoint, goalPoint, lerpAmount);
  }

  public Vector3 CalculateHelperPoint(Vector3 position)
  {
    return GetClosestPoint(position, false);
  }

  /*public Vector3 GetClosestPoint(Vector3 position, Vector3 currentEndPoint)
  {
    float bestT = 0;
    float bestDistance = 10000f;

    int i = currentIndex;
    BezierPoint p0 = bezierCurve[i];
    BezierPoint p1 = bezierCurve[i + 1];
    for (int j = 1; j < resolution + 1; j++)
    {
      float t = (float)j / resolution;
      Vector3 currentPoint = BezierCurve.GetPoint(p0, p1, t);
      float distance = Vector3.Distance(position, currentPoint);
      if (distance < bestDistance)
      {
        bestDistance = distance;
        bestT = t;
      }
    }
    trackerDistance = bestT + trackerLength;
    Vector3 goalPoint = BezierCurve.GetPoint(p0, p1, trackerDistance);
    return Vector3.Lerp(currentEndPoint, goalPoint, lerpAmount);
  }*/

  private void PopulateCurve()
  {
    List<Vector3> curvePoints = new List<Vector3>();

    //for (int i = 0; i < bezierCurve.pointCount - 1; i++)
    //{
    int i = currentIndex;
    BezierPoint p0 = bezierCurve[i];
    BezierPoint p1 = bezierCurve[i + sign];

    //Vector3 lastPoint = p0.position;
    //Vector3 currentPoint = Vector3.zero;

    float increment = 1.0f / resolution;

    if (sign == 1)
    {
      for (float t = trackerDistance; t < 1; t += increment)
      {
        AddPoint(p0, p1, t, curvePoints);
      }
    }
    else
    {
      for (float t = 1; t > trackerDistance; t -= increment)
      {
        AddPoint(p0, p1, t, curvePoints);
      }
    }
    //}
    lineRenderer.positionCount = curvePoints.Count;
    lineRenderer.SetPositions(curvePoints.ToArray());
  }

  private void AddPoint(BezierPoint p0, BezierPoint p1, float t, List<Vector3> curvePoints)
  {
    Vector3 currentPoint = GetCurvePoint(p0, p1, t);
    curvePoints.Add(currentPoint);
  }

  private Vector3 GetCurvePoint(BezierPoint p0, BezierPoint p1, float t)
  {
    if (sign == 1)
    {
      return BezierCurve.GetPoint(p0, p1, t);
    }
    else
    {
      return BezierCurve.GetPoint(p1, p0, 1 - t);
    }
  }

	// Use this for initialization
	void Start () {
    if (sign == 1)
    {
      currentIndex = 0;
    }
    else
    {
      currentIndex = bezierCurve.pointCount - 1;
    }

    lineRenderer = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
    PopulateCurve();
	}
}
