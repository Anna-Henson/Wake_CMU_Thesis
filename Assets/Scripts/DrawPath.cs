using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathPoints
{
  public class DrawPath : MonoBehaviour
  {
    public IList<Point> PathPoints = new List<Point>();

    const int maxPoints = 20;

    int pathArrayIndex = 0;

    LineRenderer lineRenderer;

    void Start()
    {
      System.Random random = new System.Random();

      // initialize my path points
      for (int i = 0; i < maxPoints; i++)
      {
        PathPoints.Add(new Point { X = random.Next() % 100, Y = random.Next() % 100 });
      }

      lineRenderer = GetComponent<LineRenderer>();
    }

    private T GetComponent<T>()
    {
      throw new NotImplementedException();
    }

    void Update()
    {
      if (pathArrayIndex != 0 && pathArrayIndex < (maxPoints - 2)) // nothing to draw first time through AND respect length of path
      {
        // read the tracker and check if location is within 'reach distance' of PathPoints[pathArrayIndex + 1]
        // if it is, use lineRenderer to draw line between PathPoints[pathArrayIndex] and PathPoints[pathArrayIndex + 1]
      }

      if (pathArrayIndex < maxPoints - 1)
      {
        pathArrayIndex++;
      }
    }
  }


  public class Point
  {
    public float X { get; set; }

    public float Y { get; set; }
  }
}
