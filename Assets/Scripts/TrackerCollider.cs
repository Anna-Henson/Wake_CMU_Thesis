using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerCollider : MonoBehaviour {

	public bool isDancer;

  private void OnTriggerEnter(Collider other)
  {
    WayPoint w = other.GetComponent<WayPoint>();
    if (w != null)
    {
	  if (!isDancer) {
        w.Play();
	  }
	  else
      {
		w.ReachWithDancer();
      }
    }
  }
	
}
