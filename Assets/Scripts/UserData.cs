using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData {

    public float time { get; set; }
    public Vector3 tracker1pos { get; set; }
    public Vector3 tracker2pos { get; set; }
    public Vector3 dancer1pos { get; set; }
    public Vector3 dancer2pos { get; set; }
    public Vector3 headpos { get; set; }
    public float proximity { get; set; }
    public float alignment { get; set; }
    public float speed1 { get; set; }
    public float speed2 { get; set; }

    public UserData(float time, Vector3 headPos, Vector3 tracker1pos, Vector3 tracker2pos, Vector3 dancer1pos, Vector3 dancer2pos, Vector3 camera, float speed1, float speed2)
    {
        this.time = time;
        this.tracker1pos = tracker1pos;
        this.tracker2pos = tracker2pos;
        this.headpos = headPos;
        this.dancer1pos = dancer1pos;
        this.dancer2pos = dancer2pos;
        this.proximity = (Vector3.Distance(tracker1pos, dancer1pos) + Vector3.Distance(tracker2pos, dancer2pos)) / 2;
        this.alignment = Vector3.Dot((dancer1pos + dancer2pos) / 2, camera);
        this.speed1 = speed1;
        this.speed2 = speed2;
    }

    public UserData(float time, Vector3 headPos, Vector3 tracker1pos, Vector3 tracker2pos, Vector3 dancer1pos, Vector3 dancer2pos, float proximity, float alignment, float speed1, float speed2)
    {
        this.time = time;
        this.tracker1pos = tracker1pos;
        this.tracker2pos = tracker2pos;
        this.headpos = headPos;
        this.dancer1pos = dancer1pos;
        this.dancer2pos = dancer2pos;
        this.proximity = proximity;
        this.alignment = alignment;
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
