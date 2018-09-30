using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class TriggerMgr : MonoBehaviour
{
    public TasiYokan.Curve.BezierCurve curve;
    public PlayerPathDrawer playerDrawer;

    private static int m_bufferLength;
    public static int BufferLength
    {
        get
        {
            if (m_bufferLength == 0)
            {
                // We need to know the dsp buffer size so we can schedule playback correctly
                // AudioSettings.GetDSPBufferSize is effectively a forwarded call 
                // to FMOD::System::getDSPBufferSize. It defaults to 1024 sample frames on PC.
                int numBuffers;
                AudioSettings.GetDSPBufferSize(out m_bufferLength, out numBuffers);
            }
            return m_bufferLength;
        }
    }

    public List<Trigger> triggers;
    private List<Action<TriggerMgr, Trigger>> triggerCallbacks = new List<Action<TriggerMgr, Trigger>>
    {
        (mgr, trigger)=>{
            print("0");

            mgr.StartCoroutine(mgr.PlayAudio(trigger, () =>
            {
                print("Complete 0");
                mgr.SetAnchorToNext(trigger);
                trigger.TriggeredCallback();
            }));
        },
        (mgr, trigger)=>{
            print("1");

            mgr.StartCoroutine(mgr.PlayAudio(trigger, () =>
            {
                print("Complete 1");
                mgr.SetAnchorToNext(trigger);
                trigger.TriggeredCallback();
            }));
        },
        (mgr, trigger)=>{
            print("2");

            mgr.StartCoroutine(mgr.PlayAudio(trigger, () =>
            {
                print("Complete 2");
                mgr.SetAnchorToNext(trigger);
                trigger.TriggeredCallback();
            }));
        },
        (mgr, trigger)=>{
            print("3");

            mgr.StartCoroutine(mgr.PlayAudio(trigger, () =>
            {
                print("Complete 3");
                mgr.SetAnchorToNext(trigger);
                trigger.TriggeredCallback();
            }));
        },
        (mgr, trigger)=>{
            print("4");

            mgr.StartCoroutine(mgr.PlayAudio(trigger, () =>
            {
                print("Complete 4");
                mgr.SetAnchorToNext(trigger);
                trigger.TriggeredCallback();
            }));
        },
        (mgr, trigger)=>{
            print("5");

            mgr.SetAnchorToNext(trigger);
            trigger.TriggeredCallback();
        },
        (mgr, trigger)=>{
            print("6");

            mgr.SetAnchorToNext(trigger);
            trigger.TriggeredCallback();
        },
        (mgr, trigger)=>{
            print("7");

            mgr.StartCoroutine(mgr.PlayAudio(trigger, () =>
            {
                print("Complete 7");
                mgr.SetAnchorToNext(trigger);
                trigger.TriggeredCallback();
            }));
        },
    };

    private IEnumerator PlayAudio(Trigger _trigger, Action _callback)
    {
        LineRenderer line = curve.GetComponent<LineRenderer>();
        line.enabled = false;
        AudioSource audio = _trigger.GetComponent<AudioSource>();
        if (audio == null)
            yield return null;
        else
        {
            audio.Play();
            while (audio.timeSamples + BufferLength < audio.clip.samples)
            {
                yield return null;
            }
            line.enabled = true;
            _callback();
        }
    }

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
            triggers[i].mgr = this;

            if (triggerCallbacks.Count > i)
                triggers[i].triggerAction = triggerCallbacks[i];

            if (i > 0)
                triggers[i].dependencies.Add(triggers[i - 1]);

        }
        int a = 1;
    }
}
