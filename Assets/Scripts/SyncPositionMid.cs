using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncPositionMid : SyncPosition {

	public GameObject otherTracker;

	// Update is called once per frame
	void LateUpdate(){
		Vector3 pos = (dancerTracker.transform.position + otherTracker.transform.position) / 2;
		pos.y = 0f;
		this.transform.position = pos;

	}

}
