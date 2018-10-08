/**
\mainpage Obi documentation
 
Introduction:
------------- 

Obi is a position-based dynamics framework for unity. It enables the simulation of cloth, ropes and fluid in realtime, complete with two-way
rigidbody interaction.
 
Features:
-------------------

- Particles can be pinned both in local space and to rigidbodies (kinematic or not).
- Realistic wind forces.
- Rigidbodies react to particle dynamics, and particles reach to each other and to rigidbodies too.
- Easy prefab instantiation, particle-based actors can be translated, scaled and rotated.
- Simulation can be warm-started in the editor, then all simulation state gets serialized with the object. This means
  your prefabs can be stored at any point in the simulation, and they will resume it when instantiated.
- Custom editor tools.

*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.Linq;

namespace Obi
{

/**
 * An ObiSolver component simulates particles and their interactions using the Oni unified physics library.
 * Several kinds of constraint types and their parameters are exposed, and several Obi components can
 * be used to feed particles and constraints to the solver.
 */
[ExecuteInEditMode]
[AddComponentMenu("Physics/Obi/Obi Solver")]
[DisallowMultipleComponent]
public sealed class ObiSolver : MonoBehaviour
{

	public enum SimulationOrder{
		FixedUpdate,
		AfterFixedUpdate,
		LateUpdate
	}

	public class ObiCollisionEventArgs : System.EventArgs{
		public ObiList<Oni.Contact> contacts = new ObiList<Oni.Contact>();	/**< collision contacts.*/
	}

	public class ObiFluidEventArgs : System.EventArgs{

		public ObiList<int> indices = new ObiList<int>();			/**< fluid particle indices.*/
		public ObiList<Vector4> vorticities = new ObiList<Vector4>();
		public ObiList<float> densities = new ObiList<float>();

	}

	public class ParticleInActor{
		public ObiActor actor;
		public int indexInActor;

		public ParticleInActor(ObiActor actor, int indexInActor){
			this.actor = actor;
			this.indexInActor = indexInActor;
		}
	}

	public const int MAX_NEIGHBOURS = 92;
	public const int CONSTRAINT_GROUPS = 12;

	public static event System.EventHandler OnUpdateColliders;
	public static event System.EventHandler OnAfterUpdateColliders;
	public static event System.EventHandler OnUpdateRigidbodies;

	public event System.EventHandler OnFrameBegin;
	public event System.EventHandler OnStepBegin;
	public event System.EventHandler OnFixedParticlesUpdated;
	public event System.EventHandler OnStepEnd;
	public event System.EventHandler OnBeforePositionInterpolation;
	public event System.EventHandler OnBeforeActorsFrameEnd;
	public event System.EventHandler OnFrameEnd;
	public event System.EventHandler<ObiCollisionEventArgs> OnCollision;
	public event System.EventHandler<ObiFluidEventArgs> OnFluidUpdated;
	
	public int maxParticles = 5000;

	[HideInInspector] [NonSerialized] public bool simulate = true;

	public uint substeps = 1;

	[Tooltip("If enabled, will force the solver to keep simulating even when not visible from any camera.")]
	public bool simulateWhenInvisible = true; 			/**< Whether to keep simulating the cloth when its not visible by any camera.*/

	[Tooltip("If enabled, the solver object transform will be used as the frame of reference for all actors using this solver, instead of the world's frame.")]
	public bool simulateInLocalSpace = false;

	[Indent()]
	[VisibleIf("simulateInLocalSpace")]
	public float worldLinearVelocityScale = 0;			/**< how much does world-space linear velocity affect the actor. This only applies when the solver has "simulateInLocalSpace" enabled.*/

	[Indent()]
	[VisibleIf("simulateInLocalSpace")]
	public float worldAngularVelocityScale = 0;			/**< how much does world-space angular velocity affect the actor. This only applies when the solver has "simulateInLocalSpace" enabled.*/

	[Indent()]
	[VisibleIf("simulateInLocalSpace")]
	public float worldLinearInertiaScale = 0;			/**< how much does world-space linear inertia affect the actor. This only applies when the solver has "simulateInLocalSpace" enabled.*/

	[Indent()]
	[VisibleIf("simulateInLocalSpace")]
	public float worldAngularInertiaScale = 0;			/**< how much does world-space angular inertia affect the actor. This only applies when the solver has "simulateInLocalSpace" enabled.*/


	[Tooltip("Determines when will the solver update particles.")]
	[SerializeProperty("UpdateOrder")]
	[SerializeField] private SimulationOrder simulationOrder = SimulationOrder.FixedUpdate;

	[ChildrenOnly]
	public Oni.SolverParameters parameters = new Oni.SolverParameters(Oni.SolverParameters.Interpolation.None,
	                                                                  new Vector4(0,-9.81f,0,0));

	[HideInInspector] [NonSerialized] public List<ObiActor> actors = new List<ObiActor>();

	private int allocatedParticleCount = 0;
	[HideInInspector] [NonSerialized] public ParticleInActor[] particleToActor;
	[HideInInspector] [NonSerialized] public int[] materialIndices;
	[HideInInspector] [NonSerialized] public int[] fluidMaterialIndices;

	private int[] activeParticles;
	private List<ObiEmitterMaterial> emitterMaterials = new List<ObiEmitterMaterial>();

	[HideInInspector] [NonSerialized] public Vector4[] renderablePositions;	/**< renderable particle positions.*/
	[HideInInspector] [NonSerialized] public Oni.Anisotropy[] anisotropies;		/**< particle anisotropy matrices.*/
	
	// constraint parameters:
	[Header("Constraints")]
	public Oni.ConstraintParameters distanceConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Sequential,3);
	public Oni.ConstraintParameters bendingConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters particleCollisionConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters collisionConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters skinConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Sequential,3);
	public Oni.ConstraintParameters volumeConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters shapeMatchingConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters tetherConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters pinConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,3);
	public Oni.ConstraintParameters stitchConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,2);
	public Oni.ConstraintParameters densityConstraintParameters = new Oni.ConstraintParameters(true,Oni.ConstraintParameters.EvaluationOrder.Parallel,2);

	private IntPtr oniSolver;

	private ObiCollisionEventArgs collisionArgs = new ObiCollisionEventArgs();
	private ObiFluidEventArgs fluidArgs = new ObiFluidEventArgs();

	private ObiEmitterMaterial defaultFluidMaterial;
	private UnityEngine.Bounds bounds = new UnityEngine.Bounds();
 
 	private bool initialized = false;
	private bool isVisible = true;
	private float smoothDelta = 0.02f;
	private int renderablePositionsClients = 0;		/** counter for the amount of actors that need renderable positions.*/

	private static bool firstFixedUpdate = true;
	public static ObiArbiter fixedUpdateArbiter = new ObiArbiter();
	public static ObiArbiter afterFixedUpdateArbiter = new ObiArbiter();
	public static ObiArbiter lateUpdateArbiter = new ObiArbiter();

	public IntPtr OniSolver
	{
		get{return oniSolver;}
	}

	public UnityEngine.Bounds Bounds
	{
		get{return bounds;}
	}

	public bool IsVisible
	{
		get{return isVisible;}
	}

	public SimulationOrder UpdateOrder{
		set{
			if (simulationOrder != value){

				switch(simulationOrder){
					case SimulationOrder.FixedUpdate: fixedUpdateArbiter.UnregisterSolver(this); break;	
					case SimulationOrder.AfterFixedUpdate: afterFixedUpdateArbiter.UnregisterSolver(this); break;
					case SimulationOrder.LateUpdate: lateUpdateArbiter.UnregisterSolver(this); break;
				}
				
				simulationOrder = value;

				switch(simulationOrder){
					case SimulationOrder.FixedUpdate: fixedUpdateArbiter.RegisterSolver(this); break;	
					case SimulationOrder.AfterFixedUpdate: afterFixedUpdateArbiter.RegisterSolver(this); break;
					case SimulationOrder.LateUpdate: lateUpdateArbiter.RegisterSolver(this); break;
				}
			}
		}
		get{return simulationOrder;}
	}

	public int AllocParticleCount{
		get{return allocatedParticleCount;}
	}

	public bool IsUpdating{
		get{return (initialized && simulate && (simulateWhenInvisible || IsVisible));}
	}

	public void RequireRenderablePositions(){
		renderablePositionsClients++;
	}

	public void RelinquishRenderablePositions(){
		if (renderablePositionsClients > 0)
			renderablePositionsClients--;
	}

	void OnValidate(){
		if (substeps < 1) substeps = 1;
	}

	void Awake(){

		if (Application.isPlaying) //only during game.
			Initialize();
	}

	void OnDestroy(){
		if (Application.isPlaying) //only during game.
			Teardown();
	}

	void OnEnable(){
		if (!Application.isPlaying) //only in editor.
			Initialize();

		StartCoroutine("RunLateFixedUpdate");

		switch(simulationOrder){
			case SimulationOrder.FixedUpdate: fixedUpdateArbiter.RegisterSolver(this); break;	
			case SimulationOrder.AfterFixedUpdate: afterFixedUpdateArbiter.RegisterSolver(this); break;
			case SimulationOrder.LateUpdate: lateUpdateArbiter.RegisterSolver(this); break;
		}		
	}
	
	void OnDisable(){
		if (!Application.isPlaying) //only in editor.
			Teardown();
		StopCoroutine("RunLateFixedUpdate");
		switch(simulationOrder){
			case SimulationOrder.FixedUpdate: fixedUpdateArbiter.UnregisterSolver(this); break;	
			case SimulationOrder.AfterFixedUpdate: afterFixedUpdateArbiter.UnregisterSolver(this); break;
			case SimulationOrder.LateUpdate: lateUpdateArbiter.UnregisterSolver(this); break;
		}
	}
	
	public void Initialize(){

		// Tear everything down first:
		Teardown();
			
		try{

			// Create a default material:
			defaultFluidMaterial = ScriptableObject.CreateInstance<ObiEmitterMaterialFluid>();
			defaultFluidMaterial.hideFlags = HideFlags.HideAndDontSave;
	
			// Create the Oni solver:
			oniSolver = Oni.CreateSolver(maxParticles,MAX_NEIGHBOURS);

			// Initialize moving transform:
			InitializeTransformFrame();
			
			actors = new List<ObiActor>();
			activeParticles = new int[maxParticles];
			particleToActor = new ParticleInActor[maxParticles];
			materialIndices = new int[maxParticles];
			fluidMaterialIndices = new int[maxParticles];
			renderablePositions = new Vector4[maxParticles];
			anisotropies = new Oni.Anisotropy[maxParticles];
			
			// Initialize materials:
			UpdateEmitterMaterials();
			
			// Initialize parameters:
			UpdateParameters();
			
		}catch (Exception exception){
			Debug.LogException(exception);
		}finally{
			initialized = true;
		};

	}

	private void Teardown(){
	
		if (!initialized) return;
		
		try{

			while (actors.Count > 0){
				actors[actors.Count-1].RemoveFromSolver(null);
			}
				
			Oni.DestroySolver(oniSolver);
			oniSolver = IntPtr.Zero;

			GameObject.DestroyImmediate(defaultFluidMaterial);
		
		}catch (Exception exception){
			Debug.LogException(exception);
		}finally{
			initialized = false;
		}
	}

	private void InitializeTransformFrame(){
		Vector4 translation = transform.position;
		Vector4 scale = transform.lossyScale;
		Quaternion rotation = transform.rotation;
		Oni.InitializeFrame(this.oniSolver,ref translation,ref scale, ref rotation);
	}

	private void UpdateTransformFrame(float dt){
		if (simulateInLocalSpace){
			Vector4 translation = transform.position;
			Vector4 scale = transform.lossyScale;
			Quaternion rotation = transform.rotation;
			Oni.UpdateFrame(this.oniSolver,ref translation,ref scale, ref rotation,dt);
			Oni.ApplyFrame(this.oniSolver,worldLinearVelocityScale,worldAngularVelocityScale,worldLinearInertiaScale,worldAngularInertiaScale,dt);
		}else{
			Vector4 translation = Vector4.zero;
			Vector4 scale = Vector4.one;
			Quaternion rotation = Quaternion.identity;
			Oni.InitializeFrame(this.oniSolver,ref translation,ref scale, ref rotation);
		}
	}

	/**
	 * Adds the actor to this solver. Will return whether the allocation was sucessful or not.
	 */
	public bool AddActor(ObiActor actor, int numParticles){

		if (particleToActor == null || actor == null)
			return false;

		int[] allocated = new int[numParticles];
		int allocatedCount = 0;

		for (int i = 0; i < maxParticles && allocatedCount < numParticles; i++){
			if (particleToActor[i] == null){
				allocated[allocatedCount] = i;
				allocatedCount++;
			}
		}

		// could not allocate enough particles.
		if (allocatedCount < numParticles){
			return false; 
		}

		allocatedParticleCount += numParticles;

		// store per-particle actor reference:
		for (int i = 0; i < numParticles; ++i)
			particleToActor[allocated[i]] = new ParticleInActor(actor,i);

		// set the actor particle indices.
		actor.particleIndices = allocated;

        // Add the actor to the actor list:
		actors.Add(actor);

		// Update active particles. Update materials, in case the actor has a new one.
		UpdateActiveParticles();  
		UpdateEmitterMaterials();
       
		return true;

	}

	/**
	 * Removes an actor from this solver. Returns the index that was occupied by the actor in the actor list, or -1 if it was not managed by this solver.
	 */
	public int RemoveActor(ObiActor actor){
		
		if (particleToActor == null || actor == null)
			return -1;

		// Find actor index in our actors array:
		int index = actors.IndexOf(actor);

		// If we are in charce of this actor indeed, perform all steps necessary to release it.
		if (index > -1){

			allocatedParticleCount -= actor.particleIndices.Length;

			for (int i = 0; i < actor.particleIndices.Length; ++i)
				particleToActor[actor.particleIndices[i]] = null;
	
			actors.RemoveAt(index); 
	
			// Update active particles. Update materials, in case the actor had one.
			UpdateActiveParticles(); 
			UpdateEmitterMaterials();

		}
		
		return index;
	}

	/**
	 * Updates solver parameters, sending them to the Oni library.
	 */
	public void UpdateParameters(){

		Oni.SetSolverParameters(oniSolver,ref parameters);

		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Distance,ref distanceConstraintParameters);
		
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Bending,ref bendingConstraintParameters);
	
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.ParticleCollision,ref particleCollisionConstraintParameters);

		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Collision,ref collisionConstraintParameters);

		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Density,ref densityConstraintParameters);
		
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Skin,ref skinConstraintParameters);
		
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Volume,ref volumeConstraintParameters);

		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.ShapeMatching,ref shapeMatchingConstraintParameters);
		
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Tether,ref tetherConstraintParameters);
	
		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Pin,ref pinConstraintParameters);

		Oni.SetConstraintGroupParameters(oniSolver,(int)Oni.ConstraintType.Stitch,ref stitchConstraintParameters);

    }

	/**
	 * Updates the active particles array.
	 */
	public void UpdateActiveParticles(){

		int numActive = 0;

		for (int i = 0; i < actors.Count; ++i){

			ObiActor currentActor = actors[i];

			if (currentActor.isActiveAndEnabled){
				for (int j = 0; j < currentActor.particleIndices.Length; ++j){
					if (currentActor.active[j]){
						activeParticles[numActive] = currentActor.particleIndices[j];
						numActive++;
					}
				}
			}
		}	

		Oni.SetActiveParticles(oniSolver,activeParticles,numActive);

	}

	public void UpdateEmitterMaterials(){

		// reset the emitter material list:
		emitterMaterials = new List<ObiEmitterMaterial>(){defaultFluidMaterial};

		// Setup all materials used by particle actors:
		foreach (ObiActor actor in actors){
			
			ObiEmitter em = actor as ObiEmitter;
				if (em == null) continue;

			int materialIndex = 0;

			if (em.EmitterMaterial != null){

				materialIndex = emitterMaterials.IndexOf(em.EmitterMaterial);

				// if the material has not been considered before:
				if (materialIndex < 0){

					materialIndex = emitterMaterials.Count;
					emitterMaterials.Add(em.EmitterMaterial);
			
					//keep an eye on material changes:
					em.EmitterMaterial.OnChangesMade += emitterMaterial_OnChangesMade;
				}
			}
			
			// Update material index for all actor particles:
			for(int i = 0; i < actor.particleIndices.Length; i++){
				fluidMaterialIndices[actor.particleIndices[i]] = materialIndex;
			}
		}

		Oni.SetFluidMaterialIndices(oniSolver,fluidMaterialIndices,fluidMaterialIndices.Length,0);
		Oni.FluidMaterial[] mArray = emitterMaterials.ConvertAll<Oni.FluidMaterial>(a => a.GetEquivalentOniMaterial(parameters.mode)).ToArray();
		Oni.SetFluidMaterials(oniSolver,mArray,mArray.Length,0);
	}

	private void emitterMaterial_OnChangesMade (object sender, ObiEmitterMaterial.MaterialChangeEventArgs e)
	{
		ObiEmitterMaterial material = sender as ObiEmitterMaterial; 
		int index = emitterMaterials.IndexOf(material);
		if (index >= 0){
			Oni.SetFluidMaterials(oniSolver,new Oni.FluidMaterial[]{material.GetEquivalentOniMaterial(parameters.mode)},1,index);
		}
	}

	public void AccumulateSimulationTime(float dt){
		Oni.AddSimulationTime(oniSolver,dt);
	}

	public void ResetSimulationTime(){
		Oni.ResetSimulationTime(oniSolver);
	}

	public void SimulateStep(float stepTime){

		UpdateTransformFrame(stepTime);

		if (IsUpdating){

			Oni.ClearDiffuseParticles(oniSolver);
	
			if (OnStepBegin != null)
				OnStepBegin(this,null);
	
			foreach(ObiActor actor in actors)
	            actor.OnSolverStepBegin();
	
			// Update Oni skeletal mesh skinning after updating animators:
			Oni.UpdateSkeletalAnimation(oniSolver);
	
			// Trigger event right after actors have fixed their particles in OnSolverStepBegin.
			if (OnFixedParticlesUpdated != null)
				OnFixedParticlesUpdated(this,null);
	
			// Update the solver (this is internally split in tasks so multiple solvers can be updated in parallel)
			Oni.UpdateSolver(oniSolver, substeps, stepTime/(float)substeps); 
	
			// Wait here for all other solvers to finish:
		 	WaitForAllSolvers();

		}
	} 

	public void EndFrame(float stepTime){

		foreach(ObiActor actor in actors)
            actor.OnSolverPreInterpolation();

		if (OnBeforePositionInterpolation != null)
			OnBeforePositionInterpolation(this,null);

		Oni.ApplyPositionInterpolation(oniSolver, stepTime);

		// if we need to get renderable positions back from the solver:
		if (renderablePositionsClients > 0)
		{
			// get renderable particle anisotropies:
			Oni.GetParticleAnisotropies(oniSolver,anisotropies,anisotropies.Length,0);

			// get renderable particle positions:
			Oni.GetRenderableParticlePositions(oniSolver, renderablePositions, renderablePositions.Length,0);
		}

		// Trigger fluid update:
		TriggerFluidUpdateEvents();

		if (OnBeforeActorsFrameEnd != null)
			OnBeforeActorsFrameEnd(this,null);

		UpdateVisibility();
		
		foreach(ObiActor actor in actors)
            actor.OnSolverFrameEnd();

	}

	private void TriggerFluidUpdateEvents(){

		int numFluidParticles = Oni.GetConstraintCount(oniSolver,(int)Oni.ConstraintType.Density);

		if (OnFluidUpdated != null){

			fluidArgs.indices.SetCount(numFluidParticles);
			fluidArgs.vorticities.SetCount(maxParticles);
			fluidArgs.densities.SetCount(maxParticles);

			if (numFluidParticles > 0){
				Oni.GetActiveConstraintIndices(oniSolver,fluidArgs.indices.Data,numFluidParticles,(int)Oni.ConstraintType.Density);
				Oni.GetParticleVorticities(oniSolver,fluidArgs.vorticities.Data,maxParticles,0);
				Oni.GetParticleDensities(oniSolver,fluidArgs.densities.Data,maxParticles,0);
			}

			OnFluidUpdated(this,fluidArgs);
		}
	}

	private void TriggerCollisionEvents(){
	
		int numCollisions = Oni.GetConstraintCount(oniSolver,(int)Oni.ConstraintType.Collision);

		if (OnCollision != null){

			collisionArgs.contacts.SetCount(numCollisions);

			if (numCollisions > 0)
				Oni.GetCollisionContacts(oniSolver,collisionArgs.contacts.Data,numCollisions);

			OnCollision(this,collisionArgs);

		}
	}

	private bool AreBoundsValid(Bounds bounds){
		return !(float.IsNaN(bounds.center.x) || float.IsInfinity(bounds.center.x) ||
			     float.IsNaN(bounds.center.y) || float.IsInfinity(bounds.center.y) ||
			     float.IsNaN(bounds.center.z) || float.IsInfinity(bounds.center.z));
	}

	public void UpdateColliders(){
		if (OnUpdateColliders != null)
			OnUpdateColliders(this,null);
		if (OnAfterUpdateColliders != null)
			OnAfterUpdateColliders(this,null);
	}

	public void UpdateRigidbodies(){
		if (OnUpdateRigidbodies != null)
			OnUpdateRigidbodies(this,null);
	}

	public void WaitForAllSolvers(){

		switch(simulationOrder){

			case SimulationOrder.FixedUpdate: 
				if (fixedUpdateArbiter.WaitForAllSolvers()) 
					UpdateRigidbodies();
			break;	

			case SimulationOrder.AfterFixedUpdate: 
				afterFixedUpdateArbiter.WaitForAllSolvers(); 
			break;

			case SimulationOrder.LateUpdate: 
				lateUpdateArbiter.WaitForAllSolvers(); 
			break;

		}
	}

	/**
	 * Checks if any particle in the solver is visible from at least one camera. If so, sets isVisible to true, false otherwise.
	 */
	public void UpdateVisibility(){

		Vector3 min = Vector3.zero, max = Vector3.zero;
		Oni.GetBounds(oniSolver,ref min, ref max);
		bounds.SetMinMax(min,max);

		if (AreBoundsValid(bounds)){

			foreach (Camera cam in Camera.allCameras){
	        	Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
	       		if (GeometryUtility.TestPlanesAABB(planes, bounds)){
					if (!isVisible){
						isVisible = true;
						foreach(ObiActor actor in actors)
							actor.OnSolverVisibilityChanged(isVisible);
					}
					return;
				}
			}
		}

		if (isVisible){
			isVisible = false;
			foreach(ObiActor actor in actors)
				actor.OnSolverVisibilityChanged(isVisible);
		}
	}
    
    void Update(){

		if (!Application.isPlaying)
			return;

		if (OnFrameBegin != null)
			OnFrameBegin(this,null);

		foreach(ObiActor actor in actors)
            actor.OnSolverFrameBegin();

		if (IsUpdating && simulationOrder != SimulationOrder.LateUpdate){
			AccumulateSimulationTime(Time.deltaTime);
		}

	}

	IEnumerator RunLateFixedUpdate() {
         while (true) {

             yield return new WaitForFixedUpdate();
		
			 firstFixedUpdate = true;

			 if (Application.isPlaying && simulationOrder == SimulationOrder.AfterFixedUpdate)
             	SimulateStep(Time.fixedDeltaTime); 
         }
     }

    void FixedUpdate()
    {
		if (Application.isPlaying){

			// Update colliders before this frame's first FixedUpdate.
			if (firstFixedUpdate){
				firstFixedUpdate = false;

				UpdateColliders();

				if (ObiProfiler.Instance != null)
					ObiProfiler.Instance.StartStep();
			}

			if (simulationOrder == SimulationOrder.FixedUpdate)
				SimulateStep(Time.fixedDeltaTime);
			
		}
    }

	public void AllSolversStepEnd()
	{
		// Trigger solver events:
		TriggerCollisionEvents();
	
		foreach(ObiActor actor in actors)
       	 	actor.OnSolverStepEnd();

		if (OnStepEnd != null)
			OnStepEnd(this,null);

	}

	private void LateUpdate(){

		float lateUpdateDelta = 0;

		if (Application.isPlaying && simulationOrder == SimulationOrder.LateUpdate){

			 if (Time.deltaTime > 0){

			 	// smooth out timestep and accumulate it:
			 	lateUpdateDelta = smoothDelta = Mathf.Lerp(Time.deltaTime,smoothDelta,0.95f);

			 	AccumulateSimulationTime(lateUpdateDelta);
             	SimulateStep(lateUpdateDelta);

			 }else{
				lateUpdateDelta = 0;
			 }

		}

		if (!Application.isPlaying)
			return;
   
		EndFrame (simulationOrder == SimulationOrder.LateUpdate ? lateUpdateDelta : Time.fixedDeltaTime);

		if (OnFrameEnd != null)
			OnFrameEnd(this,null);
	}

}

}
