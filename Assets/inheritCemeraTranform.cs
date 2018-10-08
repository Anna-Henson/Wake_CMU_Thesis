using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inheritCemeraTranform : MonoBehaviour {
    public GameObject cameraObject;
    private Vector3 offset;
    public float Y;

	// Use this for initialization
	void Start () {
        offset = transform.position - cameraObject.transform.position;
        //Y = transform.position.y;
        Debug.Log(offset);
	}
    void Update()
    {
        
    }
    void LateUpdate () {
        transform.SetPositionY(Y);
        float YRotate = cameraObject.transform.eulerAngles.y;
        transform.eulerAngles = new Vector3(0, YRotate, 0);
    }
}
