using UnityEngine;
using System;

namespace Obi
{
	[RequireComponent(typeof(Animator))]
	[DisallowMultipleComponent]
	public class ObiAnimatorController : MonoBehaviour
	{
		bool updatedThisStep = false;
		Animator animator;

		public void Awake(){
			animator = GetComponent<Animator>();
		}

		public void UpdateAnimation()
		{
			if (animator != null && animator.enabled && !updatedThisStep){
				animator.playableGraph.Stop();
				animator.Update(Time.fixedDeltaTime);
				updatedThisStep = true;
			}
		}

		public void ResetUpdateFlag(){
			updatedThisStep = false;
		}

		public void ResumeAutonomousUpdate(){
			if (animator != null && animator.enabled){
				animator.playableGraph.Play();
			}
		}
	}
}

