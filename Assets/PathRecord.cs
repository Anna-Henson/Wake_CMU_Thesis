using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRecord : MonoBehaviour {

    public GameObject trackerPlayer;
    public LineRenderer pathRenderer;

    //Dancer's Trackers
    public GameObject tracker1;
    public GameObject tracker2;

    private List<Vector3> path = new List<Vector3>() { };
    private float startTime; 
    private bool isTracking;

    private IEnumerator DrawPath(List<Vector3> path)
    {
        List<Vector3> renderedPath = new List<Vector3>() { };

        foreach (Vector3 point in path)
        {
            yield return new WaitForSeconds(0.01f);
            renderedPath.Add(point);
            pathRenderer.positionCount = renderedPath.Count;
            pathRenderer.SetPositions(renderedPath.ToArray());
            Debug.Log(point);
        }

        Debug.Log(renderedPath.Count);
        yield return null;
    }
    private void Start()
    {
        isTracking = true;
        startTime = Time.time;
    }

    void Update () {
		if (isTracking && Time.time - startTime > 0.05f)
        {
            startTime = Time.time;
            path.Add(trackerPlayer.transform.position);
        }

        //Stop Tracking Location When Camera Fades out
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            isTracking = false;
            StartCoroutine(DrawPath(path));
        }
	}
}
