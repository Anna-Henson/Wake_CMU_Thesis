using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableLookDown : MonoBehaviour {

    public GameObject plane;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	    if (GetComponent<Transform>().rotation.x > 10)
        {
            Debug.Log("Camera is looking down!");
            plane.GetComponent<Renderer>().material.SetFloat("_FadeOut", 0);
        }
	}
}
