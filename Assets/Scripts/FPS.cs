using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS : MonoBehaviour
{

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 100, 100), "FPS: " + (1.0f / Time.smoothDeltaTime));
    }
}
