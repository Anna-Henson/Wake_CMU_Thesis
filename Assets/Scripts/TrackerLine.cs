using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerLine : MonoBehaviour {

  public LineDataCopy lineData;
  private LineRenderer lineRenderer;
  public int resolution;
  private Vector3 startPosition;
  private Vector3 endPosition;
  private Vector3 helperPosition;
  public Transform helperObj;
  public GameObject closeLineRenderer;
  public float closeDistance;
  public GameObject otherTracker;
  public int closeWaypoint;
  public float fadeTime;

  private Vector3 GetCurvePoint(float t)
  {
    Vector3 mid1 = Vector3.Lerp(startPosition, helperPosition, t);
    Vector3 mid2 = Vector3.Lerp(helperPosition, endPosition, t);

    Vector3 result = Vector3.Lerp(mid1, mid2, t);

    return result;
  }

  private void UpdatePoints()
  {
    Vector3[] curvePoints = new Vector3[resolution];

    for (int i = 0; i < resolution; i++)
    {
      float t = (float)i / resolution;
      curvePoints[i] = GetCurvePoint(t);
    }

    lineRenderer.positionCount = resolution;
    lineRenderer.SetPositions(curvePoints);
  }
	// Use this for initialization
	void Start () {
    lineRenderer = GetComponent<LineRenderer>();	
	}
	
	// Update is called once per frame
	void Update () {
    startPosition = transform.position;
    endPosition = lineData.CalculateTrackerEndpoint(transform.position, endPosition);//transform.position + Vector3.forward + Vector3.down;
    helperPosition = lineData.CalculateHelperPoint(transform.position);
    helperObj.position = helperPosition;
    UpdatePoints();

    if(otherTracker != null && lineData.GetIndex() >= closeWaypoint)
    {
      float distance = Vector3.Distance(transform.position, otherTracker.transform.position);
      if(distance < closeDistance)
      {
        Connect();
      }
    }
	}

  [ContextMenu("Connect")]
  public void Connect()
  {
    closeLineRenderer.SetActive(true);
    StartCoroutine(Fade());
  }

  private IEnumerator Fade()
  {
    //TODO: make line transition between tracker line and close line by lerping between corresponding points

    for (float time = 0; time < fadeTime; time += Time.deltaTime)
    {
      float t = time / fadeTime;
      SetAlpha(1 - t);
      otherTracker.GetComponent<TrackerLine>().SetAlpha(1 - t);
      SetObjAlpha(closeLineRenderer, t);
      yield return new WaitForEndOfFrame();
    }

    otherTracker.SetActive(false);
    lineData.gameObject.SetActive(false);
    otherTracker.GetComponent<TrackerLine>().lineData.gameObject.SetActive(false);
    gameObject.SetActive(false);
  }

  public void SetAlpha(float t)
  {
    SetObjAlpha(gameObject, t);
    SetObjAlpha(lineData.gameObject, t);
  }

  public void SetObjAlpha(GameObject obj, float t)
  {
    LineRenderer lr = obj.GetComponent<LineRenderer>();
    Color startCol = lr.startColor;
    startCol.a = t;
    lr.startColor = startCol;

    Color endCol = lr.endColor;
    endCol.a = t;
    lr.endColor = endCol;
  }
}
