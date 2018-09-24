using UnityEngine;
using System.Collections;

public class riverFlow : MonoBehaviour
{
    private static Vector3[] curvePoints;

    public static void setCurvePoints(Vector3[] curvePointsInput)
    {
        curvePoints = curvePointsInput;
    }

    void Start()
    {
        Debug.Log("Curvepoints are: " + curvePoints);   
        iTween.MoveBy(gameObject, iTween.Hash("path", curvePoints , "easeType", "easeInOutExpo", "loopType", "pingPong", "delay", .1));
    }

}