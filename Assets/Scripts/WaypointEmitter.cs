using UnityEngine;
using System.Collections;

public class WaypointEmitter: MonoBehaviour
{
    private GameObject destination;
    private float intensity = 0.0f;
    private float startLightingUpDist = 0.5f;
    private bool started = false;

    void Start()
    {
        Vector3[] path = new Vector3[] {gameObject.transform.position, destination.transform.position };
        try
        {
            iTween.MoveTo(this.gameObject, iTween.Hash("path", path, "easeType", "easeOutExpo", "loopType", "None", "delay", .1, "time", 8));
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

    private IEnumerator LightOn()
    {
        float startTime = Time.time;
        const float duration = 3f;
        while (Time.time < startTime + duration)
        {
            float ratio = (Time.time - startTime) / duration;
            float intensity = AnimationUtil.EaseInOutQuad(ratio, 0, 1, 1);
            destination.GetComponent<Light>().intensity = intensity;
            yield return null;
        }
        
    }

    private IEnumerator HaloOff()
    {
        float startTime = Time.time;
        const float duration = 3f;
        while (Time.time < startTime + duration)
        {
            float ratio = (Time.time - startTime) / duration;
            GetComponentInChildren<Light>().intensity = 2.07f - ratio * 2.07f;
            yield return null;
        }
    }

    //When the shoot-out particle is in place
    public void Update()
    {
        
        if (Vector3.Distance(gameObject.transform.position, destination.transform.position) < startLightingUpDist && !started)
        {
            StartCoroutine(LightOn());
            StartCoroutine(HaloOff());
            started = true;
        }
          
    }

}