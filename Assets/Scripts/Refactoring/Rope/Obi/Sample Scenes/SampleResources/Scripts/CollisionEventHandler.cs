using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Obi;

[RequireComponent(typeof(ObiSolver))]
public class CollisionEventHandler : MonoBehaviour {

 	ObiSolver solver;
	
	Obi.ObiSolver.ObiCollisionEventArgs frame;

	void Awake(){
		solver = GetComponent<Obi.ObiSolver>();
	}

	void OnEnable () {
		solver.OnCollision += Solver_OnCollision;
	}

	void OnDisable(){
		solver.OnCollision -= Solver_OnCollision;
	}
	
	void Solver_OnCollision (object sender, Obi.ObiSolver.ObiCollisionEventArgs e)
	{
		frame = e;
	}

	void OnDrawGizmos()
	{
		if (solver == null || frame == null || frame.contacts == null) return;

		for(int i = 0;  i < frame.contacts.Count; ++i)
		{
			Gizmos.color = (frame.contacts.Data[i].distance < 0.01f) ? Color.red : Color.green;

			Vector3 point = frame.contacts.Data[i].point;
			Vector3 normal = frame.contacts.Data[i].normal;

			Gizmos.DrawSphere(point,0.025f);
	
			Gizmos.DrawRay(point,normal.normalized * 0.1f );//* frame.contacts[i].distance);
		}
	}

}

/*[RequireComponent(typeof(ObiSolver))]
public class CollisionEventHandler : MonoBehaviour {

 	ObiSolver solver;
	
	Obi.ObiSolver.ObiCollisionEventArgs frame;

	void Awake(){
		solver = GetComponent<Obi.ObiSolver>();
	}

	void OnEnable () {
		solver.OnCollision += Solver_OnCollision;
	}

	void OnDisable(){
		solver.OnCollision -= Solver_OnCollision;
	}
	
	void Solver_OnCollision (object sender, Obi.ObiSolver.ObiCollisionEventArgs e)
	{
		
		for(int i = 0;  i < e.contacts.Count; ++i)
		{
			if (e.contacts.Data[i].distance < 0.001f)
			{

				Component collider;
				if (ObiCollider.idToCollider.TryGetValue(e.contacts.Data[i].other,out collider)){

					ObiSolver.ParticleInActor pa = solver.particleToActor[e.contacts.Data[i].particle];
	
					if (pa.actor.colors != null && pa.actor.colors.Length > 0)
						pa.actor.colors[pa.indexInActor] = Color.green;

					collider.gameObject.GetComponent<Renderer>().material.color = new Color(Random.value,Random.value,Random.value);
				}
			}
		}
	}

}*/
