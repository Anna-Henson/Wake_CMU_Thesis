using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectLight : MonoBehaviour {
    [Header("path1")]
    public GameObject light1;
    public GameObject particle1;
    [Header("path2")]
    public GameObject light2;
    public GameObject particle2;

    public void ShootParicle()
    {
        StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        
        var emission = particle1.GetComponent<ParticleSystem>().emission;
        emission.enabled = true;
        particle1.GetComponentInChildren<Light>().enabled = true;
        particle1.GetComponent<WaypointEmitter>().setDestination(light1);
        particle1.GetComponent<WaypointEmitter>().enabled = true;

        yield return new WaitForSeconds(1);
        var emission2 = particle2.GetComponent<ParticleSystem>().emission;
        emission2.enabled = true;
        particle2.GetComponentInChildren<Light>().enabled = true;
        particle2.GetComponent<WaypointEmitter>().setDestination(light2);
        particle2.GetComponent<WaypointEmitter>().enabled = true;
    }

}
