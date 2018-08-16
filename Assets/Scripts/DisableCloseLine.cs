using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableCloseLine : MonoBehaviour {

	public bool disableDancer;
	public GameObject dancerObj;
	public GameObject playerCurveLine;
	public GameObject playerTrackerLine;
	public TrackerCollider playerTracker;
	private AudioSource audioSrc;

	private void Start()
	{
		audioSrc = GetComponent<AudioSource>();
	}

	private void Update ()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Disable();
		}
	}

	[ContextMenu("Disable")]
	public void Disable()
	{
		if (disableDancer)
		{
			dancerObj.SetActive(false);
		}
		audioSrc.Play();
		gameObject.SetActive(false);
		playerCurveLine.SetActive(true);
		playerTrackerLine.SetActive(true);
		playerTracker.isClose = false;
	}
}
