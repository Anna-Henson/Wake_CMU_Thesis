using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRecord : MonoBehaviour {

    public GameObject trackerPlayer;
    public LineRenderer pathRenderer;
    public GameObject renderPlane;

    //Dancer's Trackers
    [Header("Dancer's Trackers")]
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

    [ContextMenu("Reset Mesh")]
    private void SetTextureLocation()
    {
        
        Mesh mesh = renderPlane.GetComponent<MeshFilter>().mesh;
        Vector2 tracker1Pos = new Vector2(tracker1.transform.position.x, tracker1.transform.position.z);
        Vector2 tracker2Pos = new Vector2(tracker2.transform.position.x, tracker2.transform.position.z);
        float distance = Vector2.Distance(tracker1Pos, tracker2Pos);
        Vector3 newCenter = (tracker1.transform.position + tracker2.transform.position) / 2;
        
        Vector2 bound1 = new Vector2(mesh.bounds.min.x, mesh.bounds.min.z);
        Vector2 bound2 = new Vector2(mesh.bounds.max.x, mesh.bounds.max.z);
        Debug.Log("Bound1 : " + bound1 + "Bound2:" + bound2);
        float meshDist = Vector2.Distance(bound1, bound2);
        Debug.Log("MeshDist:" + meshDist);
        float ratio = distance / meshDist;

        Vector3 center = mesh.bounds.center;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        renderPlane.transform.position = newCenter;
        renderPlane.transform.localScale *= 2 * ratio;
        renderPlane.GetComponent<MeshRenderer>().enabled = true;
    }

    private void Start()
    {
        isTracking = true;
        startTime = Time.time;
        renderPlane.GetComponent<MeshRenderer>().enabled = false;
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
            SetTextureLocation();
            StartCoroutine(DrawPath(path));
        }
	}
}
