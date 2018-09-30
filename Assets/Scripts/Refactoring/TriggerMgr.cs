using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriggerMgr : MonoBehaviour
{
    public TasiYokan.Curve.BezierCurve curve;
    public PlayerPathDrawer playerDrawer;

    public List<Trigger> triggers;
    private List<Action<Trigger>> triggerCallbacks = new List<Action<Trigger>>
    {
        (trigger)=>{
            print("0");
            trigger.TriggeredCallback();
        },
        (trigger)=>{
            print("1");
            trigger.TriggeredCallback();
        },
        (trigger)=>{
            print("2");
            trigger.TriggeredCallback();
        },
        (trigger)=>{
            print("3");
            trigger.TriggeredCallback();
        },
        (trigger)=>{
            print("4");
            trigger.TriggeredCallback();
        },
        (trigger)=>{
            print("5");
            trigger.TriggeredCallback();
        },
    };

    private void SetAnchorToNext(Trigger curTrigger)
    {
        // Set bezier to next trigger point as its target
        curve.SetAnchorPosition(1, triggers[curTrigger.id + 1].transform.position);

        Vector3 targetOutDir = (playerDrawer.player.position - curve.Points[1].Position).SetY(0);
        playerDrawer.onLeft = Vector3.Cross(playerDrawer.player.forward.SetY(0), targetOutDir).y.Sgn();
    }

    private void Start()
    {
        triggers = GetComponentsInChildren<Trigger>().ToList();

        for (int i = 0; i < triggers.Count; ++i)
        {
            triggers[i].id = i;
            if (triggerCallbacks.Count > i)
                triggers[i].triggerAction = triggerCallbacks[i];

            if (i < triggers.Count - 1)
                triggers[i].triggerAction += SetAnchorToNext;

            if (i > 0)
                triggers[i].dependencies.Add(triggers[i - 1]);

        }
        int a = 1;
    }
}
