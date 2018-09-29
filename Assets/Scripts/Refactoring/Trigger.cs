using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Trigger : MonoBehaviour,
    IPrerequisite
{
    private bool m_hasTriggered;

    public bool IsSatisfied()
    {
        return m_hasTriggered;
    }

    public Action triggerCallback;

    public List<Trigger> dependencies;

    public void ToTrigger()
    {
        foreach (var dependency in dependencies)
        {
            if (dependency.IsSatisfied() == false)
                return;
        }

        // Do something about the actual content
        triggerCallback();
    }

    // Use this for initialization
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        print(this.transform.parent.name + " Collide with " + other.name);
    }
}
