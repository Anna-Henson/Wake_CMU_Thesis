using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class TriggerMgr : MonoBehaviour
{
    private List<KeyCode> keys = new List<KeyCode>()
    {
        KeyCode.Alpha0,
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8
    };
    public TasiYokan.Curve.BezierCurve curve;
    public PlayerPathDrawer playerDrawer;
    public MeshRenderer RS_PlaneRenderer;
    public Shader RS_Shader_OnTop;
    public FakeMagnetHook hook;
    public Transform anotherTracker;
    public Transform dancer;
    [Header("Light Controls")]
    public Light lightToTurnOn;
    public Light startLight;

    [Header("Particles")]
    public ParticleSystem ParticleOnWaypoint1;
    public ParticleSystem ParticleOnWaypoint2;
    public ParticleSystem ParticleOnWayPoint3;
    public ParticleSystem ParticleOnWayPoint4;

    // current playing audio
    private AudioSource audio;

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

            mgr.StartCoroutine(mgr.ReachWaypointAndPlayAudio(trigger, () =>
            {
                print("Complete 0");
                // Here we enter Stage2 where we connect the player to next way point, until we reach waypoint03(#4 indeed)
                mgr.hook.SetBackToOriginParent();
                mgr.SetAnchorToNext(trigger);
                trigger.TriggeredCallback();
            }));
             mgr.StartCoroutine(mgr.ParticleOn(mgr.ParticleOnWaypoint1));
        },
        (mgr, trigger)=>{
            print("1");

            mgr.StartCoroutine(mgr.ReachWaypointAndPlayAudio(trigger, () =>
            {
                print("Complete 1");
                mgr.SetAnchorToNext(trigger);
                trigger.TriggeredCallback();
            }));

            mgr.StartCoroutine(mgr.EmitParticle(mgr.ParticleOnWaypoint1));
            mgr.StartCoroutine(mgr.ParticleOn(mgr.ParticleOnWaypoint2));


        },
        (mgr, trigger)=>{
            print("2");

            mgr.StartCoroutine(mgr.ReachWaypointAndPlayAudio(trigger, () =>
            {
                print("Complete 2");
                mgr.SetAnchorToNext(trigger);
                trigger.TriggeredCallback();
            }));

             mgr.StartCoroutine(mgr.EmitParticle(mgr.ParticleOnWaypoint2));
             mgr.StartCoroutine(mgr.ParticleOn(mgr.ParticleOnWayPoint3));
        },
        (mgr, trigger)=>{
            print("3");

            mgr.StartCoroutine(mgr.FadeIn(5f));
            mgr.StartCoroutine(mgr.LightOn(5f));

            mgr.StartCoroutine(mgr.ReachWaypointAndPlayAudio(trigger, () =>
            {
                print("Complete 3");
                mgr.SetAnchorToNext(trigger);
                trigger.TriggeredCallback();
            }));

            mgr.StartCoroutine(mgr.EmitParticle(mgr.ParticleOnWayPoint3));
            mgr.StartCoroutine(mgr.ParticleOn(mgr.ParticleOnWayPoint4));
        },
        (mgr, trigger)=>{
            print("4");

            mgr.StartCoroutine(mgr.ReachWaypointAndPlayAudio(trigger, () =>
            {
                print("Complete 4");
                //mgr.SetAnchorToNext(trigger);
                trigger.TriggeredCallback();

                // Here we enter Stage3, we connect the player and dancer
                mgr.ConnectToDancer();
            }));

            mgr.StartCoroutine(mgr.EmitParticle(mgr.ParticleOnWayPoint4));

        },
        (mgr, trigger)=>{
            print("5");

            //mgr.SetAnchorToNext(trigger);
            trigger.TriggeredCallback();
        },
        (mgr, trigger)=>{
            print("6");

            //mgr.SetAnchorToNext(trigger);
            trigger.TriggeredCallback();
        },
        (mgr, trigger)=>{
            print("7");

            mgr.StartCoroutine(mgr.ReachWaypointAndPlayAudio(trigger, () =>
            {
                print("Complete 7");
                //mgr.SetAnchorToNext(trigger);
                trigger.TriggeredCallback();
            }));
        },
        (mgr, trigger)=>{
            print("8");

            mgr.RS_PlaneRenderer.material.shader = mgr.RS_Shader_OnTop;
        },
    };

    private IEnumerator FadeIn(float _duration)
    {
        float startTime = Time.time;
        while (Time.time < startTime + _duration)
        {
            RS_PlaneRenderer.material.SetFloat("_FadeOut", (Time.time - startTime) / _duration);
            yield return null;
        }
    }

    private IEnumerator FadeOut(float _duration)
    {
        float startTime = Time.time;
        while(Time.time < startTime + _duration)
        {
            RS_PlaneRenderer.material.SetFloat("_FadeOut", 1.0f - (Time.time - startTime) / _duration);
            yield return null;
        }
    }
    //Lights up the first spot light closest to the end when the camera turned on.
    private IEnumerator LightOn(float _duration)
    {
        float startTime = Time.time;
        while (Time.time < startTime + _duration)
        {
            Debug.Log("In Light ON");
            lightToTurnOn.intensity = 2.7f * (Time.time - startTime) / _duration;
            yield return null;
        }
    }

    private IEnumerator ParticleOn(ParticleSystem particle)
    {
        var emission = particle.emission;
        emission.enabled = true;
        yield return null;
    }

    private IEnumerator EmitParticle(ParticleSystem particle)
    {

        particle.GetComponent<ConnectLight>().ShootParicle();
        yield return null;
    }

    private IEnumerator ReachWaypointAndPlayAudio(Trigger _trigger, Action _callback)
    {

        // Once you reach the way point, you should hide the hook
        hook.DetachHook();

        // Start play audio and wait until end
        audio = _trigger.GetComponent<AudioSource>();
        if (audio == null)
            yield return null;
        else
        {
            audio.Play();
            while (audio.timeSamples + BufferLength < audio.clip.samples)
            {
                yield return null;
            }

             //For quick debug only
            //yield return new WaitForSeconds(3);
            //audio.Stop();

            _callback();
        }
    }

    private void SetAnchorToNext(Trigger curTrigger)
    {
        // Has already reached the end
        if (curTrigger.id >= triggers.Count - 1)
            return;

        // Attach the magnet hook
        ConnectToWaypoint(triggers[curTrigger.id + 1].transform);
    }

    private void ConnectTwoTrackers()
    {
        hook.StartCoroutine(hook.AttachHookAsChild(anotherTracker));
    }

    private void ConnectToWaypoint(Transform target)
    {
        print("Connect " + target.name);
        hook.StartCoroutine(hook.AttachHook(target));
        //hook.AttachHookStatic(target);
    }

    private void ConnectToDancer()
    {
        hook.StartCoroutine(hook.AttachHookAsChildAfter(dancer));
    }

    private void PlayStartAudio()
    {
        var startAudio = startLight.GetComponent<AudioSource>();
        if (startAudio != null)
        {
            startAudio.Play();
            float audioLength = startAudio.clip.length;
            StartCoroutine(StartLightOn(audioLength));
        }
    }

    private IEnumerator StartLightOn(float audioLength)
    {
        yield return new WaitForSeconds(audioLength);
        float startTime = Time.time;
        while(startLight.intensity < 2.17f)
        {
            startLight.intensity = 2.17f * (Time.time - startTime) / 5f;
            startLight.gameObject.GetComponent<HxVolumetricLight>().Intensity = 2.17f * (Time.time - startTime) / 5f;
            yield return null;
        }
    }

    private void Start()
    {
        triggers = GetComponentsInChildren<Trigger>().ToList();

        // On Stage1, we connect the two trackers of player
        ConnectTwoTrackers();

        for (int i = 0; i < triggers.Count; ++i)
        {
            triggers[i].id = i;
            triggers[i].mgr = this;

            if (triggerCallbacks.Count > i)
                triggers[i].triggerAction = triggerCallbacks[i];

            if (i > 0)
                triggers[i].dependencies.Add(triggers[i - 1]);

        }

        List<ParticleSystem> particles = new List<ParticleSystem>
        {
            ParticleOnWaypoint1,
            ParticleOnWaypoint2,
            ParticleOnWayPoint3,
            ParticleOnWayPoint4
        };
        for (int i = 0; i < particles.Count; i++)
        {
            var emission = particles[i].emission;
            emission.enabled = false;
        }
    }

    private void Update()
    {
        for (int i = 0; i < keys.Count; ++i)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                print("Manually trigger " + keys[i]);
                if (audio != null)
                    audio.Stop();
                StopAllCoroutines();

                if (i > 0)
                    SetAnchorToNext(triggers[i - 1]);
                triggers[i].ForceTrigger();
            }
        }
        
        //Start the Experience
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space Pressed");
            var startAudio = startLight.GetComponent<AudioSource>();
            if (startAudio != null)
            {
                PlayStartAudio();
            } else
            {
                Debug.Log("Did not find start audio on start light.");
            }
        }

        //End the experience
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            StartCoroutine(FadeOut(5f));
        }
    }
}
