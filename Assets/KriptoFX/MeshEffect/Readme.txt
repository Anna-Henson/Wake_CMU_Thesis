version 1.3.1

The package includes prefabs of mesh effects and demo scenes for pc/mobiles with characters and environment.

------------------------------------------------------------------------------------------------------------------------------------------

NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE !!!!


	1) Unity does NOT supported multiple submeshes with multiple materials to each submesh. 1 submesh = 1 material!
	You can not add new material (for example lightning material) to all submeshes!
	So, if your model have more then 2 submeshes, then material will be added only for last submesh. 
	You must use splitted meshes. There is no performance or draw calls difference. But then you can use additional materials for all meshes.



------------------------------------------------------------------------------------------------------------------------------------------

NOTE for PC:

	If you want to use posteffect for PC like in the demo:

	*) Remove "ME_Bloom.cs" from camera if you used this script before. 
	1) Download unity free posteffects 
	https://assetstore.unity.com/packages/essentials/post-processing-stack-83912
	2) Add "PostProcessingBehaviour.cs" on main Camera.
	3) Set the "PostEffectsProfile" (the path "\Assets\KriptoFX\MeshEffect\PostEffectsProfile.asset")
	4) You should turn on "HDR" on main camera for correct posteffects. 
	If you have forward rendering path (by default in Unity), you need disable antialiasing "edit->project settings->quality->antialiasing"
	or turn of "MSAA" on main camera, because HDR does not works with msaa. If you want to use HDR and MSAA then use "post effect msaa". 



------------------------------------------------------------------------------------------------------------------------------------------
NOTE for MOBILES:

	For correct work on mobiles in your project scene you need:

	1) Add script "ME_DistortionAndBloom.cs" on main camera. It's allow you to see correct distortion, soft particles and physical bloom 
	The mobile bloom posteffect work if mobiles supported HDR textures or supported openGLES 3.0
	The distortion and soft particles work on all mobiles. 

------------------------------------------------------------------------------------------------------------------------------------------

Effect USING:

In editor mode:

	1) Just drag&drop prefab to your object. 
	2) Set any object to the property "Mesh Object" of script "PSMeshRendererUpdater".
	3) Click "Update Mesh Renderer".
	Particles and materials will be added to your object. 


In runtime mode:

	var currentInstance = Instantiate(Effect) as GameObject; 
	var psUpdater = currentInstance.GetComponent<PSMeshRendererUpdater>();
	psUpdater.UpdateMeshEffect(MeshObject);


For SCALING just change transform scale of mesh.

------------------------------------------------------------------------------------------------------------------------------------------
Supported platforms:

	PC/Consoles/VR/Mobiles with directx9/11, opengles 2.0/3.0 and gamma/linear color space
	All effects tested on Oculus Rift CV1 with single and dual mode rendering and works perfect. 

------------------------------------------------------------------------------------------------------------------------------------------


If you have some questions, you can write me to email "kripto289@gmail.com" 