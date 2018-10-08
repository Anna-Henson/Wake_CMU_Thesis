using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class CursorController : MonoBehaviour {

	ObiRopeCursor cursor;
	ObiRope rope;
	public float minLength = 0.1f;

	// Use this for initialization
	void Start () {
		cursor = GetComponentInChildren<ObiRopeCursor>();
		rope = cursor.GetComponent<ObiRope>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.W)){
			if (rope.RestLength > minLength)
				cursor.ChangeLength(rope.RestLength - 1f * Time.deltaTime);
		}

		if (Input.GetKey(KeyCode.S)){
			cursor.ChangeLength(rope.RestLength + 1f * Time.deltaTime);
		}

		if (Input.GetKey(KeyCode.A)){
			rope.transform.Translate(Vector3.left * Time.deltaTime,Space.World);
		}

		if (Input.GetKey(KeyCode.D)){
			rope.transform.Translate(Vector3.right * Time.deltaTime,Space.World);
		}

	}
}
