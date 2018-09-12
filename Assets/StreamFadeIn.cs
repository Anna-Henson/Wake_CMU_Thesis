using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamFadeIn : MonoBehaviour {

    private bool _fadeIn = false;
    public GameObject RSPlane;

    private void OnTriggerEnter(Collider other)
    {
        TrackerCollider trackerCollider = other.GetComponent<TrackerCollider>();
        if (!trackerCollider.isDancer && !trackerCollider.isClose)
        {
            _fadeIn = true;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (_fadeIn == true)
        {
            Debug.Log("FadeIn Triggered");
            float current = RSPlane.GetComponent<Renderer>().material.GetFloat("_FadeOut");

            if (current >= 1)
            {
                _fadeIn = false;
            }

            RSPlane.GetComponent<Renderer>().material.SetFloat("_FadeOut", current + 0.1f * Time.deltaTime);
        }
	}
}
