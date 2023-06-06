using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class InteractableObject : MonoBehaviour
{ public bool CanBeInteracted { get; private set; } = true; // Can the object be interacted with?
    public string grabTrigger = "Grabitem"; // The name of the grab animation trigger
    public void Interact(Transform handIKTarget, Animator animator)
    {
        // Perform object-specific interaction logic here
        Debug.Log("Interacting with object: " + gameObject.name);

        // Play the grab animation
       

        // Set the IK target position
       // SetIKTargetPosition(handIKTarget.position);

    handIKTarget.position = transform.position;
        // Example interaction: Parent the object to the character's hand

        
        //transform.SetParent(handIKTarget);

        // transform.localPosition = Vector3.zero;
        // transform.localRotation = Quaternion.identity;
 animator.SetTrigger(grabTrigger);
        CanBeInteracted = false; // Disable further interaction with this object
    }

    private void SetIKTargetPosition(Vector3 position)
    {
        // Set the IK target position
        AnimatorIKManager.SetIKTargetPosition(position);
    }
}
