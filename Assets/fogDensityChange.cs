using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class fogDensityChange : MonoBehaviour {
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;
    public GameObject particleSysObject;
    private Queue<float> normVels = new Queue<float>();
    public const float desiredMaxVelocity = 20f;

	// Use this for initialization
	void Start () {
        trackedObject = GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObject.index);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 vel = device.velocity;
        float normVel = vel.magnitude * 100f;
        HxVolumetricParticleSystem particleSys = particleSysObject.GetComponent<HxVolumetricParticleSystem>();

        if (normVels.Count == 60)
        {
            normVels.Dequeue();    
        }
        normVels.Enqueue(normVel);
        normVel = normVels.Sum() / normVels.Count;

        if (normVel > desiredMaxVelocity)
        {
            particleSys.DensityStrength -= 0.01f;
            if (particleSys.DensityStrength < 0)
            {
                particleSys.DensityStrength = 0.0f;
            }
        }
        else
        {
            particleSys.DensityStrength += 0.01f;
            if (particleSys.DensityStrength > 4.0f)
            {
                particleSys.DensityStrength = 4.0f;
            }
        }
        
        
        
	}
}
