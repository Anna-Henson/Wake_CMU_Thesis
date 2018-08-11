using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{

  private LineRenderer lineRenderer;
  public GameObject trackedObject1;
  public GameObject trackedObject2;
  private Vector3 controlPoint1;
  private Vector3 controlPoint2;
  public int numPoints;
  public float controlDistance;
  public float lineWidth;
  public GameObject originObject1;
  public GameObject originObject2;
  private Vector3 originPoint1;
  private Vector3 originPoint2;
  public float amplitude;
  public float wobbles;
  public AnimationCurve myWidthCurve;

  // Use this for initialization
  void Start()
  {
    print("Hello World");
    lineRenderer = GetComponent<LineRenderer>();

    lineRenderer.positionCount = numPoints;

  }

  // Update is called once per frame
  void Update()
  {

    //lineRenderer.startWidth = lineWidth;
    //lineRenderer.endWidth = lineWidth;
    lineRenderer.widthMultiplier = lineWidth;
    lineRenderer.widthCurve = myWidthCurve;
    originPoint1 = originObject1.transform.position;
    originPoint2 = originObject2.transform.position;

    float distance = Vector3.Distance(originPoint1, originPoint2);

    //originPoint1.y -= ComputeSinOffset(0, distance);
    //originPoint2.y -= ComputeSinOffset(1, distance);
    // this is not doing anything anymore (results in 0) because the yFactor cancels it out

    //controlPoint1 = trackedObject1.transform.position + trackedObject1.transform.forward * controlDistance;
    //controlPoint2 = trackedObject2.transform.position + trackedObject2.transform.forward * controlDistance;

    controlPoint1 = originPoint1 + trackedObject1.transform.forward * controlDistance;
    controlPoint2 = originPoint2 + trackedObject2.transform.forward * controlDistance;

    //controlPoint1.y -= ComputeSinOffset(0, distance);
    //controlPoint2.y -= ComputeSinOffset(1, distance);

    //print(ComputeBezierPoint(0));

    var points = new Vector3[numPoints];
    for (int i = 0; i < numPoints; i++)
    {
      float t = (float)i / numPoints;
      points[i] = ComputeBezierPoint(t, distance);
    }
    lineRenderer.SetPositions(points);
  }
  Vector3 ComputeBezierPoint(float t, float distance)
  {
    //Vector3 mid1 = Vector3.Lerp(trackedObject1.transform.position, controlPoint1, t);
    //Vector3 mid2 = Vector3.Lerp(controlPoint2, trackedObject2.transform.position, t);

    Vector3 mid1 = Vector3.Lerp(originPoint1, controlPoint1, t);
    Vector3 mid2 = Vector3.Lerp(controlPoint2, originPoint2, t);

    Vector3 result = Vector3.Lerp(mid1, mid2, t);
    float yOffset = ComputeSinOffset(t, distance);
    result.y += yOffset;
    return result;

  }
  float ComputeSinOffset(float t, float distance)
  {
    //print(distance);
    distance = Mathf.Max(distance, 0.1f);
    float yFactor = t < 0.5f ? 2 * t : 2 - 2 * t;
    return yFactor * amplitude * Mathf.Sin(t / distance * wobbles + Time.time);
  }
}
