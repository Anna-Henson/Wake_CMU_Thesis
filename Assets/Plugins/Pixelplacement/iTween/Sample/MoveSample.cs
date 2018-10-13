using UnityEngine;
using System.Collections;

public class MoveSample : MonoBehaviour
{	
	void Start(){

		iTween.MoveBy(gameObject, iTween.Hash("from", new Vector3(0,0,0), "to", new Vector3(2, 2, 2), "easeType", "easeInOutExpo", "loopType", "loop", "delay", .1));
	}
}

