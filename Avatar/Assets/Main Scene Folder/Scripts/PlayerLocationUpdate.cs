using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerLocationUpdate : MonoBehaviour
{
    public int playerID = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
     int xpos = (int)Math.Ceiling(transform.position.x);
     int ypos = (int)Math.Ceiling(transform.position.y);
     int zpos = (int)Math.Ceiling(transform.position.z);
     DatabaseScript.instance.UpdatePlayerLocation(playerID, xpos, ypos, zpos);
    }
}
