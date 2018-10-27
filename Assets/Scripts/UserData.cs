using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData {

    private float time;
    private Vector3 tracker1pos;
    private Vector3 tracker2pos;
    private Vector3 dancer1pos;
    private Vector3 dancer2pos;
    private Vector3 headpos;
    private float proximity;
    private float alignment;
    private float speed1;
    private float speed2;

    public UserData(float time, Vector3 headPos, Vector3 tracker1pos, Vector3 tracker2pos, Vector3 dancer1pos, Vector3 dancer2pos, Vector3 camera, float speed1, float speed2)
    {
        this.time = time;
        this.tracker1pos = tracker1pos;
        this.tracker2pos = tracker2pos;
        this.headpos = headPos;
        this.dancer1pos = dancer1pos;
        this.dancer2pos = dancer2pos;
        this.proximity = Vector3.Distance(tracker1pos, dancer1pos) + Vector3.Distance(tracker2pos, dancer2pos) / 2;
        this.alignment = Vector3.Dot(dancer1pos + dancer2pos / 2, camera);
        this.speed1 = speed1;
        this.speed2 = speed2;
    }

    public override string ToString()
    {
        string pos1 = tracker1pos.ToString();
        pos1 = pos1.Substring(1, pos1.Length - 2);
        string pos2 = tracker2pos.ToString();
        pos2 = pos2.Substring(1, pos2.Length - 2);
        string pos3 = dancer1pos.ToString();
        pos3 = pos3.Substring(1, pos3.Length - 2);
        string pos4 = dancer2pos.ToString();
        pos4 = pos4.Substring(1, pos4.Length - 2);
        string head = headpos.ToString();
        head = head.Substring(1, head.Length - 2);
        return time + "," + head + "," + pos1 + "," + pos2 + "," + pos3 + ","
               + pos4 + "," + proximity + "," + alignment + "," + speed1 + "," + speed2;
    }

}
