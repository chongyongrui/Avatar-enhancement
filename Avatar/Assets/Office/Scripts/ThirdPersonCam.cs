using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    public float rotationSpeed;
    public Transform player;//Transform components for object ie.position of player in word space;
    public Transform playerObj;
    private Vector2 input;
    private Quaternion freeRotation;
    private Vector3 targetDirection;
    public Transform orientation;

    // Start is called before the first frame update
    void Start()
    {
     
        

    }

    // Update is called once per frame
    void Update()
    {  input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
         //same y position of player, same x and z position of camera.
        Vector3 offset = new Vector3(transform.position.x, player.position.y, transform.position.z);

        Vector3 view = player.position - offset;
        orientation.forward = view.normalized;
        float horizontalinput = Input.GetAxis("Horizontal");// returns 1 or -1 depending on keys pressed
        float verticalinput = Input.GetAxis("Vertical");
        if (verticalinput < 0)  
            verticalinput *= -1;
        if (horizontalinput < 0)
            horizontalinput *= -1;
        float move = verticalinput + horizontalinput;
         UpdateTargetDirection();
 
        if(input != Vector2.zero && targetDirection.magnitude > 0.1f)
        {
            Vector3 lookDirection = targetDirection.normalized;
            freeRotation = Quaternion.LookRotation(lookDirection, transform.up);
            var diferenceRotation = freeRotation.eulerAngles.y - transform.eulerAngles.y;
            var eulerY = transform.eulerAngles.y;
 
            if (diferenceRotation < 0 || diferenceRotation > 0)
                eulerY = freeRotation.eulerAngles.y;
            var euler = new Vector3(0, eulerY, 0);
 
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(euler), rotationSpeed * Time.deltaTime);
        }
        // //calculating where the camera rotates based on player input
        // Vector3 inputDirection = orientation.forward * verticalinput + orientation.right * horizontalinput;
        // if (inputDirection != Vector3.zero)
        // {
        //     playerObj.forward = Vector3.Slerp(playerObj.forward, inputDirection.normalized, Time.fixedDeltaTime * rotationSpeed);
        // }
    }
     public void UpdateTargetDirection()
    {
        var forward = Camera.main.transform.TransformDirection(Vector3.forward);
        forward.y = 0;
 
        var right = Camera.main.transform.TransformDirection(Vector3.right);
 
        targetDirection = input.x * right + input.y * forward;
    }
};
