using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiRopeCursor))]
public class AutoLength : MonoBehaviour
{
    public Transform target;
    private ObiRopeCursor cursor;
    private ObiRope rope;

    public float baseFactor = 0.8f;
    public float factor;
    public float lerpFactor = 0.5f;
    public float dist;
    private Vector3 initDir;
    private float initUv;
    private Quaternion initQuaternion;

    public ObiRope Rope
    {
        get
        {
            if(rope == null)
                rope = GetComponent<ObiRope>();
            return rope;
        }
    }

    public ObiRopeCursor Cursor
    {
        get
        {
            if(cursor == null)
                cursor = GetComponent<ObiRopeCursor>();
            return cursor;
        }

        set
        {
            cursor = value;
        }
    }

    private void Awake()
    {
        if (target)
        {
            initDir = (target.position - transform.parent.position).normalized;
            initQuaternion = Quaternion.FromToRotation(transform.forward, initDir);
            initUv = Rope.uvScale.y * Rope.RestLength;
        }
    }

    // Use this for initialization
    private void Start()
    {

    }

    public void SetTarget(Transform _target)
    {
        target = _target;

        initDir = (target.position - transform.parent.position).normalized;
        initQuaternion = Quaternion.FromToRotation(transform.forward, initDir);
        initUv = Rope.uvScale.y * Rope.RestLength;
    }

    // Update is called once per frame
    private void Update()
    {
        if (target == null)
            return;

        if (Input.GetKey(KeyCode.UpArrow))
            transform.parent.position -= initDir * 5 * Time.deltaTime;
        else if (Input.GetKey(KeyCode.DownArrow))
            transform.parent.position += initDir * 5 * Time.deltaTime;

        AdjustLength();
    }

    public void AdjustLength()
    {
        Vector3 dir = (target.position - transform.position);
        dist = dir.magnitude;

        float angle = Vector3.Angle(dir, initQuaternion * transform.forward);
        //print("angle " + angle);
        factor = baseFactor + angle / 180f * 0.4f;

        float newLength = Mathf.Lerp(Rope.RestLength, dist * factor, lerpFactor);
        Cursor.ChangeLength(newLength);
        //rope.RestLength = dist * factor;
        rope.uvScale = new Vector2(1, initUv / rope.RestLength);

        //print("new length " + newLength);
    }
}
