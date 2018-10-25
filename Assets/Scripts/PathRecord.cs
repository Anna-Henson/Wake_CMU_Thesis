using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRecord : MonoBehaviour {

    public GameObject trackerPlayer;
    public LineRenderer pathRenderer;
    public GameObject renderPlane;

    //Dancer's Trackers
    [Header("Dancer's Trackers")]
    public GameObject tracker1;
    public GameObject tracker2;

    [Header("Parameters")]
    public float durationInLight = 15f;
    public float durationInDark = 5f;

    private List<Vector3> path = new List<Vector3>() { };
    private float startTime; 
    private bool isTracking;

    private IEnumerator DrawPath(float duration)
    {

        Debug.Log("Count:" + path.Count);
        List<Vector3> renderedPath = new List<Vector3>() { };

        float sTime = Time.time;

        while (Time.time  < sTime  + duration)
        {
            
            //yield return new WaitForSeconds(0.01f);
            int index = (int)Mathf.Ceil(((Time.time - sTime) / duration) * path.Count) - 1;
            try
            {
              
                renderedPath.Add(path[index]);
                pathRenderer.positionCount = renderedPath.Count;
                pathRenderer.SetPositions(renderedPath.ToArray());
            } catch (ArgumentOutOfRangeException e)
            {
                Debug.Log(index);
            }
            yield return null;
            
        }

        StartCoroutine(TurnLightOff());

        yield return null;
    }

    private IEnumerator TurnLightOff()
    {
        Light[] lights = GameObject.FindObjectsOfType<Light>();
        foreach(Light light in lights)
        {
            StartCoroutine(LightOff(light));
        }
        yield return null;
    }

    private IEnumerator LightOff(Light light)
    {
        float duration = 25f;
        float startTime = Time.time;
        float startIntensity = light.intensity;
        Debug.Log("In Light Off");
        while (Time.time < startTime + duration)
        {
            light.intensity = startIntensity - startIntensity * (Time.time - startTime) / duration;
            yield return null;
        }
        light.intensity = 0.0f;
        StartCoroutine(TurnPathOff());
    }

    private IEnumerator TurnPathOff()
    {
        float duration = 10f;
        float startTime = Time.time;
        float startIntensity = pathRenderer.material.GetFloat("_InvFade");
        Debug.Log("In Path Off");
        while (Time.time < startTime + duration)
        {
            float fade = startIntensity - startIntensity * (Time.time - startTime) / duration;
            pathRenderer.material.SetFloat("_InvFade", fade);
            yield return null;
        }
        pathRenderer.material.SetFloat("_InvFade", 0.0f);
    }
    private void Start()
    {
        isTracking = true;
        startTime = Time.time;
        renderPlane.GetComponent<MeshRenderer>().enabled = false;
        pathRenderer.enabled = false;
    }

    void Update () {
		if (isTracking && Time.time - startTime > 0.1f)
        {
            startTime = Time.time;
            path.Add(trackerPlayer.transform.position);
        }

        //Stop Tracking Location When Camera Fades out
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            isTracking = false;
            pathRenderer.enabled = true;
            StartCoroutine(DrawPath(durationInLight));
            GameObject[] particles =  GameObject.FindGameObjectsWithTag("pathParticle");
            Debug.Log(particles);
            foreach(GameObject particle in particles)
            {
                particle.GetComponent<MeshRenderer>().enabled = false;
            }
        }
	}
}
