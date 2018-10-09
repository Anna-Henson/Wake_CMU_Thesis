using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class FakeMagnetHook : MonoBehaviour
{
    public ObiRope rope;
    public ObiParticleHandle targetHandle;
    public bool attached = false;

    // Use this for initialization
    private void Awake()
    {
        DetachHook();
    }

    private void LaunchHook()
    {
        // Get the mouse position in the scene, in the same XY plane as this object:
        Vector3 mouse = Input.mousePosition;
        mouse.z = 1;// transform.position.z - Camera.main.transform.position.z;
        Vector3 mouseInScene = Camera.main.ScreenToWorldPoint(mouse);

        // Get a ray from the character to the mouse:
        Ray ray = new Ray(Camera.main.transform.position, mouseInScene - Camera.main.transform.position);

        // Raycast to see what we hit:
        RaycastHit hookAttachment;
        if (Physics.Raycast(ray, out hookAttachment))
        {
            // We actually hit something, so attach the hook!
            StartCoroutine(AttachHook(hookAttachment.transform));
        }
        else
        {
            DetachHook();
        }
    }

    public IEnumerator AttachHook(Transform _target)
    {
        //print("Hook to " + _target + " "+ _target.position);
        rope.GetComponent<MeshRenderer>().enabled = true;
        //rope.gameObject.SetActive(true);
        float dist = (_target.position - targetHandle.transform.position).magnitude;
        while (dist > 0.01f)
        {
            //targetHandle.transform.position = Vector3.Lerp(targetHandle.transform.position, _target.position, 0.15f);
            targetHandle.transform.Translate((_target.position - targetHandle.transform.position) * 0.10f);
            dist = (_target.position - targetHandle.transform.position).magnitude;
            //print("dist " + dist);
            yield return null;
        }
        //print("reach " + targetHandle.transform.position);

        targetHandle.transform.position = _target.position;

        attached = true;
    }

    public IEnumerator AttachHook(Vector3 _targetPos)
    {
        //print("Hook to " + _target + " "+ _target.position);
        rope.GetComponent<MeshRenderer>().enabled = true;
        //rope.gameObject.SetActive(true);
        float dist = (_targetPos - targetHandle.transform.position).magnitude;
        while (dist > 0.01f)
        {
            //targetHandle.transform.position = Vector3.Lerp(targetHandle.transform.position, _target.position, 0.15f);
            targetHandle.transform.Translate((_targetPos - targetHandle.transform.position) * 0.10f);
            dist = (_targetPos - targetHandle.transform.position).magnitude;
            //print("dist " + dist);
            yield return null;
        }
        //print("reach " + targetHandle.transform.position);

        targetHandle.transform.position = _targetPos;

        attached = true;
    }

    public IEnumerator AttachHookAsChild(Transform _target)
    {
        targetHandle.transform.parent = _target;
        targetHandle.transform.localPosition = Vector3.zero;

        rope.GetComponent<MeshRenderer>().enabled = true;

        attached = true;
        yield return null;
    }

    public void SetBackToOriginParent()
    {
        targetHandle.transform.parent = transform.parent;
    }

    public void DetachHook()
    {
        SetToHidePos();
        rope.GetComponent<MeshRenderer>().enabled = false;
        //rope.gameObject.SetActive(false);
        attached = false;
    }

    private void SetToHidePos()
    {
        targetHandle.transform.position = transform.position + transform.forward;
        rope.GetComponent<AutoLength>().AdjustLength();
    }

    // Update is called once per frame
    private void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    LaunchHook();
        //}

        //if (Input.GetKey(KeyCode.W))
        //{
        //    rope.transform.Translate(transform.forward * Time.deltaTime * 1);
        //}
        //if (Input.GetKey(KeyCode.S))
        //{
        //    rope.transform.Translate(transform.forward * Time.deltaTime * -1);
        //}
        //if (Input.GetKey(KeyCode.A))
        //{
        //    rope.transform.Translate(transform.right * Time.deltaTime * -1);
        //}
        //if (Input.GetKey(KeyCode.D))
        //{
        //    rope.transform.Translate(transform.right * Time.deltaTime * 1);
        //}
        //if (Input.GetKey(KeyCode.Q))
        //{
        //    rope.transform.Rotate(transform.up, Time.deltaTime * -150);
        //}
        //if (Input.GetKey(KeyCode.E))
        //{
        //    rope.transform.Rotate(transform.up, Time.deltaTime * 150);
        //}
    }
}
