using System;
using System.Collections;
using UnityEngine;
using Obi;

public class RuntimeRopeGenerator
{
	private ObiRope rope;
	private ObiRopeCursor cursor;
	private ObiSolver solver;
	private int pinnedParticle = -1;

	/// <summary>
    /// Creates a straight rope anchored to a transform at the top.
    /// Transform may or may not move around and may or may not have a rigidbody.
    /// When you call this the rope will appear in the scene and immediately interact with gravity and objects with ObiColliders.
    /// Called from anywhere (main thread only)
    /// </summary>
	public IEnumerator MakeRope(Transform anchoredTo, Vector3 attachmentOffset, float ropeLength)
	{
		// create a new GameObject with the required components: a solver, a rope, and a curve. 
		// we also throw a cursor in to be able to change its length.
		GameObject ropeObject = new GameObject("rope",typeof(ObiSolver),
													  typeof(ObiRope),
													  typeof(ObiCatmullRomCurve), 
													  typeof (ObiRopeCursor));

		// get references to all components:
		rope 					= ropeObject.GetComponent<ObiRope>();
		cursor 					= ropeObject.GetComponent<ObiRopeCursor>();
		solver 					= ropeObject.GetComponent<ObiSolver>();
		ObiCatmullRomCurve path = ropeObject.GetComponent<ObiCatmullRomCurve>();

		// set up component references (see ObiRopeHelper.cs)
		rope.Solver = solver;
		rope.ropePath = path;	
		rope.section = Resources.Load<ObiRopeSection>("DefaultRopeSection");

		// set path control points (duplicate end points, to set curvature as required by CatmullRom splines):
		path.controlPoints.Clear();
		path.controlPoints.Add(Vector3.zero);
		path.controlPoints.Add(Vector3.zero);
		path.controlPoints.Add(Vector3.down*ropeLength);
		path.controlPoints.Add(Vector3.down*ropeLength);

		rope.pooledParticles = 2000;

		// parent the rope to the anchor transform:
		rope.transform.SetParent(anchoredTo,false);
		rope.transform.localPosition = attachmentOffset;

		// generate particles/constraints and add them to the solver (see ObiRopeHelper.cs)
		yield return rope.StartCoroutine(rope.GeneratePhysicRepresentationForMesh());
		rope.AddToSolver(null);

		// get the last particle in the rope at its rest state.
		pinnedParticle = rope.UsedParticles-1; 

		// add a tethers batch:
		ObiTetherConstraintBatch tetherBatch = new ObiTetherConstraintBatch(true,false,0,1);
		rope.TetherConstraints.AddBatch(tetherBatch);
		//UpdateTethers();

		// fix first particle in place (see http://obi.virtualmethodstudio.com/tutorials/scriptingparticles.html)
		rope.invMasses[0] = 0;
		Oni.SetParticleInverseMasses(solver.OniSolver,new float[]{0},1,rope.particleIndices[0]);
	}

	/// <summary>
    /// MakeRope and AddPendulum may NOT be called on the same frame. You must wait for the MakeRope coroutine to finish first, as creating a rope is an asynchronous operation.
    /// Just adds a pendulum to the rope on the un-anchored end.
    /// </summary>
	public void AddPendulum(ObiCollider pendulum, Vector3 attachmentOffset)
	{
		// simply add a new pin constraint (see http://obi.virtualmethodstudio.com/tutorials/scriptingconstraints.html)
		rope.PinConstraints.RemoveFromSolver(null);
		ObiPinConstraintBatch batch = (ObiPinConstraintBatch)rope.PinConstraints.GetFirstBatch();
		batch.AddConstraint(pinnedParticle, pendulum, attachmentOffset, 1);
		rope.PinConstraints.AddToSolver(null);
	}

	/// <summary>
    /// RemovePendulum and AddPendulum may be called on the same frame.
    /// </summary>
	public void RemovePendulum()
	{
		// simply remove all pin constraints (see http://obi.virtualmethodstudio.com/tutorials/scriptingconstraints.html)
		rope.PinConstraints.RemoveFromSolver(null);
		rope.PinConstraints.GetFirstBatch().Clear();
		rope.PinConstraints.AddToSolver(null);
	}

	/// <summary>
	/// Like extending or retracting a winch.
	/// </summary>
	public void ChangeRopeLength(float changeAmount)
	{
		// the cursor will automatically add/remove/modify constraints and particles as needed to obtain the new length.
		cursor.ChangeLength(rope.RestLength + changeAmount);
		//UpdateTethers();
	}

	private void UpdateTethers()
	{
		rope.TetherConstraints.RemoveFromSolver(null);
		ObiTetherConstraintBatch batch = (ObiTetherConstraintBatch)rope.TetherConstraints.GetFirstBatch();
		batch.Clear();

		ObiDistanceConstraintBatch dbatch = rope.DistanceConstraints.GetFirstBatch();
		for (int i = 0; i < dbatch.ConstraintCount; ++i)
			batch.AddConstraint(0,dbatch.springIndices[i*2+1], rope.InterparticleDistance*i, 1, 1);

		batch.Cook();
		rope.TetherConstraints.AddToSolver(null);
	}
}


