using UnityEngine;
using System.Collections;

public class WaypointEmitter: MonoBehaviour
{
    private GameObject destination;
    private float intensity = 0.0f;
    private float startLightingUpDist = 0.2f;

    void Start()
    {
        Vector3[] path = new Vector3[] {gameObject.transform.position, destination.transform.position };
        try
        {
            iTween.MoveTo(this.gameObject, iTween.Hash("path", path, "easeType", "easeOutExpo", "loopType", "None", "delay", .1, "time", 10));
        } catch(System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    //Set the destination of the game object
    public void setDestination(GameObject destination)
    {
        this.destination = destination;
    }

    //When the shoot-out particle is in place
    public void Update()
    {
        
        if (Mathf.Abs(gameObject.transform.position.z - destination.transform.position.z) < startLightingUpDist)
        {
            Debug.Log("Lighting up");
            Debug.Log(destination.GetComponent<Light>());
            destination.GetComponent<Light>().intensity = Mathf.Min(2.7f, intensity += 0.2f);
        }
          
    }

}