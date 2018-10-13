using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAttractor_Bezier : MonoBehaviour
{
    public Transform start;
    [Range(0,10)]
    public float startRadius;

    public Transform ctrl_1;
    [Range(0, 10)]
    public float ctrl_1Radius;

    public Transform ctrl_2;
    [Range(0, 10)]
    public float ctrl_2Radius;

    public Transform end;
    [Range(0, 10)]
    public float endRadius;

    private ParticleSystem m_particleSys;
    private ParticleSystem.Particle[] m_Particles;
    private int numParticlesAlive;

    public ParticleSystem ParticleSys
    {
        get
        {
            if (m_particleSys == null)
                m_particleSys = GetComponent<ParticleSystem>();
            return m_particleSys;
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
        m_Particles = new ParticleSystem.Particle[ParticleSys.main.maxParticles];
        numParticlesAlive = ParticleSys.GetParticles(m_Particles);

        for (int i = 0; i < numParticlesAlive; i++)
        {
            float t = Mathf.Max(0, 1 - (m_Particles[i].remainingLifetime / m_Particles[i].startLifetime));

            m_Particles[i].position = Vector3.Lerp(m_Particles[i].position,
                CalculateCubicBezierPos(t,
                    GetRandomPointOnSphere(m_Particles[i].randomSeed, start.position,   startRadius),
                    GetRandomPointOnSphere(m_Particles[i].randomSeed, ctrl_1.position,  ctrl_1Radius),
                    GetRandomPointOnSphere(m_Particles[i].randomSeed, ctrl_2.position,  ctrl_2Radius),
                    GetRandomPointOnSphere(m_Particles[i].randomSeed, end.position,     endRadius)), 
                    0.1f);
        }
        ParticleSys.SetParticles(m_Particles, numParticlesAlive);
    }

    private Vector3 CalculateCubicBezierPos(float _t, Vector3 _start, Vector3 _ctrl_1, Vector3 _ctrl_2, Vector3 _end)
    {
        float u = 1 - _t;
        float t2 = _t * _t;
        float u2 = u * u;
        float u3 = u2 * u;
        float t3 = t2 * _t;

        Vector3 p = u3 * _start
                    + t3 * _end
                    + 3 * u2 * _t * _ctrl_1
                    + 3 * u * t2 * _ctrl_2;

        return p;
    }

    private Vector3 GetRandomPointOnSphere(uint _rand, Vector3 _center, float _radius)
    {
        Random.InitState((int)_rand);

        Vector3 randOffset = (Random.onUnitSphere * _radius);//.SetY(0);

        return _center + randOffset;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(start.position, startRadius);
        Gizmos.DrawWireSphere(ctrl_1.position, ctrl_1Radius);
        Gizmos.DrawWireSphere(ctrl_2.position, ctrl_2Radius);
        Gizmos.DrawWireSphere(end.position, endRadius);
    }
}
