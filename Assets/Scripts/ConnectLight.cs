using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectLight : MonoBehaviour {
    [Header("path1")]
    public GameObject light1;
    public GameObject particle1;

    [Header("Audio")]
    public AudioSource audio;

    public void ShootParicle()
    {
        StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        float startTime = Time.time;
        particle1.GetComponentInChildren<Light>().enabled = true;
        while (Time.time < startTime + 1f)
        {
            float ratio = AnimationUtil.EaseOutQuad((Time.time - startTime) / 1f, 0f, 1f, 1f);
            particle1.GetComponentInChildren<Light>().range += (0.01f + 0.001f * (Time.time - startTime));
            yield return null;
        }

        while (Time.time < startTime + 2f)
        {
            float ratio = AnimationUtil.EaseInQuad((Time.time - startTime) / 1f, 0f, 1f, 1f);
            particle1.GetComponentInChildren<Light>().range -= (0.01f + 0.001f * (Time.time - startTime));
            yield return null;
        }

        particle1.GetComponent<WaypointEmitter>().setDestination(light1);
        particle1.GetComponent<WaypointEmitter>().enabled = true;
        audio.Play();
        yield return null;
    }

    private void Start()
    {
        light1.GetComponent<Light>().intensity = 0.0f;
    }

}
