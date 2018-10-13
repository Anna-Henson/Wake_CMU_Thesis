using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbiter : MonoBehaviour
{
    public Transform target;
    public Transform Target
    {
        get
        {
            if (target == null)
                target = transform.parent.GetComponent<Transform>();
            return target;
        }
    }

    //private float radius;
    //public float angularSpeed = 10;
    public float duration = 1f;

    // Use this for initialization
    private void Start()
    {
        //radius = (transform.position - Target.position).magnitude;
    }

    // Update is called once per frame
    private void Update()
    {
        Vector3 dir = transform.position - Target.position;
        transform.position = Target.position + 
            Quaternion.AngleAxis(360f/duration * Time.deltaTime, Target.up) * dir;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Target.position, 0.1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}
