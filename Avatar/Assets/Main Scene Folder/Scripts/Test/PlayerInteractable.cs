using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerInteractable : MonoBehaviour
{
   public float interactionRadius = 2f; // The radius for detecting nearby objects
    public KeyCode interactionKey = KeyCode.E; // The key to trigger the interaction
    public Transform handIKTarget; // The IK target for the character's hand
    private Animator animator; // Reference to the animator component
    
    private RigBuilder rb;
 
    private void Start(){
        animator = GetComponent<Animator>();
           rb = GetComponentInChildren<RigBuilder>();
        rb.enabled = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown(interactionKey))
        {   
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRadius);

            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Interactable"))
                {   rb.enabled = true;
                    InteractableObject interactableObject = collider.GetComponent<InteractableObject>();
                    if (interactableObject != null && interactableObject.CanBeInteracted)
                    {
                        interactableObject.Interact(handIKTarget, animator);
                        break; // Interact with only one object at a time (remove this line if you want to interact with multiple objects simultaneously)
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
