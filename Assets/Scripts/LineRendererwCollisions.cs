using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererwCollisions : MonoBehaviour {

  LineRenderer lineRenderer = null;
  Vector3[] startingLineRendererPoints = null;

  // Use this for initialization
  void Start () {
    //get the attacked line renderer
    lineRenderer = GetComponent<LineRenderer>();

    //set the max points in the line renderer. This will be custom as you need it
    lineRenderer.positionCount = 4;
    //setup the array to store the starting positions of the line renderer points. May need updated as you apply rotation, gravity, etc.
    startingLineRendererPoints = new Vector3[4];

    //This gets 4 transform points under the parent object that I use to first create the shape of the line renderer
    lineRenderer.SetPosition(0, transform.Find("S").position);
    lineRenderer.SetPosition(1, transform.Find("M1").position);
    lineRenderer.SetPosition(2, transform.Find("M2").position);
    lineRenderer.SetPosition(3, transform.Find("E").position);

    //store the starting points in the array
    lineRenderer.GetPositions(startingLineRendererPoints);

  }

  // Fixed update is used for physics
  void FixedUpdate()
  {
    bool hitSomething = false;

    if (lineRenderer)
    {
      RaycastHit hitInfo;
      //create an array to hold the line renderer points
      Vector3[] newPointsInLine = null;

      for (int i = 0; i < startingLineRendererPoints.Length - 1; i++)
      {
        if (Physics.Linecast(startingLineRendererPoints[i], startingLineRendererPoints[i + 1], out hitInfo))
        {

          Debug.Log("Line cast between " + i + " " + startingLineRendererPoints[i] + " and " + i + 1 + " " + startingLineRendererPoints[i + 1]);

          //initialize the new array to the furthest point + 1 since the array is 0-based
          newPointsInLine = new Vector3[(i + 1) + 1];

          //transfer the points we need to the new array
          for (int i2 = 0; i2 < newPointsInLine.Length; i2++)
          {
            newPointsInLine[i2] = startingLineRendererPoints[i2];
          }

          //set the current point to the raycast hit point (the end of the line renderer)
          newPointsInLine[i + 1] = hitInfo.point;

          //flag that we hit something
          hitSomething = true;

          break;
        }
      }

      if (hitSomething)
      {
        //use new points for the line renderer
        lineRenderer.positionCount = newPointsInLine.Length;

        lineRenderer.SetPositions(newPointsInLine);
      }
      else
      {
        //use old points for the line renderer
        lineRenderer.positionCount = startingLineRendererPoints.Length;

        lineRenderer.SetPositions(startingLineRendererPoints);
      }

    }
  }
}
 