using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriggerMgr : MonoBehaviour
{
    public List<Trigger> triggers;
    private List<Action<Action>> triggerCallbacks = new List<Action<Action>>
    {
        (callback)=>{
            print("0");
            callback();
        },
        (callback)=>{
            print("1");
            callback();
        },
        (callback)=>{
            print("2");
            callback();
        },
        (callback)=>{
            print("3");
            callback();
        },
        (callback)=>{
            print("4");
            callback();
        },
        (callback)=>{
            print("5");
            callback();
        },
    };

    private void Start()
    {
        triggers = GetComponentsInChildren<Trigger>().ToList();

        for (int i = 0; i < triggers.Count; ++i)
        {
            if (triggerCallbacks.Count > i)
                triggers[i].triggerAction = triggerCallbacks[i];

            if (i > 0)
                triggers[i].dependencies.Add(triggers[i - 1]);
        }
        int a = 1;
    }
}
