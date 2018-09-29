using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlatformSwitch : MonoBehaviour
{
    public GameObject VRSet;
    public GameObject PCSet;
    public GameObject CurSet;
    private bool isVR;
    
    private void Awake()
    {
#if IS_PC
        isVR = false;
#else
        isVR = true;
#endif
        SetVR(isVR);
    }

    T ForceCopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        var dst = destination.GetComponent(type) as T;
        //if (!dst)
        dst = destination.AddComponent(type) as T;
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            if (field.IsStatic) continue;
            field.SetValue(dst, field.GetValue(original));
        }
        var props = type.GetProperties();
        foreach (var prop in props)
        {
            if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
            prop.SetValue(dst, prop.GetValue(original, null), null);
        }
        return dst as T;
    }

    public void SetVR(bool _isVR)
    {
        if (_isVR)
        {
            print("is on VR");
            if (VRSet)
                VRSet.SetActive(true);
            if (PCSet)
                PCSet.SetActive(false);
            CurSet = VRSet;
        }
        else
        {
            print("is on PC");
            if (PCSet)
                PCSet.SetActive(true);
            if (VRSet)
                VRSet.SetActive(false);
            CurSet = PCSet;
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
