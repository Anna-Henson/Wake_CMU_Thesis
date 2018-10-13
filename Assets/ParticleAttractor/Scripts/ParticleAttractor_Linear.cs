using System.Collections;
using UnityEngine;
[RequireComponent(typeof(ParticleSystem))]
public class ParticleAttractor_Linear : MonoBehaviour
{
    private ParticleSystem m_particleSys;
    public ParticleSystem ParticleSys
    {
        get
        {
            if (m_particleSys == null)
                m_particleSys = GetComponent<ParticleSystem>();
            return m_particleSys;
        }
    }
    ParticleSystem.Particle[] m_Particles;
    public Transform target;
    public float speed = 5f;
    int numParticlesAlive;

    void Start()
    {
    }

    void Update()
    {
        m_Particles = new ParticleSystem.Particle[ParticleSys.main.maxParticles];
        numParticlesAlive = ParticleSys.GetParticles(m_Particles);
        float step = speed * Time.deltaTime;
        for (int i = 0; i < numParticlesAlive; i++)
        {
            m_Particles[i].position = Vector3.SlerpUnclamped(m_Particles[i].position, target.position, step);
        }
        ParticleSys.SetParticles(m_Particles, numParticlesAlive);
    }
}
