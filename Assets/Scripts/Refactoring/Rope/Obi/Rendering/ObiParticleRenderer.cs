using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace Obi{

[ExecuteInEditMode]
[RequireComponent(typeof(ObiActor))]
public class ObiParticleRenderer : MonoBehaviour
{
	public bool render = true;
	public Shader shader;
	public Color particleColor = Color.white; 
	public float radiusScale = 1;

	private ObiActor actor;
	private List<Mesh> meshes = new List<Mesh>();
	private Material material;

	// Geometry buffers:
	private List<Vector3> vertices = new List<Vector3>(4000);
	private List<Vector3> normals = new List<Vector3>(4000);
	private List<Color> colors = new List<Color>(4000);
	private List<int> triangles = new List<int>(6000);

	private List<Vector4> anisotropy1 = new List<Vector4>(4000);
	private List<Vector4> anisotropy2 = new List<Vector4>(4000);
	private List<Vector4> anisotropy3 = new List<Vector4>(4000);

	int particlesPerDrawcall = 0;
	int drawcallCount;
	bool subscribed = false; 	/**< Whether the renderer is subscribed to the solver or not.*/

	private Vector3[] particleOffsets = new Vector3[4]{
		new Vector3(1,1,0),
		new Vector3(-1,1,0),
		new Vector3(-1,-1,0),
		new Vector3(1,-1,0)
	};

	public IEnumerable<Mesh> ParticleMeshes{
		get { return meshes; }
	}

	public Material ParticleMaterial{
		get { return material; }
	}

	public void Awake(){
		actor = GetComponent<ObiActor>();

		// figure out the size of our drawcall arrays:
		particlesPerDrawcall = Constants.maxVertsPerMesh/4;
		drawcallCount = actor.positions.Length / particlesPerDrawcall + 1;
		particlesPerDrawcall = Mathf.Min(particlesPerDrawcall,actor.positions.Length);

		actor.OnAddedToSolver += delegate {Subscribe();};
		actor.OnRemovedFromSolver += delegate {Unsubscribe();};
	}

	private void Subscribe(){
		if (!subscribed && actor.Solver != null){
			subscribed = true;
			actor.Solver.RequireRenderablePositions();
			actor.Solver.OnFrameEnd += Actor_solver_OnFrameEnd;	
		}
	}

	private void Unsubscribe(){
		if (subscribed && actor.Solver != null){
			subscribed = false;
			actor.Solver.RelinquishRenderablePositions();
			actor.Solver.OnFrameEnd -= Actor_solver_OnFrameEnd;
		}
	}

	public void OnEnable(){
		Subscribe();
	}

	public void OnDisable(){

		Unsubscribe();

		ClearMeshes();
		GameObject.DestroyImmediate(material);
	}

	void CreateMaterialIfNeeded(){

		if (shader != null){

			if (!shader.isSupported)
				Debug.LogWarning("Particle rendering shader not suported.");
			
			if (material == null || material.shader != shader){
				GameObject.DestroyImmediate(material);
				material= new Material (shader);
	        	material.hideFlags = HideFlags.HideAndDontSave;
			}
		}
	}	
		
	void Actor_solver_OnFrameEnd (object sender, EventArgs e)
	{
		if (!isActiveAndEnabled || !actor.isActiveAndEnabled || !actor.InSolver ){
			ClearMeshes();
			return;
		}

		CreateMaterialIfNeeded();

		ObiSolver solver = actor.Solver;

		// If the amount of meshes we need to draw the particles has changed:
		if (drawcallCount != meshes.Count){

			// Re-generate meshes:
			ClearMeshes();
			for (int i = 0; i < drawcallCount; i++){
				Mesh mesh = new Mesh();
				mesh.name = "Particle imposters";
				mesh.hideFlags = HideFlags.HideAndDontSave;
				mesh.MarkDynamic();
				meshes.Add(mesh);
			}

		}

		//Convert particle data to mesh geometry:
		for (int i = 0; i < drawcallCount; i++){

			// Clear all arrays
			vertices.Clear();
			normals.Clear();
			colors.Clear();
			triangles.Clear();
			anisotropy1.Clear();
			anisotropy2.Clear();	
			anisotropy3.Clear();
			
			int index = 0;

			for(int j = i * particlesPerDrawcall; j < (i+1) * particlesPerDrawcall && j < actor.active.Length; ++j)
			{
				if (actor.active[j]){

					AddParticle(index,
								solver.renderablePositions[actor.particleIndices[j]],
								solver.anisotropies[actor.particleIndices[j]],
								(actor.colors != null && j < actor.colors.Length) ? actor.colors[j] : Color.white);
					index +=4;
				}
			}

			Apply(meshes[i]);
		}
	
		DrawParticles();
	}

	private void DrawParticles(){

		if (material != null){

			material.SetFloat("_RadiusScale",radiusScale);
			material.SetColor("_Color",particleColor);

			// Send the meshes to be drawn:
			if (render){
				foreach(Mesh mesh in meshes)
					Graphics.DrawMesh(mesh, Matrix4x4.identity, material, gameObject.layer);
			}
		}

	}

	private void Apply(Mesh mesh){
		mesh.Clear();
		mesh.SetVertices(vertices);
		mesh.SetNormals(normals);
		mesh.SetColors(colors);
		mesh.SetUVs(0,anisotropy1);
		mesh.SetUVs(1,anisotropy2);
		mesh.SetUVs(2,anisotropy3);
		mesh.SetTriangles(triangles,0,true);
	}

	private void ClearMeshes(){
		foreach(Mesh mesh in meshes)
			GameObject.DestroyImmediate(mesh);
		meshes.Clear();
	}

	private void AddParticle(int i, Vector3 position, Oni.Anisotropy anisotropy, Color color){
		
		vertices.Add(position);
		vertices.Add(position);
		vertices.Add(position);
		vertices.Add(position);

		normals.Add(particleOffsets[0]);
		normals.Add(particleOffsets[1]);
		normals.Add(particleOffsets[2]);
		normals.Add(particleOffsets[3]);
		
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
        colors.Add(color);

		anisotropy1.Add(anisotropy.b1);
		anisotropy1.Add(anisotropy.b1);
		anisotropy1.Add(anisotropy.b1);
		anisotropy1.Add(anisotropy.b1);

		anisotropy2.Add(anisotropy.b2);
		anisotropy2.Add(anisotropy.b2);
		anisotropy2.Add(anisotropy.b2);
		anisotropy2.Add(anisotropy.b2);

		anisotropy3.Add(anisotropy.b3);
		anisotropy3.Add(anisotropy.b3);
		anisotropy3.Add(anisotropy.b3);
		anisotropy3.Add(anisotropy.b3);

		triangles.Add(i+2);
		triangles.Add(i+1);
		triangles.Add(i);
		triangles.Add(i+3);
        triangles.Add(i+2);
        triangles.Add(i);
    }

}
}

