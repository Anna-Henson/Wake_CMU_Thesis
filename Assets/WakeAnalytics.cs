using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Analytics;

public class WakeAnalytics : MonoBehaviour {

    public string filePath;
    public GameObject hmd;
    public GameObject cameraobj;
    public GameObject playerTracker1;
    public GameObject playerTracker2;
    public GameObject dancerTracker1;
    public GameObject dancerTracker2;
    private SteamVR_Controller.Device device1;
    private SteamVR_Controller.Device device2;

    private float startTime;

	
    private IEnumerator SendPositionData()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            float timeSinceLoad = Time.time - startTime;
            float speed1;
            float speed2;
            if (device1 == null || device2 == null)
            {
                speed1 = 0;
                speed2 = 0;
               
            } else
            {
                speed1 = Vector3.Magnitude(device1.velocity);
                speed2 = Vector3.Magnitude(device2.velocity);
            }
            UserData position = new UserData(timeSinceLoad, hmd.transform.position, playerTracker1.transform.position, 
                playerTracker2.transform.position, dancerTracker1.transform.position, dancerTracker2.transform.position, 
                cameraobj.transform.forward, speed1, speed2);
            
            WriteToLog(position.ToString());
        }
    }

    private void WriteToLog(string data)
    {
        string path = @filePath;
        if (!File.Exists(path))
        {
            File.Create(path).Dispose();

            using (TextWriter tw = new StreamWriter(path, true))
            {
                tw.WriteLine(data);
                tw.Close();
            }
        } else if (File.Exists(path))
        {
            using (var tw = new StreamWriter(path, true))
            {
                tw.WriteLine(data);
            }
        }
    }

    private void Start()
    {
        startTime = Time.time;
        StartCoroutine(SendPositionData());
        var tracker1 = playerTracker1.GetComponent<SteamVR_TrackedObject>();
        device1 = SteamVR_Controller.Input((int)tracker1.index);
        var tracker2 = playerTracker2.GetComponent<SteamVR_TrackedObject>();
        device2 = SteamVR_Controller.Input((int)tracker2.index);
    }
    // Update is called once per frame
    void Update () {
		
	}
}
