using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectLight : MonoBehaviour {
    [Header("path1")]
    public GameObject light1;
    public GameObject particle1;
    //[Header("path2")]
    //public GameObject light2;
    //public GameObject particle2;

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
            particle1.GetComponentInChildren<Light>().range += (0.01f + 0.001f * (Time.time - startTime));
            yield return null;
        }

        while (Time.time < startTime + 2f)
        {
            particle1.GetComponentInChildren<Light>().range -= (0.01f + 0.001f * (Time.time - startTime));
            yield return null;
        }

        particle1.GetComponent<WaypointEmitter>().setDestination(light1);
        particle1.GetComponent<WaypointEmitter>().enabled = true;
        yield return null;
    }

}
