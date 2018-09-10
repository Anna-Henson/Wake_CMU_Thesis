using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableCloseLine : MonoBehaviour {

	public bool disableDancer;
	public GameObject dancerObj;
	public GameObject playerCurveLine;
	public GameObject playerTrackerLine;
	public GameObject dancerCurveLine;
	public GameObject dancerTrackerLine;
	public TrackerCollider playerTracker;
	public TrackerCollider dancerTracker;
	private AudioSource audioSrc;
    [Header("End Light")]
    public GameObject endLight;

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
		/* Obsolete Code: no longer need lines to be drawn after close line
        else
		{
			dancerCurveLine.SetActive(true);
			dancerTrackerLine.SetActive(true);
		}*/
		audioSrc.Play();
        gameObject.SetActive(false);

        //Obsolete Code: no longer need lines to be drawn after close line
        //playerCurveLine.SetActive(true);
        //playerTrackerLine.SetActive(true);

        playerTracker.isClose = false;
		dancerTracker.isClose = false;

        endLight.SetActive(true);
	}
}
