using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TasiYokan.Curve;

public class PlayerPathDrawer : MonoBehaviour
{
    public Transform player;
    public int onLeft = 1;
    public int startSampleId = 0;
    public float speed = 1;
    private int timeCnt = 0;
    public int sampleCountInArc = 20;
    public int renderSampleCount = 10;

    private TasiYokan.Curve.BezierCurve m_curve;
    public TasiYokan.Curve.BezierCurve Curve
    {
        get
        {
            if (m_curve == null)
                m_curve = GetComponent<TasiYokan.Curve.BezierCurve>();
            return m_curve;
        }
    }

    private LineRenderer m_lineRenderer;
    public LineRenderer LineRenderer
    {
        get
        {
            if (m_lineRenderer == null)
                m_lineRenderer = GetComponent<LineRenderer>();
            return m_lineRenderer;
        }
    }

    // Use this for initialization
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        timeCnt++;
        if (timeCnt > 10 / speed)
        {
            startSampleId = (startSampleId + 1) % sampleCountInArc;
            timeCnt = 0;
        }

        UpdateCurvePos();
        DrawCurve();
    }

    private Vector3 temp;
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(Curve.Points[1].Position, Curve.Points[1].Position + temp);
    }

    private void UpdateCurvePos()
    {
        Curve.SetAnchorPosition(0, player.position + player.forward * 1 + player.up * -1);
        Vector3 targetOutDir = (player.position - Curve.Points[1].Position).SetY(0);
        float distance = targetOutDir.magnitude;


        //int isLeft = 1; //Vector3.Cross(player.forward.SetY(0), targetOutDir).y.Sgn();
        Vector3 newTargetOutDir = Quaternion.FromToRotation(Vector3.forward,
            (new Vector3(-distance, 0, 10)).normalized * onLeft) * targetOutDir;

        temp = newTargetOutDir;

        Curve.Points[0].SetHandlePosition(1, player.position + (Curve.Points[1].Position + newTargetOutDir * 0.6f - player.position) * 0.5f);
        Curve.Points[1].SetHandlePosition(0, Curve.Points[1].Position + newTargetOutDir * 0.3f);
        Curve.Points[1].SmoothHandle(true);
        Curve.Points[2].SetHandlePosition(0, Curve.Points[2].Position + (Curve.Points[1].Position - newTargetOutDir * 1.6f - Curve.Points[2].Position) * 0.5f);
    }

    private void DrawCurve()
    {
        int arcCount = 1;// Curve.Arcs.Count;
        LineRenderer.positionCount = renderSampleCount * arcCount;

        for (int i = 0; i < arcCount; ++i)
        {
            for (int j = 0; j < renderSampleCount; ++j)
            {
                int actualId = Mathf.Min((startSampleId + j), sampleCountInArc);
                LineRenderer.SetPosition(i * sampleCountInArc + j,
                    Curve.Arcs[i].CalculateCubicBezierPos((1f * actualId) / (sampleCountInArc - 1)));
            }
        }
    }

}
