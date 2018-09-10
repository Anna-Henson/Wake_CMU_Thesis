using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour {

	private AudioSource audioSource;
	private bool playedPlayer = false;
	private bool playedDancer = false;
	public int index;
	public LineDataCopy playerLineData;
	public LineDataCopy dancerLineData;
    private GameObject endLight;

	public KeyCode activateKey;

    [Header("Light to Trigger at Index 6")]
    public GameObject tableLight;

	[Header("Only for Index 0")]
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
    if (!playedPlayer && CanPlay(true))
    {
      //Debug log here. Delete Later.
      Debug.Log("Triggered" + index);

      if(index == 6)
      {
        tableLight.SetActive(true);
      }

      audioSource.Play();
      playedPlayer = true;
      float audioLength = audioSource.clip.length;

      //for testing
      //Invoke("NextWaypointPlayer", audioLength);
      NextWaypointPlayer();
    }
  }

	private void NextWaypointPlayer()
	{
		NextWaypoint(true);
	}

  public void ReachWithDancer()
  {
	if (!playedDancer && CanPlay(false))
	{
	  playedDancer = true;
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
	
	private void Update ()
	{
      if (Input.GetKeyDown(activateKey))
	  {
		Play();
	  }
	}
}
