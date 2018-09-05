using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderOcclusion : MonoBehaviour {
  Camera Camera;
  public Camera TempCam;
  public Shader SSAO;
  public Material BlendMat;

	// Use this for initialization
	void Start () {
    Camera = GetComponent<Camera>();
	}

  // Update is called once per frame
  private void OnRenderImage(RenderTexture source, RenderTexture destination)
  {
    TempCam.gameObject.transform.position = Camera.gameObject.transform.position;
    TempCam.gameObject.transform.rotation = Camera.gameObject.transform.rotation;
    TempCam.fieldOfView = Camera.fieldOfView;
    RenderTexture TempRT = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGB32);

    TempRT.Create();
    TempCam.targetTexture = TempRT;
    TempCam.Render();
 
    BlendMat.SetTexture("_SecondTex", source);

    Graphics.Blit(TempRT, destination, BlendMat);

    TempRT.Release();
  }
}
