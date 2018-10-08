using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi{

/**
 * Holds data about shape matching constraints for an actor.
 */
[Serializable]
public class ObiShapeMatchingConstraintBatch : ObiConstraintBatch
{

	[HideInInspector] public List<int> shapeIndices = new List<int>();				/**< particle indices.*/
	[HideInInspector] public List<int> firstIndex = new List<int>();				/**< index of first triangle for each constraint.*/
	[HideInInspector] public List<int> numIndices = new List<int>();				/**< num of triangles for each constraint.*/

	[HideInInspector] public List<float> shapeStiffness = new List<float>();		/**< pressure and stiffness for each constraint.*/

	int[] solverIndices;

	public ObiShapeMatchingConstraintBatch(bool cooked, bool sharesParticles) : base(cooked,sharesParticles){
	}

	public override Oni.ConstraintType GetConstraintType(){
		return Oni.ConstraintType.ShapeMatching;
	}

	public override void Clear(){
		activeConstraints.Clear();
		shapeIndices.Clear();
		firstIndex.Clear();
		numIndices.Clear();
		shapeStiffness.Clear();
		constraintCount = 0;	
	}

	public void AddConstraint(int[] particleIndices,  float stiffness){

		activeConstraints.Add(constraintCount);

		firstIndex.Add((int)shapeIndices.Count);
		numIndices.Add((int)particleIndices.Length);
		shapeIndices.AddRange(particleIndices);

		shapeStiffness.Add(stiffness);

		constraintCount++;

	}

	public void RemoveConstraint(int index){

		if (index < 0 || index >= ConstraintCount)
			return;

		activeConstraints.Remove(index);
		for(int i = 0; i < activeConstraints.Count; ++i)
		    if (activeConstraints[i] > index) activeConstraints[i]--;

		shapeIndices.RemoveRange(firstIndex[index],numIndices[index]);
		firstIndex.RemoveAt(index);
	    numIndices.RemoveAt(index);
		shapeStiffness.RemoveAt(index);
		constraintCount--;
	}

	public override List<int> GetConstraintsInvolvingParticle(int particleIndex){
	
		List<int> constraints = new List<int>(4);
		
		for (int i = 0; i < ConstraintCount; i++){
			for (int j = 0; j < numIndices[i]; j++){
				if (shapeIndices[firstIndex[i] + j] == particleIndex){ 
					constraints.Add(i);
				}
			}
		}
		
		return constraints;
	}

	protected override void OnAddToSolver(ObiBatchedConstraints constraints){

		// Set solver constraint data:
		solverIndices = new int[shapeIndices.Count];
		for (int i = 0; i < shapeIndices.Count; i++)
		{
			solverIndices[i] = constraints.Actor.particleIndices[shapeIndices[i]];
		}
	}

	protected override void OnRemoveFromSolver(ObiBatchedConstraints constraints){
	}

	public override void PushDataToSolver(ObiBatchedConstraints constraints){ 

		if (constraints == null || constraints.Actor == null || !constraints.Actor.InSolver)
			return;

		//ObiShapeMatchingConstraints sc = (ObiShapeMatchingConstraints) constraints;

		for (int i = 0; i < shapeStiffness.Count; i++){
			shapeStiffness[i] = 1;//StiffnessToCompliance(sc.stiffness);
		}

		Oni.SetShapeMatchingConstraints(batch,
										solverIndices,
									    firstIndex.ToArray(),
								        numIndices.ToArray(),
									    shapeStiffness.ToArray(),
									    ConstraintCount);

		Oni.CalculateRestShapeMatching(constraints.Actor.Solver.OniSolver,batch);
	}	

	public override void PullDataFromSolver(ObiBatchedConstraints constraints){
	}	

}
}
