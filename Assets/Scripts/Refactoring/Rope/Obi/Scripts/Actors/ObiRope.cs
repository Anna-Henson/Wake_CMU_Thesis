using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{
	
	/**
	 * Rope made of Obi particles. No mesh or topology is needed to generate a physic representation from,
	 * since the mesh is generated procedurally.
	 */
	[ExecuteInEditMode]
	[AddComponentMenu("Physics/Obi/Obi Rope")]
	[RequireComponent(typeof (MeshRenderer))]
	[RequireComponent(typeof (MeshFilter))]
	[RequireComponent(typeof (ObiDistanceConstraints))]
	[RequireComponent(typeof (ObiBendingConstraints))]
	[RequireComponent(typeof (ObiTetherConstraints))]
	[RequireComponent(typeof (ObiPinConstraints))]
	[DisallowMultipleComponent]
	public class ObiRope : ObiActor
	{
		public const float DEFAULT_PARTICLE_MASS = 0.1f;
		public const float MAX_YOUNG_MODULUS = 200.0f; //that of high carbon steel (N/m2);
		public const float MIN_YOUNG_MODULUS = 0.0001f; //that of polymer foam (N/m2);

		/**
		 * How to render the rope.
		 */
		public enum RenderingMode
		{
			ProceduralRope,
			Chain,
			Line,
			CustomMesh
		}
	
		public class CurveFrame{

			public Vector3 position = Vector3.zero;
			public Vector3 tangent = Vector3.forward;
			public Vector3 normal = Vector3.up;
			public Vector3 binormal = Vector3.left;

			public void Reset(){
				position = Vector3.zero;
				tangent = Vector3.forward;
				normal = Vector3.up;
				binormal = Vector3.left;
			}

			public void SetTwist(float twist){
				Quaternion twistQ = Quaternion.AngleAxis(twist,tangent);
				normal = twistQ*normal;
				binormal = twistQ*binormal;
			}

			public void SetTwistAndTangent(float twist, Vector3 tangent){
				this.tangent = tangent;
				normal = new Vector3(-tangent.y,tangent.x,0).normalized;
				binormal = Vector3.Cross(tangent,normal);

				Quaternion twistQ = Quaternion.AngleAxis(twist,tangent);
				normal = twistQ*normal;
				binormal = twistQ*binormal;
			}

			public void Transport(Vector3 newPosition, Vector3 newTangent, float twist){

				// Calculate delta rotation:
				Quaternion rotQ = Quaternion.FromToRotation(tangent,newTangent);
				Quaternion twistQ = Quaternion.AngleAxis(twist,newTangent);
				Quaternion finalQ = twistQ*rotQ;
				
				// Rotate previous frame axes to obtain the new ones:
				normal = finalQ*normal;
				binormal = finalQ*binormal;
				tangent = newTangent;
				position = newPosition;

			}

			public void DrawDebug(){
				Debug.DrawRay(position,normal,Color.blue);
				Debug.DrawRay(position,tangent,Color.red);
				Debug.DrawRay(position,binormal,Color.green);
			}
		}

		public struct CurveSection{
			public Vector4 positionAndRadius;
			public Vector4 color;

			public CurveSection(Vector4 positionAndRadius,Vector4 color){
				this.positionAndRadius = positionAndRadius;
				this.color = color;
			}

			public CurveSection(Vector3 position, float radius ,Vector4 color){
				this.positionAndRadius = new Vector4(position.x,position.y,position.z,radius);
				this.color = color;
			}

			public static CurveSection operator +(CurveSection c1, CurveSection c2) 
		    {
		    	return new CurveSection(c1.positionAndRadius + c2.positionAndRadius, c1.color + c2.color);
		    }

			public static CurveSection operator *(float f,CurveSection c) 
		    {
		    	return new CurveSection(c.positionAndRadius * f, c.color * f);
		    }
		}

		[Tooltip("Amount of additional particles in this rope's pool that can be used to extend its lenght, or to tear it.")]
		public int pooledParticles = 10;

		[Tooltip("Path used to generate the rope.")]
		public ObiCurve ropePath = null;

		public bool thicknessFromParticles = true;	/**< Gets rope thickness from particle radius.*/

		[Tooltip("Thickness of the rope, it is equivalent to particle radius.")]
		[Indent]
		[VisibleIf("thicknessFromParticles",true)]
		public float thickness = 0.05f;				/**< Thickness of the rope.*/

		[Tooltip("Modulates the amount of particles per lenght unit. 1 means as many particles as needed for the given length/thickness will be used, which"+
				 "can be a lot in very thin and long ropes. Setting values between 0 and 1 allows you to override the amount of particles used.")]
		[Range(0,1)]
		public float resolution = 0.5f;												/**< modulates resolution of particle representation.*/

		[Range(0,3)]
		public uint smoothing = 1;						/**< Amount of smoothing applied to the particle representation.*/

		public bool tearable = false;

		[Tooltip("Maximum strain betweeen particles before the spring constraint holding them together would break.")]
		[Indent(order = 0)]
		[VisibleIf("tearable",order = 1)]
		[MultiDelayed(order = 2)]
		public float tearResistanceMultiplier = 1000;

		[Indent]
		[VisibleIf("tearable")]
		public GameObject tearPrefab;	

		public GameObject startPrefab;
		public GameObject endPrefab;	

		[HideInInspector] public float[] tearResistance; 	/**< Per-particle tear resistances.*/
		[HideInInspector][NonSerialized] public Mesh ropeMesh;

		[Header("Rendering")]
		[SerializeProperty("RenderMode")]
		public RenderingMode renderMode = RenderingMode.ProceduralRope;

		[VisibleIf("IsRenderModeLine",order = 0)]
		[ChildrenOnly(order = 1)]
		public ObiRopeLineRenderMode lineRenderMode = new ObiRopeLineRenderMode();

		[VisibleIf("IsRenderModeThick",order = 0)]
		[ChildrenOnly(order = 1)]
		public ObiRopeThickRenderMode thickRenderMode = new ObiRopeThickRenderMode();	

		[VisibleIf("IsRenderModeChain",order = 0)]	
		[ChildrenOnly(order = 1)]
		public ObiRopeChainRenderMode chainRenderMode = new ObiRopeChainRenderMode();

		[VisibleIf("IsRenderModeMesh",order = 0)]
		[ChildrenOnly(order = 1)]
		public ObiRopeMeshRenderMode meshRenderMode = new ObiRopeMeshRenderMode();

		[VisibleIf("IsRenderModeProcedural",order = 0)]
		[MultiRange(0,1, order = 1)]
		public float uvAnchor = 0;					/**< Normalized position of texture coordinate origin along rope.*/

		[VisibleIf("IsRenderModeProcedural")]
		public Vector2 uvScale = Vector2.one;		/**< Scaling of uvs along rope.*/

		[VisibleIf("IsRenderModeProcedural")]
		public bool normalizeV = true;

		[VisibleIf("IsRenderModeProcedural")]
		public ObiRopeSection section = null;		/**< Section asset to be extruded along the rope.*/

		[VisibleIf("RenderModeSupportsTwist")]
		public float sectionTwist = 0;				/**< Amount of twist applied to each section, in degrees.*/

		[VisibleIf("IsRenderModeProcedural")]
		public float sectionThicknessScale = 0.8f;	/**< Scales section thickness.*/

		public List<GameObject> chainLinks = new List<GameObject>();

		[HideInInspector][NonSerialized] public GameObject[] tearPrefabPool;

		[HideInInspector][SerializeField] private bool closed = false;
		[HideInInspector][SerializeField] private float interParticleDistance = 0;
		[SerializeField] private float restLength = 0;
		[HideInInspector][SerializeField] private int usedParticles = 0;
		[HideInInspector][SerializeField] private int totalParticles = 0;

		[HideInInspector] public GameObject startPrefabInstance;
		[HideInInspector] public GameObject endPrefabInstance;

		private float curveLength = 0;
		private int curveSections = 0;
		[HideInInspector][NonSerialized] public ObiList<ObiList<CurveSection>> rawCurves = new ObiList<ObiList<CurveSection>>(); 
		[HideInInspector][NonSerialized] public ObiList<ObiList<CurveSection>> curves = new ObiList<ObiList<CurveSection>>(); 

		public ObiDistanceConstraints DistanceConstraints{
			get{return constraints[(int)Oni.ConstraintType.Distance] as ObiDistanceConstraints;}
		}
		public ObiBendingConstraints BendingConstraints{
			get{return constraints[(int)Oni.ConstraintType.Bending] as ObiBendingConstraints;}
		}
		public ObiTetherConstraints TetherConstraints{
			get{return constraints[(int)Oni.ConstraintType.Tether] as ObiTetherConstraints;}
		}
		public ObiPinConstraints PinConstraints{
			get{return constraints[(int)Oni.ConstraintType.Pin] as ObiPinConstraints;}
		}

		public RenderingMode RenderMode{
			set{
				if (value != renderMode){

					TearDownVisualRepresentation();

					renderMode = value;

					InitializeVisualRepresentation();
				}	
			}
			get{return renderMode;}
		}

		public float InterparticleDistance{
			get{return interParticleDistance * DistanceConstraints.stretchingScale;}
		}

		public int TotalParticles{
			get{return totalParticles;}
		}

		public int UsedParticles{
			get{return usedParticles;}
			set{
				usedParticles = value;
				pooledParticles = totalParticles-usedParticles;
			}
		}

		public float RestLength{
			get{return restLength;}
			set{restLength = value;}
		}

		public float SmoothLength{
			get{return curveLength;}
		}

		public int SmoothSections{
			get{return curveSections;}
		}

		public bool Closed{
			get{return closed;}
		}

		public int PooledParticles{
			get{return pooledParticles;}
		}

		private bool IsRenderModeProcedural(){
			return renderMode == RenderingMode.ProceduralRope || renderMode == RenderingMode.Line;
		}
	
		private bool IsRenderModeThick(){
			return renderMode == RenderingMode.ProceduralRope;
		}
		private bool IsRenderModeChain(){
			return renderMode == RenderingMode.Chain;
		}
		private bool IsRenderModeLine(){
			return renderMode == RenderingMode.Line;
		}
		private bool IsRenderModeMesh(){
			return renderMode == RenderingMode.CustomMesh;
		}
		private bool RenderModeSupportsTwist(){
			return renderMode != RenderingMode.Line;
		}

		public override void Awake(){
			base.Awake();
		
			ropeMesh = new Mesh();
			ropeMesh.name = "ropeMesh";
			ropeMesh.MarkDynamic();
			GetComponent<MeshFilter>().mesh = ropeMesh;

			chainRenderMode.linkInstances = new List<GameObject>();
		}

		public override void Start()
		{
			base.Start();

			thickRenderMode.Rope = 
			lineRenderMode.Rope =
			chainRenderMode.Rope = 
			meshRenderMode.Rope = this;

			Camera.onPreCull += RopePreCull;

			InitializeVisualRepresentation();
		}
	     
		public void OnValidate(){

			thickness = Mathf.Max(0.0001f,thickness);
			uvAnchor = Mathf.Clamp01(uvAnchor);
			tearResistanceMultiplier = Mathf.Max(0.1f,tearResistanceMultiplier);
			resolution = Mathf.Max(0.0001f,resolution);

			UpdateVisualRepresentation();

	    }

	    public void RopePreCull(Camera cam)
	    {
			// before this camera culls the scene, grab the camera position and update the mesh.
			if (Initialized && renderMode == RenderingMode.Line){
				this.lineRenderMode.Update(cam);
			}
	    }

		public override void OnSolverStepEnd(){	

			base.OnSolverStepEnd();

			if (isActiveAndEnabled){
				ApplyTearing();
	
				// breakable pin constraints:
				PinConstraints.BreakConstraints();
			}
		}

		public override void OnSolverFrameEnd(){
			
			base.OnSolverFrameEnd();

			UpdateVisualRepresentation();
			
		}
		
		public override void OnDestroy(){
			base.OnDestroy();

			Camera.onPreCull -= RopePreCull;

			thickRenderMode.TearDown();
			lineRenderMode.TearDown();
			chainRenderMode.TearDown();
			meshRenderMode.TearDown();

			GameObject.DestroyImmediate(ropeMesh);

			ClearPrefabInstances();
		}
		
		public override bool AddToSolver(object info){
			
			if (Initialized && base.AddToSolver(info)){

				solver.RequireRenderablePositions();

				return true;
			}
			return false;
		}
		
		public override bool RemoveFromSolver(object info){
			
			if (solver != null)
				solver.RelinquishRenderablePositions();

			return base.RemoveFromSolver(info);
		}
		
		/**
	 	* Generates the particle based physical representation of the rope. This is the initialization method for the rope object
		* and should not be called directly once the object has been created.
	 	*/
		protected override IEnumerator Initialize()
		{	
			initialized = false;			
			initializing = true;	
			interParticleDistance = -1;

			RemoveFromSolver(null);

			if (ropePath == null){
				Debug.LogError("Cannot initialize rope. There's no ropePath present. Please provide a spline to define the shape of the rope");
				yield break;
			}

			ropePath.RecalculateSplineLenght(0.00001f,7);
			closed = ropePath.Closed;
			restLength = ropePath.Length;

			usedParticles = Mathf.CeilToInt(restLength/thickness * resolution) + (closed ? 0:1);
			totalParticles = usedParticles + pooledParticles; //allocate extra particles to allow for lenght change and tearing.

			active = new bool[totalParticles];
			positions = new Vector3[totalParticles];
			velocities = new Vector3[totalParticles];
			invMasses  = new float[totalParticles];
			solidRadii = new float[totalParticles];
			principalRadii = new Vector2[totalParticles];
			phases = new int[totalParticles];
			restPositions = new Vector4[totalParticles];
			tearResistance = new float[totalParticles];
			colors = new Color[totalParticles];
			
			int numSegments = usedParticles - (closed ? 0:1);
			if (numSegments > 0)
				interParticleDistance = restLength/(float)numSegments;
			else 
				interParticleDistance = 0;

			float radius = interParticleDistance * resolution;

			for (int i = 0; i < usedParticles; i++){

				active[i] = true;
				invMasses[i] = 1.0f/DEFAULT_PARTICLE_MASS;
				float mu = ropePath.GetMuAtLenght(interParticleDistance*i);
				positions[i] = transform.InverseTransformPoint(ropePath.transform.TransformPoint(ropePath.GetPositionAt(mu)));
				solidRadii[i] = radius;
				principalRadii[i] = new Vector2(solidRadii[i],solidRadii[i]);
				phases[i] = Oni.MakePhase(1,selfCollisions?Oni.ParticlePhase.SelfCollide:0);
				tearResistance[i] = 1;
				colors[i] = Color.white;

				if (i % 100 == 0)
					yield return new CoroutineJob.ProgressInfo("ObiRope: generating particles...",i/(float)usedParticles);

			}

			// Initialize basic data for pooled particles:
			for (int i = usedParticles; i < totalParticles; i++){

				active[i] = false;
				invMasses[i] = 1.0f/DEFAULT_PARTICLE_MASS;
				solidRadii[i] = radius;
				principalRadii[i] = new Vector2(solidRadii[i],solidRadii[i]);
				phases[i] = Oni.MakePhase(1,selfCollisions?Oni.ParticlePhase.SelfCollide:0);
				tearResistance[i] = 1;
				colors[i] = Color.white;

				if (i % 100 == 0)
					yield return new CoroutineJob.ProgressInfo("ObiRope: generating particles...",i/(float)usedParticles);

			}

			DistanceConstraints.Clear();
			ObiDistanceConstraintBatch distanceBatch = new ObiDistanceConstraintBatch(false,false,MIN_YOUNG_MODULUS,MAX_YOUNG_MODULUS);
			DistanceConstraints.AddBatch(distanceBatch);

			for (int i = 0; i < numSegments; i++){

				distanceBatch.AddConstraint(i,(i+1) % (ropePath.Closed ? usedParticles:usedParticles+1),interParticleDistance,1,1);		

				if (i % 500 == 0)
					yield return new CoroutineJob.ProgressInfo("ObiRope: generating structural constraints...",i/(float)numSegments);

			}

			BendingConstraints.Clear();
			ObiBendConstraintBatch bendingBatch = new ObiBendConstraintBatch(false,false,MIN_YOUNG_MODULUS,MAX_YOUNG_MODULUS);
			BendingConstraints.AddBatch(bendingBatch);
			for (int i = 0; i < usedParticles - (closed?0:2); i++){

				// rope bending constraints always try to keep it completely straight:
				bendingBatch.AddConstraint(i,(i+2) % usedParticles,(i+1) % usedParticles,0,0,1);
			
				if (i % 500 == 0)
					yield return new CoroutineJob.ProgressInfo("ObiRope: adding bend constraints...",i/(float)usedParticles);

			}
			
			// Initialize tether constraints:
			TetherConstraints.Clear();

			// Initialize pin constraints:
			PinConstraints.Clear();
			ObiPinConstraintBatch pinBatch = new ObiPinConstraintBatch(false,false,0,MAX_YOUNG_MODULUS);
			PinConstraints.AddBatch(pinBatch);

			initializing = false;
			initialized = true;

			RegenerateRestPositions();
			InitializeVisualRepresentation();

		}

		/**
		 * Generates new valid rest positions for the entire rope.
		 */
		public void RegenerateRestPositions(){

			ObiDistanceConstraintBatch distanceBatch = DistanceConstraints.GetFirstBatch();		

			// Iterate trough all distance constraints in order:
			int particle = -1;
			int lastParticle = -1;
			float accumulatedDistance = 0;
			for (int i = 0; i < distanceBatch.ConstraintCount; ++i){

				if (i == 0){
					lastParticle = particle = distanceBatch.springIndices[i*2];
					restPositions[particle] = Vector4.zero;
				}		
				
				accumulatedDistance += Mathf.Min(interParticleDistance,solidRadii[particle],solidRadii[lastParticle]);

				particle = distanceBatch.springIndices[i*2+1];
				restPositions[particle] = Vector3.right * accumulatedDistance;
				restPositions[particle][3] = 0; // activate rest position

			}

			PushDataToSolver(ParticleData.REST_POSITIONS);
		}

		/**
		 * Recalculates rest rope length.
		 */
		public void RecalculateLenght(){

			ObiDistanceConstraintBatch distanceBatch = DistanceConstraints.GetFirstBatch();		

			restLength = 0;

			// Iterate trough all distance constraints in order:
			for (int i = 0; i < distanceBatch.ConstraintCount; ++i)
				restLength += distanceBatch.restLengths[i];
			
		}

		/**
		 * Returns actual rope length, including stretch.
		 */
		public float CalculateLength(){

			ObiDistanceConstraintBatch batch = DistanceConstraints.GetFirstBatch();

			// iterate over all distance constraints and accumulate their length:
			float actualLength = 0;
			Vector3 a,b;
			for (int i = 0; i < batch.ConstraintCount; ++i){
				a = GetParticlePosition(batch.springIndices[i*2]);
				b = GetParticlePosition(batch.springIndices[i*2+1]);
				actualLength += Vector3.Distance(a,b);
			}
			return actualLength;
		}

		/**
		 * Generates any precomputable data for the current visual representation.
		 */
		protected void InitializeVisualRepresentation(){

			if (!Initialized) return;

			// create start/end prefabs
			GeneratePrefabInstances();

			switch(renderMode){
				case RenderingMode.Chain: chainRenderMode.Initialize(); break;
				case RenderingMode.Line: lineRenderMode.Initialize(); break;
				case RenderingMode.ProceduralRope: thickRenderMode.Initialize(); break;
				case RenderingMode.CustomMesh: meshRenderMode.Initialize(); break;
			}
		}

		/**
		 * Updates the current visual representation.
		 */
		protected void UpdateVisualRepresentation(){

			if (!isActiveAndEnabled || !Initialized) return;

			switch(renderMode){

				// line rendering mode is updated in the camera's OnPreCull.
				case RenderingMode.Chain: chainRenderMode.Update(null); break;
				case RenderingMode.ProceduralRope: thickRenderMode.Update(null); break;
				case RenderingMode.CustomMesh: meshRenderMode.Update(null); break;
			}
		}	

		protected void TearDownVisualRepresentation(){

			if (!Initialized) return;

			switch(renderMode){
				case RenderingMode.Chain: chainRenderMode.TearDown(); break;
				case RenderingMode.Line: lineRenderMode.TearDown(); break;
				case RenderingMode.ProceduralRope: thickRenderMode.TearDown(); break;
				case RenderingMode.CustomMesh: meshRenderMode.TearDown(); break;
			}
		}

		protected void GeneratePrefabInstances(){

			ClearPrefabInstances();

			if (tearPrefab != null){

				// create tear prefab pool, two per potential cut:
				tearPrefabPool = new GameObject[pooledParticles*2];

				for (int i = 0; i < tearPrefabPool.Length; ++i){
					GameObject tearPrefabInstance = GameObject.Instantiate(tearPrefab);
					tearPrefabInstance.hideFlags = HideFlags.HideAndDontSave;
					tearPrefabInstance.SetActive(false);
					tearPrefabPool[i] = tearPrefabInstance;
				}

			}

			// create start/end prefabs
			if (startPrefabInstance == null && startPrefab != null){
				startPrefabInstance = GameObject.Instantiate(startPrefab);
				startPrefabInstance.hideFlags = HideFlags.HideAndDontSave;
			}
			if (endPrefabInstance == null && endPrefab != null){
				endPrefabInstance = GameObject.Instantiate(endPrefab);
				endPrefabInstance.hideFlags = HideFlags.HideAndDontSave;
			}
		}

		/**
		 * Destroys all prefab instances used as start/end caps and tear prefabs.
		 */
		protected void ClearPrefabInstances(){

			GameObject.DestroyImmediate(startPrefabInstance);
			GameObject.DestroyImmediate(endPrefabInstance);

			if (tearPrefabPool != null){
				for (int i = 0; i < tearPrefabPool.Length; ++i){
					if (tearPrefabPool[i] != null){
						GameObject.DestroyImmediate(tearPrefabPool[i]);
						tearPrefabPool[i] = null;
					}
				}
			}
		}

		/** 
		 * This method uses Chainkin's algorithm to produce a smooth curve from a set of control points. It is specially fast
		 * because it directly calculates subdivision level k, instead of recursively calculating levels 1..k.
		 */
		protected void ChaikinSmoothing(ObiList<CurveSection> input, ObiList<CurveSection> output, uint k)
		{
			// no subdivision levels, no work to do. just copy the input to the output:
			if (k == 0 || input.Count < 3){
				output.SetCount(input.Count);
				for (int i = 0; i < input.Count; ++i)
					output[i] = input[i];
				return;
			}

			// calculate amount of new points generated by each inner control point:
			int pCount = (int)Mathf.Pow(2,k);

			// precalculate some quantities:
			int n0 = input.Count-1;
			float twoRaisedToMinusKPlus1 = Mathf.Pow(2,-(k+1));
			float twoRaisedToMinusK = Mathf.Pow(2,-k);
			float twoRaisedToMinus2K = Mathf.Pow(2,-2*k);
			float twoRaisedToMinus2KMinus1 = Mathf.Pow(2,-2*k-1);

			// allocate ouput:
			output.SetCount((n0-1) * pCount + 2); 

			// calculate initial curve points:
			output[0] = 				 (0.5f + twoRaisedToMinusKPlus1) * input[0] + (0.5f - twoRaisedToMinusKPlus1) * input[1];
			output[pCount*n0-pCount+1] = (0.5f - twoRaisedToMinusKPlus1) * input[n0-1] + (0.5f + twoRaisedToMinusKPlus1) * input[n0];

			// calculate internal points:
			for (int j = 1; j <= pCount; ++j){

				// precalculate coefficients:
				float F = 0.5f - twoRaisedToMinusKPlus1 - (j-1)*(twoRaisedToMinusK - j*twoRaisedToMinus2KMinus1);
				float G = 0.5f + twoRaisedToMinusKPlus1 + (j-1)*(twoRaisedToMinusK - j*twoRaisedToMinus2K); 
				float H = (j-1)*j*twoRaisedToMinus2KMinus1;

				for (int i = 1; i < n0; ++i){
					output[(i-1)*pCount+j] = F*input[i-1] + G*input[i] + H*input[i+1];
				}
			}

			// make first and last curve points coincide with original points:
			output[0] = input[0];	
			output[output.Count-1] = input[input.Count-1];	
		}	

		protected float CalculateCurveLength(ObiList<CurveSection> curve){
			float length = 0;
			for (int i = 1; i < curve.Count; ++i){
				length += Vector3.Distance(curve[i].positionAndRadius,curve[i-1].positionAndRadius);
			}
			return length;
		}

		/**
		 * Returns the index of the distance constraint at a given normalized rope coordinate.
		 */
		public int GetConstraintIndexAtNormalizedCoordinate(float coord){

			// Nothing guarantees particle index order is the same as particle ordering in the rope.
			// However distance constraints must be ordered, so we'll use that:

			ObiDistanceConstraintBatch distanceBatch = DistanceConstraints.GetFirstBatch();	
		
			float mu = coord * distanceBatch.ConstraintCount;
			return Mathf.Clamp(Mathf.FloorToInt(mu),0,distanceBatch.ConstraintCount-1);
		}

		protected void AddCurve(int sections){

			if (sections > 1){

				if (rawCurves.Data[rawCurves.Count] == null){
					rawCurves.Data[rawCurves.Count] = new ObiList<CurveSection>();
					curves.Data[curves.Count] = new ObiList<CurveSection>();
				}

				rawCurves.Data[rawCurves.Count].SetCount(sections);

				rawCurves.SetCount(rawCurves.Count+1);
				curves.SetCount(curves.Count+1);
			}
		}

		/**
		 * Counts the amount of continuous sections in each chunk of rope.
		 */
		protected void CountContinuousSegments(){

			rawCurves.Clear();
			ObiDistanceConstraintBatch distanceBatch = DistanceConstraints.GetFirstBatch();		

			int segmentCount = 0;
			int lastParticle = -1;

			// Iterate trough all distance constraints in order. If we find a discontinuity, reset segment count:
			for (int i = 0; i < distanceBatch.ConstraintCount; ++i){

				int particle1 = distanceBatch.springIndices[i*2];
				int particle2 = distanceBatch.springIndices[i*2+1];
			
				// start new curve at discontinuities:
				if (particle1 != lastParticle && segmentCount > 0){
					
					// add a new curve with the correct amount of sections: 
					AddCurve(segmentCount+1);
					segmentCount = 0;
				}

				lastParticle = particle2;
				segmentCount++;
			}

			// add the last curve:
			AddCurve(segmentCount+1);
		}

		/** 
		 * Generate a list of smooth curves using particles as control points. Will take into account cuts in the rope,
		 * generating one curve for each continuous piece of rope.
		 */
		public void SmoothCurvesFromParticles(){

			curves.Clear();

			curveSections = 0;
			curveLength = 0;

			// count amount of segments in each rope chunk:
			CountContinuousSegments();

			ObiDistanceConstraintBatch distanceBatch = DistanceConstraints.GetFirstBatch();	
			Matrix4x4 w2l = transform.worldToLocalMatrix;

			int firstSegment = 0;

			// generate curve for each rope chunk:
			for (int i = 0; i < rawCurves.Count; ++i){

				int segments = rawCurves[i].Count-1;

				// allocate memory for the curve:
				ObiList<CurveSection> controlPoints = rawCurves[i];

				// get control points position:
				for (int m = 0; m < segments; ++m){

					int particleIndex = distanceBatch.springIndices[(firstSegment + m)*2];
					controlPoints[m] = new CurveSection(w2l.MultiplyPoint3x4(GetParticlePosition(particleIndex)),
													    solidRadii[particleIndex],
					(this.colors != null && particleIndex < this.colors.Length) ? this.colors[particleIndex] : Color.white);

					// last segment adds its second particle too:
					if (m == segments-1){
						particleIndex = distanceBatch.springIndices[(firstSegment + m)*2+1];
						controlPoints[m+1] = new CurveSection(w2l.MultiplyPoint3x4(GetParticlePosition(particleIndex)),
													          solidRadii[particleIndex],
						(this.colors != null && particleIndex < this.colors.Length) ? this.colors[particleIndex] : Color.white);
					}
				}

				firstSegment += segments;

				// get smooth curve points:
				ChaikinSmoothing(controlPoints,curves[i],smoothing);

				// count total curve sections and total curve length:
				curveSections += curves[i].Count-1;
				curveLength += CalculateCurveLength(curves[i]);
			}
	
		}

		public void PlaceObjectAtCurveFrame(CurveFrame frame, GameObject obj, Space space, bool reverseLookDirection){
			if (space == Space.Self){
				Matrix4x4 l2w = transform.localToWorldMatrix;
				obj.transform.position = l2w.MultiplyPoint3x4(frame.position);
				if (frame.tangent != Vector3.zero)
					obj.transform.rotation = Quaternion.LookRotation(l2w.MultiplyVector(reverseLookDirection ? frame.tangent:-frame.tangent),
																 	 l2w.MultiplyVector(frame.normal));
			}else{
				obj.transform.position = frame.position;
				if (frame.tangent != Vector3.zero)
					obj.transform.rotation = Quaternion.LookRotation(reverseLookDirection ? frame.tangent:-frame.tangent,frame.normal);
			}
		}
		
		/**
 		* Resets mesh to its original state.
 		*/
		public override void ResetActor(){
	
			PushDataToSolver(ParticleData.POSITIONS | ParticleData.VELOCITIES);
			
			if (particleIndices != null){
				for(int i = 0; i < particleIndices.Length; ++i){
					solver.renderablePositions[particleIndices[i]] = positions[i];
				}
			}

			UpdateVisualRepresentation();

		}

		protected void ApplyTearing(){

			if (!tearable) 
				return;
	
			ObiDistanceConstraintBatch distanceBatch = DistanceConstraints.GetFirstBatch();
			float[] forces = new float[distanceBatch.ConstraintCount];
			Oni.GetBatchConstraintForces(distanceBatch.OniBatch,forces,distanceBatch.ConstraintCount,0);	
	
			List<int> tearedEdges = new List<int>();
			for (int i = 0; i < forces.Length; i++){
	
				float p1Resistance = tearResistance[distanceBatch.springIndices[i*2]];
				float p2Resistance = tearResistance[distanceBatch.springIndices[i*2+1]];

				// average particle resistances:
				float resistance = (p1Resistance + p2Resistance) * 0.5f * tearResistanceMultiplier;
	
				if (-forces[i] * 1000 > resistance){ // units are kilonewtons.
					tearedEdges.Add(i);
				}
			}
	
			if (tearedEdges.Count > 0){
	
				DistanceConstraints.RemoveFromSolver(null);
				BendingConstraints.RemoveFromSolver(null);
				for(int i = 0; i < tearedEdges.Count; i++)
					Tear(tearedEdges[i]);
				BendingConstraints.AddToSolver(this);
				DistanceConstraints.AddToSolver(this);
	
				// update active bending constraints:
				BendingConstraints.SetActiveConstraints();
	
				// upload active particle list to solver:
				solver.UpdateActiveParticles();
			}
			
		}

		/**
		 * Returns whether a bend constraint affects the two particles referenced by a given distance constraint:
		 */
		public bool DoesBendConstraintSpanDistanceConstraint(ObiDistanceConstraintBatch dbatch, ObiBendConstraintBatch bbatch, int d, int b){

		return (bbatch.bendingIndices[b*3+2] == dbatch.springIndices[d*2] &&
			 	bbatch.bendingIndices[b*3+1] == dbatch.springIndices[d*2+1]) ||

			   (bbatch.bendingIndices[b*3+1] == dbatch.springIndices[d*2] &&
			 	bbatch.bendingIndices[b*3+2] == dbatch.springIndices[d*2+1]) ||

			   (bbatch.bendingIndices[b*3+2] == dbatch.springIndices[d*2] &&
			 	bbatch.bendingIndices[b*3] == dbatch.springIndices[d*2+1]) ||

			   (bbatch.bendingIndices[b*3] == dbatch.springIndices[d*2] &&
			 	bbatch.bendingIndices[b*3+2] == dbatch.springIndices[d*2+1]);
		}	

		public void Tear(int constraintIndex){

			// don't allow splitting if there are no free particles left in the pool.
			if (usedParticles >= totalParticles) return;
	
			// get involved constraint batches: 
			ObiDistanceConstraintBatch distanceBatch = (ObiDistanceConstraintBatch)DistanceConstraints.GetFirstBatch();
			ObiBendConstraintBatch bendingBatch = (ObiBendConstraintBatch)BendingConstraints.GetFirstBatch();
	
			// get particle indices at both ends of the constraint:
			int splitIndex = distanceBatch.springIndices[constraintIndex*2];
			int intactIndex = distanceBatch.springIndices[constraintIndex*2+1];

			// see if the rope is continuous at the split index and the intact index:
			bool continuousAtSplit = (constraintIndex < distanceBatch.ConstraintCount-1 && distanceBatch.springIndices[(constraintIndex+1)*2] == splitIndex) || 
									 (constraintIndex > 0 && distanceBatch.springIndices[(constraintIndex-1)*2+1] == splitIndex);

			bool continuousAtIntact = (constraintIndex < distanceBatch.ConstraintCount-1 && distanceBatch.springIndices[(constraintIndex+1)*2] == intactIndex) || 
									  (constraintIndex > 0 && distanceBatch.springIndices[(constraintIndex-1)*2+1] == intactIndex);
	
			// we will split the particle with higher mass, so swap them if needed (and possible). Also make sure that the rope hasnt been cut there yet:
			if ((invMasses[splitIndex] > invMasses[intactIndex] || invMasses[splitIndex] == 0) &&
				continuousAtIntact){

				int aux = splitIndex;
				splitIndex = intactIndex;
				intactIndex = aux;

			} 

			// see if we are able to proceed with the cut:
			if (invMasses[splitIndex] == 0 || !continuousAtSplit){	
				return;
			}

			// halve the mass of the teared particle:
			invMasses[splitIndex] *= 2;

			// copy the new particle data in the actor and solver arrays:
			positions[usedParticles] = positions[splitIndex];
			velocities[usedParticles] = velocities[splitIndex];
			active[usedParticles] = active[splitIndex];
			invMasses[usedParticles] = invMasses[splitIndex];
			solidRadii[usedParticles] = solidRadii[splitIndex];
			principalRadii[usedParticles] = principalRadii[splitIndex];
			phases[usedParticles] = phases[splitIndex];
	
			if (colors != null && colors.Length > 0)
				colors[usedParticles] = colors[splitIndex];
			tearResistance[usedParticles] = tearResistance[splitIndex];
			restPositions[usedParticles] = positions[splitIndex];
			restPositions[usedParticles][3] = 0; // activate rest position.
			
			// update solver particle data:
			Vector4[] velocity = {Vector4.zero};
			Oni.GetParticleVelocities(solver.OniSolver,velocity,1,particleIndices[splitIndex]);
			Oni.SetParticleVelocities(solver.OniSolver,velocity,1,particleIndices[usedParticles]);
	
			Vector4[] position = {Vector4.zero};
			Oni.GetParticlePositions(solver.OniSolver,position,1,particleIndices[splitIndex]);
			Oni.SetParticlePositions(solver.OniSolver,position,1,particleIndices[usedParticles]);
			
			Oni.SetParticleInverseMasses(solver.OniSolver,new float[]{invMasses[splitIndex]},1,particleIndices[usedParticles]);
			Oni.SetParticleSolidRadii(solver.OniSolver,new float[]{solidRadii[splitIndex]},1,particleIndices[usedParticles]);
			Oni.SetParticlePrincipalRadii(solver.OniSolver,new Vector2[]{principalRadii[splitIndex]},1,particleIndices[usedParticles]);
			Oni.SetParticlePhases(solver.OniSolver,new int[]{phases[splitIndex]},1,particleIndices[usedParticles]);

			// Update bending constraints:
			for (int i = 0 ; i < bendingBatch.ConstraintCount; ++i){

				// disable the bending constraint centered at the split particle:
				if (bendingBatch.bendingIndices[i*3+2] == splitIndex)
					bendingBatch.DeactivateConstraint(i);

				// update the one that bridges the cut:
				else if (!DoesBendConstraintSpanDistanceConstraint(distanceBatch,bendingBatch,constraintIndex,i)){

					// if the bend constraint does not involve the split distance constraint, 
					// update the end that references the split vertex:
					if (bendingBatch.bendingIndices[i*3] == splitIndex)
						bendingBatch.bendingIndices[i*3] = usedParticles;
					else if (bendingBatch.bendingIndices[i*3+1] == splitIndex)
						bendingBatch.bendingIndices[i*3+1] = usedParticles;

				}
			}

			// Update distance constraints at both ends of the cut:
			if (constraintIndex < distanceBatch.ConstraintCount-1){
				if (distanceBatch.springIndices[(constraintIndex+1)*2] == splitIndex)
					distanceBatch.springIndices[(constraintIndex+1)*2] = usedParticles;
				if (distanceBatch.springIndices[(constraintIndex+1)*2+1] == splitIndex)
					distanceBatch.springIndices[(constraintIndex+1)*2+1] = usedParticles;
			}	

			if (constraintIndex > 0){
				if (distanceBatch.springIndices[(constraintIndex-1)*2] == splitIndex)
					distanceBatch.springIndices[(constraintIndex-1)*2] = usedParticles;
				if (distanceBatch.springIndices[(constraintIndex-1)*2+1] == splitIndex)
					distanceBatch.springIndices[(constraintIndex-1)*2+1] = usedParticles;
			}

			usedParticles++;
			pooledParticles--;

		}

		/**
		 * Automatically generates tether constraints for the cloth.
		 * Partitions fixed particles into "islands", then generates up to maxTethers constraints for each 
		 * particle, linking it to the closest point in each island.
		 */
		public override bool GenerateTethers(TetherType type){
			
			if (!Initialized) return false;
	
			TetherConstraints.Clear();
			
			if (type == TetherType.Hierarchical)
				GenerateHierarchicalTethers(5);
			else
				GenerateFixedTethers(2);
	        
	        return true;
	        
		}

		private void GenerateFixedTethers(int maxTethers){

			ObiTetherConstraintBatch tetherBatch = new ObiTetherConstraintBatch(true,false,MIN_YOUNG_MODULUS,MAX_YOUNG_MODULUS);
			TetherConstraints.AddBatch(tetherBatch);
			
			List<HashSet<int>> islands = new List<HashSet<int>>();
		
			// Partition fixed particles into islands:
			for (int i = 0; i < usedParticles; i++){

				if (invMasses[i] > 0 || !active[i]) continue;
				
				int assignedIsland = -1;
	
				// keep a list of islands to merge with ours:
				List<int> mergeableIslands = new List<int>();
					
				// See if any of our neighbors is part of an island:
				int prev = Mathf.Max(i-1,0);
				int next = Mathf.Min(i+1,usedParticles-1);
		
				for(int k = 0; k < islands.Count; ++k){

					if ((active[prev] && islands[k].Contains(prev)) || 
						(active[next] && islands[k].Contains(next))){

						// if we are not in an island yet, pick this one:
						if (assignedIsland < 0){
							assignedIsland = k;
                            islands[k].Add(i);
						}
						// if we already are in an island, we will merge this newfound island with ours:
						else if (assignedIsland != k && !mergeableIslands.Contains(k)){
							mergeableIslands.Add(k);
						}
					}
                }
				
				// merge islands with the assigned one:
				foreach(int merge in mergeableIslands){
					islands[assignedIsland].UnionWith(islands[merge]);
				}
	
				// remove merged islands:
				mergeableIslands.Sort();
				mergeableIslands.Reverse();
				foreach(int merge in mergeableIslands){
					islands.RemoveAt(merge);
				}
				
				// If no adjacent particle is in an island, create a new one:
				if (assignedIsland < 0){
					islands.Add(new HashSet<int>(){i});
				}
			}	
			
			// Generate tether constraints:
			for (int i = 0; i < usedParticles; ++i){
			
				if (invMasses[i] == 0) continue;
				
				List<KeyValuePair<float,int>> tethers = new List<KeyValuePair<float,int>>(islands.Count);
				
				// Find the closest particle in each island, and add it to tethers.
				foreach(HashSet<int> island in islands){
					int closest = -1;
					float minDistance = Mathf.Infinity;
					foreach (int j in island){

						// TODO: Use linear distance along the rope in a more efficient way. precalculate it on generation!
						int min = Mathf.Min(i,j);
						int max = Mathf.Max(i,j);
						float distance = 0;
						for (int k = min; k < max; ++k)
							distance += Vector3.Distance(positions[k],
														 positions[k+1]);

						if (distance < minDistance){
							minDistance = distance;
							closest = j;
						}
					}
					if (closest >= 0)
						tethers.Add(new KeyValuePair<float,int>(minDistance, closest));
				}
				
				// Sort tether indices by distance:
				tethers.Sort(
				delegate(KeyValuePair<float,int> x, KeyValuePair<float,int> y)
				{
					return x.Key.CompareTo(y.Key);
				}
				);
				
				// Create constraints for "maxTethers" closest anchor particles:
				for (int k = 0; k < Mathf.Min(maxTethers,tethers.Count); ++k){
					tetherBatch.AddConstraint(i,tethers[k].Value,tethers[k].Key,1,1);
				}
			}

			tetherBatch.Cook();
		}

		private void GenerateHierarchicalTethers(int maxLevels){

			ObiTetherConstraintBatch tetherBatch = new ObiTetherConstraintBatch(true,false,MIN_YOUNG_MODULUS,MAX_YOUNG_MODULUS);
			TetherConstraints.AddBatch(tetherBatch);

			// for each level:
			for (int i = 1; i <= maxLevels; ++i){

				int stride = i*2;

				// for each particle:
				for (int j = 0; j < usedParticles - stride; ++j){

					int nextParticle = j + stride;

					tetherBatch.AddConstraint(j,nextParticle % usedParticles,interParticleDistance * stride,1,1);	

				}	
			}

			tetherBatch.Cook();
		
		}
		
	}
}



