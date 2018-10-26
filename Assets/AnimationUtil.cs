using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Group of static methods available for animation use
public class AnimationUtil : MonoBehaviour {

    public static float EaseInOutQuad(float t, float b, float c, float d)
    {
        t /= d / 2;
        if (t < 1) return (c / 2 * t * t + b);
        t--;
        return -c / 2 * (t * (t - 2) - 1) + b;
    }

    public static float EaseInQuad(float t, float b, float c, float d)
    {
        t /= d;
        return c * t * t + b;
    }

    public static float EaseOutQuad(float t, float b, float c, float d)
    {
        t /= d;
        return -c * t * (t - 2) + b;
    }
    

}
