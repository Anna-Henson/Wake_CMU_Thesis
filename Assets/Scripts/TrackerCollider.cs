using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerCollider : MonoBehaviour {

  private void OnTriggerEnter(Collider other)
  {
    WayPoint w = other.GetComponent<WayPoint>();
    if (w != null)
    {
      w.Play();
    }
  }

  // Use this for initialization
  void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
