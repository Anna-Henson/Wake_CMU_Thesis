using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncPosition : MonoBehaviour {
    public GameObject dancerTracker;

	
	// Update is called once per frame
	void LateUpdate () {
        this.transform.position = dancerTracker.transform.position;
	}
}
