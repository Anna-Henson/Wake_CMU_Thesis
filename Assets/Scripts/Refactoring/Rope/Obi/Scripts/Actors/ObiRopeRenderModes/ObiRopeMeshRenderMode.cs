using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[Serializable]
	public class ObiRopeMeshRenderMode : ObiRopeRenderMode
	{
		public enum Axis{
			X = 0,
			Y = 1,
			Z = 2
		}

		[SerializeProperty("SourceMesh")]
		[SerializeField] private Mesh mesh;

		[SerializeProperty("SweepAxis")]
		[SerializeField] private Axis axis;

		public float volumeScaling = 0;
		public bool stretchWithRope = true;
		public bool spanEntireLength = true;

		[SerializeProperty("Instances")]
		[SerializeField] private int instances = 1;

		[SerializeProperty("InstanceSpacing")]
		[SerializeField] private float instanceSpacing = 1; 

		public float offset = 0;
		public Vector3 scale = Vector3.one;

		[HideInInspector][SerializeField] private float meshSizeAlongAxis = 1;

		private Vector3[] inputVertices;
		private Vector3[] inputNormals;
		private Vector4[] inputTangents;

		private Vector3[] vertices;
		private Vector3[] normals;
		private Vector4[] tangents;
		
		private int[] orderedVertices = new int[0];

		private ObiRope.CurveFrame frame;

		public Mesh SourceMesh{
			set{mesh = value; PreprocessInputMesh(); Update(null);}
			get{return mesh;}
		}

		public Axis SweepAxis{
			set{axis = value; PreprocessInputMesh(); Update(null);}
			get{return axis;}
		}

		public int Instances{
			set{instances = value; PreprocessInputMesh(); Update(null);}
			get{return instances;}
		}

		public float InstanceSpacing{
			set{instanceSpacing = value; PreprocessInputMesh(); Update(null);}
			get{return instanceSpacing;}
		}

		public override void Initialize(){
			PreprocessInputMesh(); 
			Update(null);
		}

		public override void TearDown(){
		}

		private void PreprocessInputMesh(){

			rope.ropeMesh.Clear();
			
			if (mesh == null){
				orderedVertices = new int[0];
				return;
			}

			// Clamp instance count to a positive value.
			instances = Mathf.Max(0,instances);

			// combine all mesh instances into a single mesh:
			Mesh combinedMesh = new Mesh();
			CombineInstance[] meshInstances = new CombineInstance[instances];
			Vector3 pos = Vector3.zero;

			// initial offset for the combined mesh is half the size of its bounding box in the swept axis:
			pos[(int)axis] = mesh.bounds.extents[(int)axis];

			for (int i = 0; i < instances; ++i){
				meshInstances[i].mesh = mesh;
				meshInstances[i].transform = Matrix4x4.TRS(pos,Quaternion.identity,Vector3.one);
				pos[(int)axis] = mesh.bounds.extents[(int)axis] + (i+1) * mesh.bounds.size[(int)axis] * instanceSpacing;
			}
			combinedMesh.CombineMeshes(meshInstances,true,true);

			// get combined mesh data:
			inputVertices = combinedMesh.vertices;
			inputNormals = combinedMesh.normals;
			inputTangents = combinedMesh.tangents;

			// sort vertices along curve axis:
			float[] keys = new float[inputVertices.Length];
			orderedVertices = new int[inputVertices.Length];

			for (int i = 0; i < keys.Length; ++i){
				keys[i] = inputVertices[i][(int)axis];
				orderedVertices[i] = i;
			}	
	
			Array.Sort(keys,orderedVertices);

			// Copy the combined mesh data to deform it:
			rope.ropeMesh.vertices = combinedMesh.vertices;
			rope.ropeMesh.normals = combinedMesh.normals;
			rope.ropeMesh.tangents = combinedMesh.tangents;
			rope.ropeMesh.uv = combinedMesh.uv;
			rope.ropeMesh.uv2 = combinedMesh.uv2;
			rope.ropeMesh.uv3 = combinedMesh.uv3;
			rope.ropeMesh.uv4 = combinedMesh.uv4;
			rope.ropeMesh.colors = combinedMesh.colors;
			rope.ropeMesh.triangles = combinedMesh.triangles;
			
			vertices = rope.ropeMesh.vertices;
			normals = rope.ropeMesh.normals;
			tangents = rope.ropeMesh.tangents;

			// Calculate scale along swept axis so that the mesh spans the entire lenght of the rope if required.
			meshSizeAlongAxis = combinedMesh.bounds.size[(int)axis];

			// destroy combined mesh:
			GameObject.DestroyImmediate(combinedMesh);

		}

		public override void Update(Camera camera){

			if (mesh == null || rope.ropeMesh == null)
				return;

			rope.SmoothCurvesFromParticles();

			if (rope.curves.Count == 0) 
				return;
			
			ObiList<ObiRope.CurveSection> curve = rope.curves[0];

			if (curve.Count < 2) 
				return;

			float actualToRestLengthRatio = stretchWithRope ? rope.SmoothLength/rope.RestLength : 1; 

			// squashing factor, makes mesh thinner when stretched and thicker when compresssed.
			float squashing = Mathf.Clamp(1 + volumeScaling*(1/Mathf.Max(actualToRestLengthRatio,0.01f) - 1), 0.01f,2);

			// Calculate scale along swept axis so that the mesh spans the entire lenght of the rope if required.
			Vector3 actualScale = scale;
			if (spanEntireLength)
				actualScale[(int)axis] = rope.RestLength/meshSizeAlongAxis;

			float previousVertexValue = 0;
			float meshLength = 0;
			int index = 0;
			int nextIndex = 1; 
			int prevIndex = 0;
			Vector3 nextV = curve[nextIndex].positionAndRadius - curve[index].positionAndRadius;
			Vector3 prevV = curve[index].positionAndRadius - curve[prevIndex].positionAndRadius;
			Vector3 tangent = (nextV + prevV).normalized;
			float sectionMagnitude = nextV.magnitude;

			// we will define and transport a reference frame along the curve using parallel transport method:
			if (frame == null) 			
				frame = new ObiRope.CurveFrame();
			frame.Reset();
			frame.SetTwistAndTangent(-rope.sectionTwist * rope.SmoothSections * rope.uvAnchor,tangent);

			// set frame's initial position:
			frame.position = curve[index].positionAndRadius;

			// basis matrix for deforming the mesh, also calculate column offsets based on swept axis:
			Matrix4x4 basis = new Matrix4x4();
			int xo = ((int)axis  )%3 * 4;
			int yo = ((int)axis+1)%3 * 4;
			int zo = ((int)axis+2)%3 * 4;

			basis[xo  ] = frame.tangent[0];
			basis[xo+1] = frame.tangent[1];
			basis[xo+2] = frame.tangent[2];

			basis[yo  ] = frame.normal[0];
			basis[yo+1] = frame.normal[1];
			basis[yo+2] = frame.normal[2];

			basis[zo  ] = frame.binormal[0];
			basis[zo+1] = frame.binormal[1];
			basis[zo+2] = frame.binormal[2];

			for (int i = 0; i < orderedVertices.Length; ++i){

				int vIndex = orderedVertices[i];
				float vertexValue = inputVertices[vIndex][(int)axis] * actualScale[(int)axis] + offset;

				// Calculate how much we've advanced in the sort axis since the last vertex:
				meshLength += (vertexValue - previousVertexValue) * actualToRestLengthRatio; 
				previousVertexValue = vertexValue;

				// If we have advanced to the next section of the curve:
				while (meshLength > sectionMagnitude && sectionMagnitude > Mathf.Epsilon){

					meshLength -= sectionMagnitude;
					index = Mathf.Min(index+1,curve.Count-1);
					
					// Calculate previous and next curve indices:
					nextIndex = Mathf.Min(index+1,curve.Count-1);
					prevIndex = Mathf.Max(index-1,0);

					// Calculate current tangent as the vector between previous and next curve points:
					nextV = curve[nextIndex].positionAndRadius - curve[index].positionAndRadius;
					prevV = curve[index].positionAndRadius - curve[prevIndex].positionAndRadius;
					tangent = (nextV + prevV).normalized;
					sectionMagnitude = nextV.magnitude;
		
					// Transport frame:
					frame.Transport(curve[index].positionAndRadius,tangent,rope.sectionTwist);

					// Update basis matrix:
					basis[xo  ] = frame.tangent[0];
					basis[xo+1] = frame.tangent[1];
					basis[xo+2] = frame.tangent[2];
	
					basis[yo  ] = frame.normal[0];
					basis[yo+1] = frame.normal[1];
					basis[yo+2] = frame.normal[2];
	
					basis[zo  ] = frame.binormal[0];
					basis[zo+1] = frame.binormal[1];
					basis[zo+2] = frame.binormal[2];
				}
			
				float sectionThickness = rope.thicknessFromParticles ? curve[index].positionAndRadius.w : rope.thickness;

				// calculate deformed vertex position:
				Vector3 offsetFromCurve = Vector3.Scale(inputVertices[vIndex],actualScale * sectionThickness * squashing);
				offsetFromCurve[(int)axis] = meshLength;

				vertices[vIndex] = frame.position + basis.MultiplyVector(offsetFromCurve);
				normals[vIndex] = basis.MultiplyVector(inputNormals[vIndex]);
				tangents[vIndex] = basis * inputTangents[vIndex]; // avoids expensive implicit conversion from Vector4 to Vector3.
				tangents[vIndex].w = inputTangents[vIndex].w;
			}

			CommitMeshData();

		}

		private void CommitMeshData(){
			rope.ropeMesh.vertices = vertices;
			rope.ropeMesh.normals = normals;
			rope.ropeMesh.tangents = tangents;
			rope.ropeMesh.RecalculateBounds();
		}
	}
}
