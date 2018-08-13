using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour {

	private AudioSource audioSource;
	private bool played = false;
	public int index;
	public LineDataCopy playerLineData;
	public LineDataCopy dancerLineData;

	//Only used for waypoint 0
	public GameObject curvePlayer;
	public GameObject trackerPlayer;
	public GameObject curveDancer;
	public GameObject trackerDancer;
	public GameObject firstPoint;


	private LineDataCopy GetLineData(bool player)
	{
		return player ? playerLineData : dancerLineData;
	}

	private bool CanPlay(bool player)
	{
		LineDataCopy ld = GetLineData(player);
		return ld.GetIndex() == index || ld.GetIndex() + ld.sign == index;
	}

  [ContextMenu("Play")]
  public void Play()
  {
    if (!played && CanPlay(true))
    {
      audioSource.Play();
      played = true;
      float audioLength = audioSource.clip.length;
      Invoke("NextWaypointPlayer", audioLength);
    }
   
  }

	private void NextWaypointPlayer()
	{
		NextWaypoint(true);
	}

  public void ReachWithDancer()
  {
	if (!played && CanPlay(false))
	{
	  played = true;
	  NextWaypoint(false);
	}
  }
  
  private void NextWaypoint(bool player)
  {
    if (index == 0)
    {
      SetLinesActive(true);
      firstPoint.SetActive(false);
    }

	LineDataCopy ld = GetLineData(player);
    ld.ReachIndex(index);
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
