using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Obi{

/**
 * Represents a group of related particles. ObiActor does not make
 * any assumptions about the relationship between these particles, except that they get allocated 
 * and released together.
 */
[ExecuteInEditMode]
[DisallowMultipleComponent]
public abstract class ObiActor : MonoBehaviour, IObiSolverClient
{
	public class ObiActorSolverArgs : System.EventArgs{

        private ObiSolver solver;
		public ObiSolver Solver{
            get{return solver;}
        }

		public ObiActorSolverArgs(ObiSolver solver){
			this.solver = solver;
		}
	}

	public enum TetherType{
		AnchorToFixed,
		Hierarchical
	}

	public event System.EventHandler<ObiActorSolverArgs> OnAddedToSolver;
	public event System.EventHandler<ObiActorSolverArgs> OnRemovedFromSolver;
	public event System.EventHandler OnInitialized;

	[SerializeProperty("Solver")]
	[SerializeField] protected ObiSolver solver;

	[SerializeProperty("CollisionMaterial")]
	[SerializeField] protected ObiCollisionMaterial collisionMaterial;

	[SerializeProperty("SelfCollisions")]
	[SerializeField] protected bool selfCollisions = false;

	[HideInInspector][NonSerialized] public int[] particleIndices;					/**< indices of allocated particles in the solver.*/

	protected ObiBatchedConstraints[] constraints;	/**< list of constraint components used by this actor.*/
	
	[HideInInspector] public bool[] active;					/**< Particle activation status.*/
	[HideInInspector] public Vector3[] positions;			/**< Particle positions.*/
	[HideInInspector] public Vector4[] restPositions;		/**< Particle rest positions, used to filter collisions.*/		
	[HideInInspector] public Quaternion[] orientations;		/**< Particle orientations.*/
	[HideInInspector] public Vector3[] velocities;			/**< Particle velocities.*/
	[HideInInspector] public Vector3[] angularVelocities;	/**< Particle angular velocities.*/
	[HideInInspector] public float[] invMasses;				/**< Particle inverse masses*/
	[HideInInspector] public float[] solidRadii;			/**< Particle solid radii (physical radius of each particle)*/
	[HideInInspector] public Vector2[] principalRadii;		/**< Particle ellipsoid principal radii (must be smaller than solidRadii)*/
	[HideInInspector] public int[] phases;					/**< Particle phases.*/
	[HideInInspector] public Color[] colors = null;				/**< Particle colors (not used by all actors, can be null)*/

	[HideInInspector] public int[] deformableTriangles = new int[0];	/**< Indices of deformable triangles (3 per triangle)*/
	[NonSerialized] protected int trianglesOffset = 0;					/**< Offset of deformable trtiangles in curent solver*/

	[SerializeField][HideInInspector] protected Matrix4x4 initialScaleMatrix = Matrix4x4.identity; /**< World scale of the actor, at the moment of initialization.*/

	private bool inSolver = false;
	protected bool initializing = false;	
	
	[HideInInspector][SerializeField] protected bool initialized = false;

	public ObiSolver Solver{
		get{return solver;}
		set{
			if (solver != value){
				RemoveFromSolver(null);
				solver = value;
			}
		}
	}

	public ObiCollisionMaterial CollisionMaterial{
		get{return collisionMaterial;}
		set{
			if (collisionMaterial != value){
				collisionMaterial = value;
				PushDataToSolver(ParticleData.COLLISION_MATERIAL);
			}
		}
	}
	
	public bool Initializing{
		get{return initializing;}
	}
	
	public bool Initialized{
		get{return initialized;}
	}

	public bool InSolver{
		get{return inSolver;}
	}

	public bool SelfCollisions{
		get{return selfCollisions;}
		set{
			if (value != selfCollisions){
				selfCollisions = value;
				UpdateParticlePhases();
			}
		}
	}

	public Matrix4x4 InitialScaleMatrix{
		get{
			return initialScaleMatrix;
		}
	}

	public virtual Matrix4x4 ActorLocalToWorldMatrix{
		get{
			return transform.localToWorldMatrix;
		}
	}

	public virtual Matrix4x4 ActorWorldToLocalMatrix{
		get{
			return transform.worldToLocalMatrix;
		}
	}

	/**
	 * If true, it means external forces aren't applied to the particles directly. For instance,
	 * cloth uses aerodynamic constraints to do so, and fluid uses drag.
	 */
	public virtual bool UsesCustomExternalForces{ 
		get{return false;}
	}

	public virtual void Awake(){
    }

	/**
	 * Since Awake is not guaranteed to be called before OnEnable, we must add the mesh to the solver here.
	 */
	public virtual void Start(){
		if (Application.isPlaying)
			AddToSolver(null);
	}

	public virtual void OnDestroy(){
		RemoveFromSolver(null);
	}

	public virtual void DestroyRequiredComponents(){
		#if UNITY_EDITOR
			foreach (ObiBatchedConstraints c in constraints)
				GameObject.DestroyImmediate(c);
		#endif
	}

	/**
	 * Flags all particles allocated by this actor as active or inactive depending on the "active array".
	 * The solver will then only simulate the active ones.
	 */
	public virtual void OnEnable(){

		ObiBatchedConstraints[] constraintComponents = GetComponents<ObiBatchedConstraints>();
		constraints = new ObiBatchedConstraints[Oni.ConstraintTypeCount];

		// build constraints list:
		foreach(ObiBatchedConstraints c in constraintComponents){

			constraints[(int)c.GetConstraintType()] = c;

			c.GrabActor();
			if (c.isActiveAndEnabled)
				c.OnEnable();
		}

		if (!InSolver) return;

		// update active status of all particles in the actor:
		solver.UpdateActiveParticles();

		// maybe this actor makes the solver visible to a camera now:
		solver.UpdateVisibility();
	}

	/**
	 * Flags all particles allocated by this actor as inactive, so the solver will not include them 
	 * in the simulation. To "teleport" the actor to a new position, disable it and then pull positions
	 * and velocities from the solver. Move it to the new position, and enable it.
	 */
	public virtual void OnDisable(){

		if (!InSolver) return;

		// flag all the actor's particles as disabled:
		solver.UpdateActiveParticles();

		// pull current position / velocity data from solver:
		PullDataFromSolver(ParticleData.POSITIONS | ParticleData.VELOCITIES);

		// disable constraints:
		foreach (ObiBatchedConstraints c in constraints)
			if (c != null)
				c.OnDisable();

		// maybe this actor makes the solver invisible to all cameras now:
		solver.UpdateVisibility();
	}

	/**
	 * Generates the particle based physical representation of the actor. This is the initialization method for the actor
	 */
	public IEnumerator GeneratePhysicRepresentationForMesh(){

		IEnumerator g = Initialize();

		while (g.MoveNext()) 
			yield return g.Current;

		if (OnInitialized != null)
			OnInitialized(this,null);
	}

	protected abstract IEnumerator Initialize();

	/**
	 * Resets the actor to its original state.
	 */
	public virtual void ResetActor(){
	}

	/**
	 * Updates particle phases in the solver.
	 */
	public virtual void UpdateParticlePhases(){

		if (!InSolver) return;

		for(int i = 0; i < phases.Length; i++){
			phases[i] = Oni.MakePhase(Oni.GetGroupFromPhase(phases[i]),selfCollisions?Oni.ParticlePhase.SelfCollide:0);
		}
		PushDataToSolver(ParticleData.PHASES);
	}

	/**
	 * Adds this actor to a solver. No simulation will take place for this actor
 	 * unless it has been added to a solver. Returns true if the actor was succesfully added,
 	 * false if it was already added or couldn't add it for any other reason.
	 */
	public virtual bool AddToSolver(object info){

		if (solver != null && !InSolver){
			
			// Allocate particles in the solver:
			if (!solver.AddActor(this,positions.Length)){
				Debug.LogWarning("Obi: Solver could not allocate enough particles for this actor. Please increase max particles.");
				return false;
			}

			inSolver = true;

			// Update particle phases before sending data to the solver, as layers/flags settings might have changed.
			UpdateParticlePhases();
			
			// find our offset in the deformable triangles array.
			trianglesOffset = Oni.GetDeformableTriangleCount(solver.OniSolver);

			// Send deformable triangle indices to the solver:
			UpdateDeformableTriangles();

			// Send our particle data to the solver:
			PushDataToSolver(ParticleData.ALL);

			// Add constraints to solver:
			foreach (ObiBatchedConstraints c in constraints)
				if (c != null)
					c.AddToSolver(null);

			if (OnAddedToSolver != null)
				OnAddedToSolver(this,new ObiActorSolverArgs(solver));

			return true;
		}
		
		return false;
	}

	public void UpdateDeformableTriangles(){

		// Send deformable triangle indices to the solver:
		int[] solverTriangles = new int[deformableTriangles.Length];
		for (int i = 0; i < deformableTriangles.Length; ++i)
		{
			solverTriangles[i] = particleIndices[deformableTriangles[i]];
		}
		Oni.SetDeformableTriangles(solver.OniSolver,solverTriangles,solverTriangles.Length/3,trianglesOffset);
	}
	
	/**
	 * Removes this actor from its current solver, if any.
	 */
	public virtual bool RemoveFromSolver(object info){

		if (solver != null && InSolver){

			// remove constraints from solver:
			foreach (ObiBatchedConstraints c in constraints)
				if (c != null)
					c.RemoveFromSolver(null);

			// remove rest positions:
 			Vector4[] noRest = {new Vector4(0,0,0,1)};
			for (int i = 0; i < particleIndices.Length; ++i){
				Oni.SetRestPositions(solver.OniSolver,noRest,1,particleIndices[i]);
			}
	
			int index = solver.RemoveActor(this);
			particleIndices = null;

			// update other actor's triangle offset:
			for (int i = index; i < solver.actors.Count; i++){
				solver.actors[i].trianglesOffset -= deformableTriangles.Length/3;
			}	
			// remove triangles:
			Oni.RemoveDeformableTriangles(solver.OniSolver,deformableTriangles.Length/3,trianglesOffset);

			inSolver = false;

			if (OnRemovedFromSolver != null)
				OnRemovedFromSolver(this,new ObiActorSolverArgs(solver));

			return true;
		}
		
		return false;
		
	}

	/**
	 * Sends local particle data to the solver.
	 */
	public virtual void PushDataToSolver(ParticleData data = ParticleData.NONE){

		if (!InSolver) return;

		Matrix4x4 l2sTransform;
		if (Solver.simulateInLocalSpace)
			l2sTransform = Solver.transform.worldToLocalMatrix * ActorLocalToWorldMatrix * initialScaleMatrix.inverse;
		else 
			l2sTransform = ActorLocalToWorldMatrix * initialScaleMatrix.inverse;

		for (int i = 0; i < particleIndices.Length; i++){
			int k = particleIndices[i];

			if ((data & ParticleData.POSITIONS) != 0 && i < positions.Length)
				Oni.SetParticlePositions(solver.OniSolver,new Vector4[]{l2sTransform.MultiplyPoint3x4(positions[i])},1,k);

			if ((data & ParticleData.ORIENTATIONS) != 0 && orientations != null && i < orientations.Length)
				Oni.SetParticleOrientations(solver.OniSolver,new Quaternion[]{orientations[i]},1,k); // TODO: transform orientation.

			if ((data & ParticleData.VELOCITIES) != 0 && i < velocities.Length)
				Oni.SetParticleVelocities(solver.OniSolver,new Vector4[]{l2sTransform.MultiplyVector(velocities[i])},1,k);

			if ((data & ParticleData.ANGULAR_VELOCITIES) != 0 && angularVelocities != null && i < angularVelocities.Length)
				Oni.SetParticleAngularVelocities(solver.OniSolver,new Vector4[]{l2sTransform.MultiplyVector(angularVelocities[i])},1,k);

			if ((data & ParticleData.INV_MASSES) != 0 && i < invMasses.Length)
				Oni.SetParticleInverseMasses(solver.OniSolver,new float[]{invMasses[i]},1,k);

			if ((data & ParticleData.SOLID_RADII) != 0 && i < solidRadii.Length)
				Oni.SetParticleSolidRadii(solver.OniSolver,new float[]{solidRadii[i]},1,k);

			if ((data & ParticleData.PRINCIPAL_RADII) != 0 && principalRadii != null && i < principalRadii.Length)
				Oni.SetParticlePrincipalRadii(solver.OniSolver,new Vector2[]{principalRadii[i]},1,k);

			if ((data & ParticleData.PHASES) != 0 && i < phases.Length)
				Oni.SetParticlePhases(solver.OniSolver,new int[]{phases[i]},1,k);

			if ((data & ParticleData.REST_POSITIONS) != 0 && i < restPositions.Length)
				Oni.SetRestPositions(solver.OniSolver,new Vector4[]{restPositions[i]},1,k);
		}

		if ((data & ParticleData.COLLISION_MATERIAL) != 0){
			IntPtr[] materials = new IntPtr[particleIndices.Length];
			for (int i = 0; i < particleIndices.Length; i++) 
				materials[i] = collisionMaterial != null ? collisionMaterial.OniCollisionMaterial : IntPtr.Zero;
			Oni.SetCollisionMaterials(solver.OniSolver,materials,particleIndices,particleIndices.Length);
		}
        
        if ((data & ParticleData.ACTIVE_STATUS) != 0)
			solver.UpdateActiveParticles();

	}

	/**
	 * Retrieves particle simulation data from the solver. Common uses are
	 * retrieving positions and velocities to set the initial status of the simulation,
 	 * or retrieving solver-generated data such as tensions, densities, etc.
	 */
	public virtual void PullDataFromSolver(ParticleData data = ParticleData.NONE){
		
		if (!InSolver) return;

		Matrix4x4 l2sTransform;
		if (Solver.simulateInLocalSpace)
			l2sTransform = Solver.transform.worldToLocalMatrix * ActorLocalToWorldMatrix * initialScaleMatrix.inverse;
		else 
			l2sTransform = ActorLocalToWorldMatrix * initialScaleMatrix.inverse;
		l2sTransform = l2sTransform.inverse;

		for (int i = 0; i < particleIndices.Length; i++){
			int k = particleIndices[i];
			if ((data & ParticleData.POSITIONS) != 0){
				Vector4[] ssPosition = {positions[i]};
				Oni.GetParticlePositions(solver.OniSolver,ssPosition,1,k);
				positions[i] = l2sTransform.MultiplyPoint3x4(ssPosition[0]);
			}
			if ((data & ParticleData.ORIENTATIONS) != 0 && orientations != null){
				Quaternion[] ssOrientation = {orientations[i]};
				Oni.GetParticleOrientations(solver.OniSolver,ssOrientation,1,k);
				orientations[i] = ssOrientation[0]; // TODO: transform orientation.
			}
			if ((data & ParticleData.VELOCITIES) != 0){
				Vector4[] ssVelocity = {velocities[i]};
				Oni.GetParticleVelocities(solver.OniSolver,ssVelocity,1,k);
				velocities[i] = l2sTransform.MultiplyVector(ssVelocity[0]);
			}
			if ((data & ParticleData.ANGULAR_VELOCITIES) != 0 && angularVelocities != null){
				Vector4[] ssAVelocity = {angularVelocities[i]};
				Oni.GetParticleAngularVelocities(solver.OniSolver,ssAVelocity,1,k);
				angularVelocities[i] = l2sTransform.MultiplyVector(ssAVelocity[0]);
			}
		}
		
	}

	/**
	 * Returns the position of a particle in world space. 
	 * Works both when the actor is managed by a solver and when it isn't. 
	 */
	public Vector3 GetParticlePosition(int index){
		if (InSolver)
			return solver.renderablePositions[particleIndices[index]];
		else
			return (ActorLocalToWorldMatrix * initialScaleMatrix.inverse).MultiplyPoint3x4(positions[index]);
	}

	public virtual bool GenerateTethers(TetherType type){
		return true;
	}

	public void ClearTethers(){
		if (constraints[(int)Oni.ConstraintType.Tether] != null){
			((ObiTetherConstraints)constraints[(int)Oni.ConstraintType.Tether]).Clear();
		}
	}

	public virtual void OnSolverPreInterpolation(){
	}

	/**
	 * Transforms the position of fixed particles from local space to simulation space and feeds them
	 * to the solver. This is performed just before performing simulation each frame.
	 */
	public virtual void OnSolverStepBegin(){

		// check if any of the involved transforms has changed since last time:
		if (!transform.hasChanged && !Solver.transform.hasChanged)
			return;

		transform.hasChanged = false;
		Solver.transform.hasChanged = false;

		// apparently checking whether the actor is enabled or not doesn't take a despreciable amount of time.
		bool actorEnabled = this.enabled;

		int particleCount = particleIndices.Length;
		Vector4[] simulationPosition = {Vector4.zero};

		// build local to simulation space transform:
		Matrix4x4 l2sTransform;
		if (Solver.simulateInLocalSpace)
			l2sTransform = Solver.transform.worldToLocalMatrix * ActorLocalToWorldMatrix * initialScaleMatrix.inverse;
		else 
			l2sTransform = ActorLocalToWorldMatrix * initialScaleMatrix.inverse;

		//Matrix4x4 delta = Solver.transform.worldToLocalMatrix * Solver.LastTransform;

		// transform particles:
		for(int i = 0; i < particleCount; i++){

			if (!actorEnabled || invMasses[i] == 0){

				simulationPosition[0] = l2sTransform.MultiplyPoint3x4(positions[i]);
				Oni.SetParticlePositions(solver.OniSolver,simulationPosition,1,particleIndices[i]);

			}/*else if (Solver.simulateInLocalSpace){

				Oni.GetParticlePositions(solver.OniSolver,simulationPosition,1,particleIndices[i]);
				simulationPosition[0] = Vector3.Lerp(simulationPosition[0],delta.MultiplyPoint3x4(simulationPosition[0]),worldVelocityScale);
				Oni.SetParticlePositions(solver.OniSolver,simulationPosition,1,particleIndices[i]);

			}*/
		}
		
	}

	public virtual void OnSolverStepEnd(){
	}

	public virtual void OnSolverFrameBegin(){
	}

	public virtual void OnSolverFrameEnd(){	
    }

	public virtual void OnSolverVisibilityChanged(bool visible){
	}

	public virtual bool ReadParticlePropertyFromTexture(Texture2D source,System.Action<int,Color> onReadProperty){return false;}
}
}

