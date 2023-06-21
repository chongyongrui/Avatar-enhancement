using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Shooting : MonoBehaviour
{public ParticleSystem[] muzzleflash;
    public void isFiring(){
        foreach(var particle in muzzleflash){
            particle.Emit(1);
        }
    }

}
