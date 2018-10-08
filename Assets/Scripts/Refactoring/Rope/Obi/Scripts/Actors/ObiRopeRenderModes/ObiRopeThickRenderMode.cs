using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[Serializable]
	public class ObiRopeThickRenderMode : ObiRopeRenderMode
	{

		private List<Vector3> vertices = new List<Vector3>();
		private List<Vector3> normals = new List<Vector3>();
		private List<Vector4> tangents = new List<Vector4>();
		private List<Vector2> uvs = new List<Vector2>();
		private List<Color> vertColors = new List<Color>();
		private List<int> tris = new List<int>();

		private ObiRope.CurveFrame frame;

		public override void Initialize(){
			Update(null);
		}

		public override void TearDown(){
		}

		public override void Update(Camera camera){

			if (rope.section == null || rope.ropeMesh == null) 
				return;

			rope.SmoothCurvesFromParticles();

			ClearMeshData();

			float actualToRestLengthRatio = rope.SmoothLength/rope.RestLength;

			int sectionSegments = rope.section.Segments;
			int verticesPerSection = sectionSegments + 1; 				// the last vertex in each section must be duplicated, due to uv wraparound.

			float vCoord = -rope.uvScale.y * rope.RestLength * rope.uvAnchor;	// v texture coordinate.
			int sectionIndex = 0;
			int tearCount = 0;

			// we will define and transport a reference frame along the curve using parallel transport method:
			if (frame == null) 			
				frame = new ObiRope.CurveFrame();
			frame.Reset();
			frame.SetTwist(-rope.sectionTwist * rope.SmoothSections * rope.uvAnchor);

			// for closed curves, last frame of the last curve must be equal to first frame of first curve.
			Vector3 firstTangent = Vector3.forward;

			Vector4 texTangent = Vector4.zero;
			Vector2 uv = Vector2.zero;

			for (int c = 0; c < rope.curves.Count; ++c){
				
				ObiList<ObiRope.CurveSection> curve = rope.curves[c];

				// Reinitialize frame for each curve.
				frame.Reset();

				for (int i = 0; i < curve.Count; ++i){
	
					// Calculate previous and next curve indices:
					int nextIndex = Mathf.Min(i+1,curve.Count-1);
					int prevIndex = Mathf.Max(i-1,0);
	
					// Calculate current tangent as the vector between previous and next curve points:
					Vector3 nextV;

					// The next tangent of the last segment of the last curve in a closed rope, is the first tangent again:
					if (rope.Closed && c == rope.curves.Count-1 && i == curve.Count-1 )
						nextV = firstTangent;
					else 
						nextV = curve[nextIndex].positionAndRadius - curve[i].positionAndRadius;

					Vector3 prevV = curve[i].positionAndRadius - curve[prevIndex].positionAndRadius;
					Vector3 tangent = nextV + prevV;

					// update frame:
					frame.Transport(curve[i].positionAndRadius,tangent,rope.sectionTwist);

					// update tear prefabs:
					if (rope.tearPrefabPool != null ){

						// first segment of not last first curve:
						if (tearCount < rope.tearPrefabPool.Length && c > 0 && i == 0){
							if (!rope.tearPrefabPool[tearCount].activeSelf)
								 rope.tearPrefabPool[tearCount].SetActive(true);
						
							rope.PlaceObjectAtCurveFrame(frame,rope.tearPrefabPool[tearCount],Space.Self, false);
		
							tearCount++;
						}

						// last segment of not last curve:
						if (tearCount < rope.tearPrefabPool.Length && c < rope.curves.Count-1 && i == curve.Count-1){
							if (!rope.tearPrefabPool[tearCount].activeSelf)
								 rope.tearPrefabPool[tearCount].SetActive(true);
						
							rope.PlaceObjectAtCurveFrame(frame,rope.tearPrefabPool[tearCount],Space.Self, true);
		
							tearCount++;
						}
					}

					// update start/end prefabs:
					if (c == 0 && i == 0){

						// store first tangent of the first curve (for closed ropes):
						firstTangent = tangent;

						if (rope.startPrefabInstance != null && !rope.Closed)
							rope.PlaceObjectAtCurveFrame(frame,rope.startPrefabInstance, Space.Self, false);

					}else if (c == rope.curves.Count-1 && i == curve.Count-1 && rope.endPrefabInstance != null && !rope.Closed){
							rope.PlaceObjectAtCurveFrame(frame,rope.endPrefabInstance,Space.Self, true);
					}
		
					// advance v texcoord:
					vCoord += rope.uvScale.y * (Vector3.Distance(curve[i].positionAndRadius,curve[prevIndex].positionAndRadius) / 
										       (rope.normalizeV ? rope.SmoothLength : actualToRestLengthRatio));
	
					// calculate section thickness (either constant, or particle radius based):
					float sectionThickness = (rope.thicknessFromParticles ? curve[i].positionAndRadius.w : rope.thickness) * rope.sectionThicknessScale;

					// Loop around each segment:
					for (int j = 0; j <= sectionSegments; ++j){
	
						vertices.Add(frame.position + (rope.section.vertices[j].x*frame.normal + rope.section.vertices[j].y*frame.binormal) * sectionThickness);
						normals.Add(vertices[vertices.Count-1] - frame.position);
						texTangent = -Vector3.Cross(normals[normals.Count-1],frame.tangent);
						texTangent.w = 1;
						tangents.Add(texTangent);

						vertColors.Add(curve[i].color);
						uv.Set((j/(float)sectionSegments)*rope.uvScale.x,vCoord);
						uvs.Add(uv);
	
						if (j < sectionSegments && i < curve.Count-1){

							tris.Add(sectionIndex*verticesPerSection + j); 			
							tris.Add(sectionIndex*verticesPerSection + (j+1)); 		
							tris.Add((sectionIndex+1)*verticesPerSection + j); 		
	
							tris.Add(sectionIndex*verticesPerSection + (j+1)); 		
							tris.Add((sectionIndex+1)*verticesPerSection + (j+1)); 	
							tris.Add((sectionIndex+1)*verticesPerSection + j); 		

						}
					}

					sectionIndex++;
				}

			}

			CommitMeshData();

		}

		private void ClearMeshData(){
			rope.ropeMesh.Clear();
			vertices.Clear();
			normals.Clear();
			tangents.Clear();
			uvs.Clear();
			vertColors.Clear();
			tris.Clear();
		}

		private void CommitMeshData(){
			rope.ropeMesh.SetVertices(vertices);
			rope.ropeMesh.SetNormals(normals);
			rope.ropeMesh.SetTangents(tangents);
			rope.ropeMesh.SetColors(vertColors);
			rope.ropeMesh.SetUVs(0,uvs);
			rope.ropeMesh.SetTriangles(tris,0,true);
		}
	}
}

