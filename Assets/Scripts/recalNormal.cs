using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class recalNormal : MonoBehaviour {

  public float BackgroundSub;
  public float dScale;
  public Material material;
  private float timecounter;
	// Use this for initialization
	void Start () {
    timecounter = 0.0f;
  }
	
	// Update is called once per frame
	void FixedUpdate () {

    timecounter += Time.deltaTime;
    if (timecounter > 1.0f)
    {
      timecounter = 0.0f;
      Mesh mesh = GetComponent<MeshFilter>().sharedMesh;


      Texture2D depthTex = material.GetTexture("_DepthTex") as Texture2D;

      Texture2D prevTex = material.GetTexture("_PrevDepth") as Texture2D;
      Vector3[] vertices = mesh.vertices;
      Vector2[] uvs = mesh.uv;
      int i = 0;
      while (i < vertices.Length)
      {
        bool getRid = false;
        Vector3 vertex = vertices[i];
        Vector2 uv = uvs[i];
        float d = depthTex.GetPixelBilinear(uv.x, uv.y).r;
        float rs_planeZDist = 3.5f;

        Vector3 camera = new Vector3(0.0f, rs_planeZDist, 0.0f);

        Vector3 projectionVec = (vertex - camera).normalized;

        if (d == 0.0f)
        {
          getRid = true;
          d = prevTex.GetPixelBilinear(uv.x, uv.y).r;
          if (d == 0.0f)
          {
            float xOffset = 0;
            float yOffset = 0;
            Vector2 toCenter = new Vector2(0.5f - uv.x, 0.5f - uv.y);

            Vector2 dirVector = toCenter.normalized;
            for (int k = 1; k < 11; k++)
            {
              xOffset = dirVector.x * (k / 30.0f);
              yOffset = dirVector.y * (k / 30.0f);
              d = (depthTex.GetPixelBilinear(uv.x + xOffset, uv.y + yOffset).r);
              if (d != 0.0f && d < BackgroundSub) break;
            }
            float dd = 0.0f;
            for (int j = 1; j < 11; j++)
            {
              xOffset = dirVector.x * (j / 30.0f);
              yOffset = dirVector.y * (j / 30.0f);
              dd = (depthTex.GetPixelBilinear(uv.x - xOffset, uv.y - yOffset).r);
              if (dd != 0.0f && d < BackgroundSub) break;
            }

            if (dd != 0.0f)
            {
              if (d != 0.0f)
              {
                float[] array = new float[] { d, dd };
                d = Mathf.Min(array);
              }
              else
              {
                d = dd;
              }
            }
            if (d == 0)
            {
              d = 1.0f;
            }
          }
        }
        d *= dScale;
        vertex += d * projectionVec;
        i++;
      }
      mesh.vertices = vertices;
      mesh.RecalculateNormals();
    }
  }
}
