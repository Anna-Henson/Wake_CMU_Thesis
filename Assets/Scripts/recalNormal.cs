using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class recalNormal : MonoBehaviour {

	// Use this for initialization
	void Start () {
    
	}
	
	// Update is called once per frame
	void Update () {
    Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
    mesh.RecalculateNormals();
  }
}
