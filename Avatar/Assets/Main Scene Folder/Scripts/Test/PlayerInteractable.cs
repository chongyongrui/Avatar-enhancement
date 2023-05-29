using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractable : MonoBehaviour
{
   
    // Update is called once per frame
    void Update()
    {
       if(Input.GetKeyDown(KeyCode.E)) {
        float interactrange = 2.0f;
        Collider[] colliderArray = Physics.OverlapSphere(transform.position,interactrange);
        foreach(Collider collider in colliderArray){
            if(collider.TryGetComponent(out EntityInteractable entity)){
                entity.interact();
            }
        }
       }
    }
}
