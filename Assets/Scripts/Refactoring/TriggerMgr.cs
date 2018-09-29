using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriggerMgr : MonoBehaviour
{
    public List<Trigger> triggers;
    private List<Action> triggerCallbacks = new List<Action>
    {
        ()=>{print("0"); },
        ()=>{print("1"); },
        ()=>{print("2"); },
        ()=>{print("3"); },
        ()=>{print("4"); },
        ()=>{print("5"); },
    };

    private void Start()
    {
        triggers = GetComponentsInChildren<Trigger>().ToList();

        for (int i = 0; i < triggers.Count; ++i)
        {
            if (triggerCallbacks.Count > i)
                triggers[i].triggerCallback = triggerCallbacks[i];
        }
    }
}
