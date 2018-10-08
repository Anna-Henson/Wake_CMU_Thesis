using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[Serializable]
	public class ObiRopeChainRenderMode : ObiRopeRenderMode
	{

		[HideInInspector] public List<GameObject> linkInstances;

		[SerializeProperty("RandomizeLinks")]
		[SerializeField] private bool randomizeLinks = false;

		public Vector3 linkScale = Vector3.one;		/**< Scale of chain links.*/
		public List<GameObject> chainLinks = new List<GameObject>();

		private ObiRope.CurveFrame frame;

		public bool RandomizeLinks{
			get{return randomizeLinks;}
			set{
				if (value != randomizeLinks){
					randomizeLinks = value;
					Initialize();
				}
			}
		}

		/**
		 * Destroys all chain link instances. Used when the chain must be re-created from scratch, and when the actor is disabled/destroyed.
		 */
		private void ClearChainLinkInstances(){

			if (linkInstances == null)
				return;

			for (int i = 0; i < linkInstances.Count; ++i){
				if (linkInstances[i] != null)
					GameObject.DestroyImmediate(linkInstances[i]);
			}
			linkInstances.Clear();
		}

		public override void Initialize(){

			if (rope.ropeMesh != null)
				rope.ropeMesh.Clear();

			ClearChainLinkInstances();
			
			if (chainLinks.Count > 0){

				for (int i = 0; i < rope.TotalParticles; ++i){
	
					int index = randomizeLinks ? UnityEngine.Random.Range(0,chainLinks.Count) : i % chainLinks.Count;
	
					GameObject linkInstance = null;

					if (chainLinks[index] != null){
						linkInstance = GameObject.Instantiate(chainLinks[index]);
						linkInstance.transform.SetParent(rope.transform,false);
						linkInstance.hideFlags = HideFlags.HideAndDontSave;
						linkInstance.SetActive(false);
					}
	
					linkInstances.Add(linkInstance);
				}
			}

			Update(null);
		}
		
		public override void TearDown(){
			ClearChainLinkInstances();
		}

		public override void Update(Camera camera){

			if (linkInstances.Count == 0)
				return;

			ObiDistanceConstraintBatch distanceBatch = rope.DistanceConstraints.GetFirstBatch();

			// we will define and transport a reference frame along the curve using parallel transport method:
			if (frame == null) 			
				frame = new ObiRope.CurveFrame();
			frame.Reset();
			frame.SetTwist(-rope.sectionTwist * distanceBatch.ConstraintCount * rope.uvAnchor);

			int lastParticle = -1;
			int tearCount = 0;

			for (int i = 0; i < distanceBatch.ConstraintCount; ++i){

				int particle1 = distanceBatch.springIndices[i*2];
				int particle2 = distanceBatch.springIndices[i*2+1];

				Vector3 pos = rope.GetParticlePosition(particle1);
				Vector3 nextPos = rope.GetParticlePosition(particle2);
				Vector3 linkVector = nextPos-pos;
				Vector3 tangent = linkVector.normalized;

				if (i > 0 && particle1 != lastParticle){

					// update tear prefab at the first side of tear:
					if (rope.tearPrefabPool != null && tearCount < rope.tearPrefabPool.Length){
						
						if (!rope.tearPrefabPool[tearCount].activeSelf)
							 rope.tearPrefabPool[tearCount].SetActive(true);
					
						rope.PlaceObjectAtCurveFrame(frame,rope.tearPrefabPool[tearCount],Space.World, true);
						tearCount++;
					}

					// reset frame at discontinuities:
					frame.Reset();
				}

				// update frame:
				frame.Transport(nextPos,tangent,rope.sectionTwist);

				// update tear prefab at the other side of the tear:
				if (i > 0 && particle1 != lastParticle && rope.tearPrefabPool != null && tearCount < rope.tearPrefabPool.Length){
					
					if (!rope.tearPrefabPool[tearCount].activeSelf)
						 rope.tearPrefabPool[tearCount].SetActive(true);

					frame.position = pos;
					rope.PlaceObjectAtCurveFrame(frame,rope.tearPrefabPool[tearCount],Space.World, false);
					tearCount++;
				}

				// update start/end prefabs:
				if (!rope.Closed){
					if (i == 0 && rope.startPrefabInstance != null)
						rope.PlaceObjectAtCurveFrame(frame,rope.startPrefabInstance,Space.World,false);
					else if (i == distanceBatch.ConstraintCount-1 && rope.endPrefabInstance != null){
						frame.position = nextPos;
						rope.PlaceObjectAtCurveFrame(frame,rope.endPrefabInstance,Space.World,true);
					}
				}

				if (linkInstances[i] != null){
					linkInstances[i].SetActive(true);
					Transform linkTransform = linkInstances[i].transform;
					linkTransform.position = pos + linkVector * 0.5f;
					linkTransform.localScale = rope.thicknessFromParticles ? (rope.solidRadii[particle1]/rope.thickness) * linkScale : linkScale;
					linkTransform.rotation = Quaternion.LookRotation(tangent,frame.normal);
				}

				lastParticle = particle2;

			}		

			for (int i = distanceBatch.ConstraintCount; i < linkInstances.Count; ++i){
				if (linkInstances[i] != null)
					linkInstances[i].SetActive(false);
			}
		}
	}
}
