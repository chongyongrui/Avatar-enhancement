using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractable : MonoBehaviour
{
   public float interactionRadius = 2f; // The radius for detecting nearby objects
    public KeyCode interactionKey = KeyCode.E; // The key to trigger the interaction
    public Transform handIKTarget; // The IK target for the character's hand
    public Animator animator; // Reference to the animator component
    private void Start(){}
    private void Update()
    {
        if (Input.GetKeyDown(interactionKey))
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRadius);

            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Interactable"))
                {
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
