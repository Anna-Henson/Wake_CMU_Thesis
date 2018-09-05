﻿using System.Collections;
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
    TempCam.CopyFrom(Camera);
    TempCam.backgroundColor = Color.white;
    TempCam.clearFlags = CameraClearFlags.Color;
    TempCam.cullingMask = 1 << LayerMask.NameToLayer("Subject");
    RenderTexture TempRT = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGB32);

    TempRT.Create();
    TempCam.targetTexture = TempRT;
    TempCam.Render();
 
    BlendMat.SetTexture("_SecondTex", source);

    Graphics.Blit(TempRT, destination, BlendMat);

    TempRT.Release();
  }
}
