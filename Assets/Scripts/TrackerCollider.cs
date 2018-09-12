using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerCollider : MonoBehaviour {

	public bool isDancer;
	public bool isClose;
    //isIntro: is the player in the introduction phase; true for player at the beginning; false for dancer
    public bool isInIntro;

    private void OnTriggerEnter(Collider other)
    {
        WayPoint w = other.GetComponent<WayPoint>();
        if (w != null)
        {
            if (isInIntro && w.index == 0)
            {
                //If the player is in the introduction position
                w.Play();
                isInIntro = false;
            }
            else if(!isInIntro &&!isDancer && !isClose) {
                //If not in introduction position and not an dancer and not Close
                w.Play();
	        }
	        else if(!isInIntro)
            {
                //Else
		        w.ReachWithDancer();
            }
        }
    }
	
}
