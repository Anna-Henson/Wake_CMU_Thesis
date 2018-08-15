using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableCloseLine : MonoBehaviour {

	public GameObject dancerObj;
	public GameObject playerCurveLine;
	public GameObject playerTrackerLine;
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
		dancerObj.SetActive(false);
		audioSrc.Play();
		gameObject.SetActive(false);
		playerCurveLine.SetActive(true);
		playerTrackerLine.SetActive(true);
	}
}
