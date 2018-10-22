using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtttachToPlayer : MonoBehaviour
{
    public Transform player;

    // Use this for initialization
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if(player != null)
        {
            
            transform.position = player.position;
        }
    }
}
