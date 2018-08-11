using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour {

  private AudioSource audioSource;
  private bool played = false;
  public int index;
  public LineDataCopy lineData;

  //Only used for waypoint 0
  public GameObject curvePlayer;
  public GameObject trackerPlayer;
  public GameObject curveDancer;
  public GameObject trackerDancer;
  public GameObject firstPoint;


  [ContextMenu("Play")]
  public void Play()
  {
    if (!played)
    {
      audioSource.Play();
      played = true;
      float audioLength = audioSource.clip.length;
      Invoke("NextWaypoint", audioLength);
    }
   
  }

  private void NextWaypoint()
  {
    if (index == 0)
    {
      SetLinesActive(true);
      firstPoint.SetActive(false);
    }

    lineData.ReachIndex(index);
  }

  private void SetLinesActive(bool active)
  {
    curvePlayer.SetActive(active);
    trackerPlayer.SetActive(active);
    curveDancer.SetActive(active);
    trackerDancer.SetActive(active);
  }

	// Use this for initialization
	void Start () {
    if (index == 0)
    {
      SetLinesActive(false);
    }
    audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
