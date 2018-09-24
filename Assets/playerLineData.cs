using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerLineData : MonoBehaviour {

    public BezierCurve bezierCurve;
    public int resolution;
    private LineRenderer lineRenderer;
    private int currentIndex;
    private List<Vector3> curvePoints;
    public int delayTime;
    
	// Use this for initialization
	void Start () {
        curvePoints = new List<Vector3>();

        currentIndex = 0;
        lineRenderer = this.GetComponent<LineRenderer>();

        for (int i = 0; i < 7; i++)
        {
            BezierPoint p0 = bezierCurve[i];
            BezierPoint p1 = bezierCurve[i + 1];

            float increment = 1.0f / resolution;

            for (float t = 0; t < 1 - increment; t += increment)
            {
                AddPoint(p0, p1, t, curvePoints);
            }

        }
        Debug.Log(curvePoints);
        //Animation
        iTween.MoveTo(gameObject, iTween.Hash("path", curvePoints.ToArray(), "easeType", "linear", "loopType", "loop", "time", 10, "delay", delayTime));

    }
	
    private void AddPoint(BezierPoint p0, BezierPoint p1, float t, List<Vector3> curvePoints)
    {
        Vector3 currentPoint = GetCurvePoint(p0, p1, t);
        curvePoints.Add(currentPoint);
    }

    private Vector3 GetCurvePoint(BezierPoint p0, BezierPoint p1, float t)
    {
        return BezierCurve.GetPoint(p0, p1, t);
    }

	// Update is called once per frame

	void Update () {
        
	}
}
