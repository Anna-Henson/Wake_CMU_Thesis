using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Obi{
	
	/**
 	 * Holds information about shape matching constraints for an actor.
 	 */
	[DisallowMultipleComponent]
	public class ObiShapeMatchingConstraints : ObiBatchedConstraints 
	{
		
		[Range(0,1)]
		[Tooltip("Stiffness of the volume constraints. Higher values will make the constraints to try harder to enforce the set volume.")]
		public float stiffness = 1;
		
		[SerializeField][HideInInspector] private List<ObiShapeMatchingConstraintBatch> batches = new List<ObiShapeMatchingConstraintBatch>();

		public override Oni.ConstraintType GetConstraintType(){
			return Oni.ConstraintType.ShapeMatching;
		}
	
		public override IEnumerable<ObiConstraintBatch> GetBatches(){
			return batches.Cast<ObiConstraintBatch>();
		}

		public ObiShapeMatchingConstraintBatch GetFirstBatch(){
			return batches.Count > 0 ? batches[0] : null;
		}
	
		public override void Clear(){
			RemoveFromSolver(null); 
			batches.Clear();
		}
	
		public void AddBatch(ObiShapeMatchingConstraintBatch batch){
			if (batch != null && batch.GetConstraintType() == GetConstraintType())
				batches.Add(batch);
		}
	
		public void RemoveBatch(ObiShapeMatchingConstraintBatch batch){
			batches.Remove(batch);
		}

		public void OnDrawGizmosSelected(){
		
			if (!visualize) return;
	
			Gizmos.color = Color.black;
	
			foreach (ObiShapeMatchingConstraintBatch batch in batches){
				foreach(int i in batch.ActiveConstraints){
					int first = batch.firstIndex[i];
				
					Vector3 p1 = actor.GetParticlePosition(batch.shapeIndices[first]);
					for(int j = 1; j < batch.numIndices[i]; ++j){

						int index = first + j;
						Vector3 p2 = actor.GetParticlePosition(batch.shapeIndices[index]);
	
						Gizmos.DrawLine(p1,p2);
					}
				}
			}
		
		}
		
	}
}





