using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class PSMeshRendererUpdater : MonoBehaviour
{
    public GameObject MeshObject;
    public Color Color = Color.black;
    const string materialName = "MeshEffect";
    List<Material[]> rendererMaterials = new List<Material[]>();
    List<Material[]> skinnedMaterials = new List<Material[]>();
    public bool IsActive = true;
    public float FadeTime = 1.5f;

    bool currentActiveStatus;
    private bool needUpdateAlpha;

    Color oldColor = Color.black;
    private float currentAlphaTime;

    void Update()
    {
        if (Application.isPlaying) CheckFading();
       

        if (Color != oldColor)
        {
            oldColor = Color;
            UpdateColor(Color);
        }
    }

    public void CheckFading()
    {
        if (currentActiveStatus != IsActive)
        {
            currentActiveStatus = IsActive;
            needUpdateAlpha = true;

            var currentParticles = GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in currentParticles)
            {
                if (currentActiveStatus) ps.Play();
                else
                {
                    ps.Clear();
                    ps.Stop();
                }
            }

            var currentTrails = GetComponentsInChildren<ME_TrailRendererNoise>();
            foreach (var trail in currentTrails)
            {
                trail.IsActive = currentActiveStatus;
            }
           
        }

        if (needUpdateAlpha)
        {
            if (currentActiveStatus) currentAlphaTime += Time.deltaTime;
            else currentAlphaTime -= Time.deltaTime;

            if (currentAlphaTime < 0 || currentAlphaTime > FadeTime)
            {
                needUpdateAlpha = false;
            }

            SetAlpha(Mathf.Clamp01(currentAlphaTime / FadeTime));

        }
        
    }

    public void SetAlpha(float alpha)
    {
        if (MeshObject == null) return;

        var currentLight = MeshObject.GetComponentInChildren<Light>();
        if (currentLight != null) currentLight.intensity = alpha;

        var meshRend = MeshObject.GetComponentInChildren<MeshRenderer>();
        if (meshRend != null)
        {
            var materials = meshRend.materials;
            foreach (var mat in materials)
            {
                if (mat.name.Contains(materialName))
                {
                    UpdateAlphaByPropertyName(mat, "_TintColor", alpha);
                    UpdateAlphaByPropertyName(mat, "_MainColor", alpha);
                }
            }
        }


        var skinnedMeshRend = MeshObject.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRend != null)
        {
            var materials = skinnedMeshRend.materials;
            foreach (var mat in materials)
            {
                if (mat.name.Contains(materialName))
                {
                    UpdateAlphaByPropertyName(mat, "_TintColor", alpha);
                    UpdateAlphaByPropertyName(mat, "_MainColor", alpha);
                }
            }
        }
    }

    void UpdateAlphaByPropertyName(Material mat, string name, float alpha)
    {
        if (mat.HasProperty(name))
        {
            var color = mat.GetColor(name);
            color.a = alpha;
            mat.SetColor(name, color);
        }
    }

    public void UpdateColor(Color color)
    {
        if (MeshObject == null) return;
        var hsv = ME_ColorHelper.ColorToHSV(color);
        ME_ColorHelper.ChangeObjectColorByHUE(MeshObject, hsv.H);
    }

    public void UpdateColor(float HUE)
    {
        if (MeshObject == null) return;
        ME_ColorHelper.ChangeObjectColorByHUE(MeshObject, HUE);
    }

    public void UpdateMeshEffect()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = new Quaternion();
        rendererMaterials.Clear();
        skinnedMaterials.Clear();
        if (MeshObject == null) return;
        UpdatePSMesh(MeshObject);
        AddMaterialToMesh(MeshObject);
    }

    void CheckScaleIncludedParticles()
    {
        
    }

    public void UpdateMeshEffect(GameObject go)
    {
        rendererMaterials.Clear();
        skinnedMaterials.Clear();
        if (go == null)
        {
            Debug.Log("You need set a gameObject");
            return;
        }
        MeshObject = go;
        UpdatePSMesh(MeshObject);
        AddMaterialToMesh(MeshObject);
    }

    private void UpdatePSMesh(GameObject go)
    {
        var ps = GetComponentsInChildren<ParticleSystem>();
        var meshRend = go.GetComponentInChildren<MeshRenderer>();
        var skinMeshRend = go.GetComponentInChildren<SkinnedMeshRenderer>();
        var lights = GetComponentsInChildren<Light>();

        float realBound = 1;
        float transformMax = 1;
        if (meshRend != null)
            realBound = meshRend.bounds.size.magnitude;
        if(skinMeshRend !=null)
            realBound = skinMeshRend.bounds.size.magnitude;

        transformMax = go.transform.lossyScale.magnitude;
       
        foreach (var particleSys in ps)
        {
            particleSys.transform.gameObject.SetActive(false);
            var sh = particleSys.shape;
            if (sh.enabled)
            {
                if (meshRend != null)
                {
                    sh.shapeType = ParticleSystemShapeType.MeshRenderer;
                    sh.meshRenderer = meshRend;
                    
                }
                if (skinMeshRend != null)
                {
                    sh.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                    sh.skinnedMeshRenderer = skinMeshRend;
                   
                }
            }
            var mainPS = particleSys.main;
            
            mainPS.startSizeMultiplier *= realBound / transformMax;
            particleSys.transform.gameObject.SetActive(true);
        }
        if (meshRend != null) foreach (var light1 in lights) light1.transform.position = meshRend.bounds.center;
        if (skinMeshRend != null) foreach (var light1 in lights) light1.transform.position = skinMeshRend.bounds.center;

    }

    private void AddMaterialToMesh(GameObject go)
    {
        var meshMatEffect = GetComponentInChildren<ME_MeshMaterialEffect>();
        if (meshMatEffect == null) return;

        var meshRenderer = go.GetComponentInChildren<MeshRenderer>();
        var skinMeshRenderer = go.GetComponentInChildren<SkinnedMeshRenderer>();

        // foreach (var meshRenderer in meshRenderers)
        if (meshRenderer != null)
        {
            rendererMaterials.Add(meshRenderer.sharedMaterials);
            meshRenderer.sharedMaterials = AddToSharedMaterial(meshRenderer.sharedMaterials, meshMatEffect);
        }

       // foreach (var skinMeshRenderer in skinMeshRenderers)
        if(skinMeshRenderer!=null)
        {
            skinnedMaterials.Add(skinMeshRenderer.sharedMaterials);
            skinMeshRenderer.sharedMaterials = AddToSharedMaterial(skinMeshRenderer.sharedMaterials, meshMatEffect);
        }
    }

    Material[] AddToSharedMaterial(Material[] sharedMaterials, ME_MeshMaterialEffect meshMatEffect)
    {
        if (meshMatEffect.IsFirstMaterial) return new[] { meshMatEffect.Material };
        var materials = sharedMaterials.ToList();
        for (int i = 0; i < materials.Count; i++)
        {
            if (materials[i].name.Contains(materialName)) materials.RemoveAt(i);
        }
        //meshMatEffect.Material.name = meshMatEffect.Material.name + materialName;
        materials.Add(meshMatEffect.Material);
        return materials.ToArray();
    }

    void OnDestroy()
    {
        //Activation(true);
        if (MeshObject == null) return;
        var meshRenderers = MeshObject.GetComponentsInChildren<MeshRenderer>();
        var skinMeshRenderers = MeshObject.GetComponentsInChildren<SkinnedMeshRenderer>();

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (rendererMaterials.Count == meshRenderers.Length)
                meshRenderers[i].sharedMaterials = rendererMaterials[i];

            var materials = meshRenderers[i].sharedMaterials.ToList();
            for (int j = 0; j < materials.Count; j++)
            {
                if (materials[j].name.Contains(materialName)) materials.RemoveAt(j);
            }
            meshRenderers[i].sharedMaterials = materials.ToArray();

        }

        for (int i = 0; i < skinMeshRenderers.Length; i++)
        {
            if (skinnedMaterials.Count == skinMeshRenderers.Length)
                skinMeshRenderers[i].sharedMaterials = skinnedMaterials[i];

            var materials = skinMeshRenderers[i].sharedMaterials.ToList();
            for (int j = 0; j < materials.Count; j++)
            {
                if (materials[j].name.Contains(materialName)) materials.RemoveAt(j);
            }
            skinMeshRenderers[i].sharedMaterials = materials.ToArray();

        }
        rendererMaterials.Clear();
        skinnedMaterials.Clear();
    }
}