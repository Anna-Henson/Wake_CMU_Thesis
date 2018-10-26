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

        //Animated Light
        float currentRange = particle1.GetComponentInChildren<Light>().range;
        AnimationCurve curve = AnimationCurve.EaseInOut(startTime, currentRange, startTime + 1f, 1.5f);

        while (Time.time < startTime + 1f)
        {
            particle1.GetComponentInChildren<Light>().range = curve.Evaluate(Time.time);
            yield return null;
        }

        startTime = Time.time;
        curve = AnimationCurve.EaseInOut(startTime, 1.5f , startTime + 1.5f, currentRange);
        while (Time.time < startTime + 1.5f)
        {
            particle1.GetComponentInChildren<Light>().range = curve.Evaluate(Time.time);
            yield return null;
        }

        //Shoot the light out
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
