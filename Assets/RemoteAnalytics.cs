using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAnalyticsHeatmap;

public class RemoteAnalytics : MonoBehaviour {
    public GameObject hmd;
    public GameObject cameraobj;
    public GameObject playerTracker1;
    public GameObject playerTracker2;
    public GameObject dancerTracker1;
    public GameObject dancerTracker2;
    private SteamVR_Controller.Device device1;
    private SteamVR_Controller.Device device2;
    private UserData averages;
    private Queue<UserData> record;

    private float startTime;



    private IEnumerator RenewData()
    {
        
        while (true)
        {
           
            float timeSinceLoad = Time.time - startTime;
            float speed1;
            float speed2;
            if (device1 == null || device2 == null)
            {
                speed1 = 0;
                speed2 = 0;

            }
            else
            {
                speed1 = Vector3.Magnitude(device1.velocity);
                speed2 = Vector3.Magnitude(device2.velocity);
            }
            UserData position = new UserData(timeSinceLoad, hmd.transform.position, playerTracker1.transform.position,
                playerTracker2.transform.position, dancerTracker1.transform.position, dancerTracker2.transform.position,
                cameraobj.transform.forward, speed1, speed2);
            
            if (record.Count == 0)
            {
                Debug.Log("Record == " + record.Count);
                averages = position;
            }
            
            if (record.Count < 100)
            {
                Debug.Log("Record2 == " + record.Count);
                UserData newAverage = CalculateAverage(position);
                averages = newAverage;
                record.Enqueue(position);
            }
            else if (record.Count >= 100)
            {
                
                SendDataToServer();
                record.Clear();
                averages = position;
                record.Enqueue(position);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private UserData CalculateAverage(UserData p)
    {
        int count = record.Count;
        Vector3 t1p = (averages.tracker1pos * count + p.tracker1pos) / (count + 1);
        Vector3 t2p = (averages.tracker2pos * count  + p.tracker2pos) / (count + 1);
        Vector3 h = (averages.headpos * count + p.headpos) / (count + 1);
        Vector3 d1p = (averages.dancer1pos * count + p.dancer1pos) / (count + 1);
        Vector3 d2p = (averages.dancer2pos * count + p.dancer2pos) / (count + 1);
        float pr = (averages.proximity * count + p.proximity) / (count + 1);
        float al = (averages.alignment * count + p.alignment) / (count + 1);
        float s1 = (averages.speed1 * count + p.speed1) / (count + 1);
        float s2 = (averages.speed2 * count + p.speed2) / (count + 1);

        return new UserData(p.time, h, t1p, t2p, d1p, d2p, pr, al, s1, s2);
    }

    private void SendDataToServer()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();

        dict.Add("dancerPos", (averages.dancer1pos + averages.dancer2pos) /2);
        dict.Add("proximity", averages.proximity);
        dict.Add("aligment", averages.alignment);
        dict.Add("speed1", averages.speed1);
        dict.Add("speed2", averages.speed2);

        Debug.Log("renew data");
        HeatmapEvent.Send("PlayerPos", averages.headpos, Time.time - startTime, dict);
    }


    private void Start()
    {
        startTime = Time.time;
        record = new Queue<UserData>();
        StartCoroutine(RenewData());
    }
    // Update is called once per frame
    void Update()
    {

    }
}
