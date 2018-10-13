using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Trigger : MonoBehaviour,
    IPrerequisite
{
    public TriggerMgr mgr;

    private bool m_hasTriggered;
    private bool m_isSatisfied = false;

    public bool IsSatisfied()
    {
        return m_isSatisfied;
    }

    public Action<TriggerMgr, Trigger> triggerAction;

    public List<Trigger> dependencies;
    public int id;

    public bool ForceTrigger()
    {
        m_hasTriggered = true;

        // Do something about the actual content
        if (triggerAction != null)
            triggerAction(mgr, this);

        return true;
    }

    public bool ToTrigger()
    {
        // If has been triggered and under processing, ignore the further trigger
        if (m_hasTriggered)
            return false;

        foreach (var dependency in dependencies)
        {
            if (dependency.IsSatisfied() == false)
            {
                print(dependency.gameObject.name + " is not satisfied");
                return false;
            }
        }
        m_hasTriggered = true;

        // Do something about the actual content
        if (triggerAction != null)
            triggerAction(mgr, this);

        return true;
    }

    public void TriggeredCallback()
    {
        print(this.name + " has been triggered");
        m_isSatisfied = true;
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
        if (other.CompareTag("player") == false)
            return;
        ToTrigger();
        //print(this.transform.parent.name + " Collide with " + other.name);
    }
}
