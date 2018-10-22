using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientSync : MonoBehaviour {

    public Camera camera;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        this.GetComponent<Transform>().position = camera.transform.position;
        this.GetComponent<Transform>().rotation = camera.transform.rotation;
    }
}
