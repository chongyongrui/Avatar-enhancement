using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    CharacterController controller;
    Vector2 movement;
    public float movementspeed;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();

    }

    // Update is called once per frame
    void Update()
    {   var horizontalinput = Input.GetAxis("Horizontal");
        var verticalinput = Input.GetAxis("Vertical");
        
        Vector3 direction = new Vector3(movement.x, 0, movement.y);
        movement = new Vector2(horizontalinput,verticalinput);
        if(controller.isGrounded){
            
            if(Input.GetButtonDown("Jump")){
                movement.y=5;
            }
        }
        if (direction.magnitude >= 0.1f)
        {
            controller.Move(direction *movementspeed*Time.fixedDeltaTime);
        }
    }
}
