using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOnPath : MonoBehaviour {

  public PathArray PathToFollow;

  public int CurrentWayPointID = 0;
  public float speed;
  public float rotationSpeed = 5.0f;
  public string pathName;

  private float reachDistance = 1.0f;

  Vector3 last_position;
  Vector3 current_position;



	// Use this for initialization
	void Start () {
    //PathToFollow = GameObject.Find(pathName).GetComponent<PathArray>();
    last_position = transform.position;

	}
	
	// Update is called once per frame
	void Update () {
    float distance = Vector3.Distance(PathToFollow.path_objs[CurrentWayPointID].position, transform.position);
    transform.position = Vector3.MoveTowards(transform.position, PathToFollow.path_objs[CurrentWayPointID].position, Time.deltaTime * speed);

    if(distance <= reachDistance)
    {
      CurrentWayPointID++;
    }
    if (CurrentWayPointID >= PathToFollow.path_objs.Count)
    {
      CurrentWayPointID = 0;
    }
	}
}
