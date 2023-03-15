using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    public float rotationSpeed;
    public Transform player;//Transform components for object ie.position of player in word space;
    public Transform playerObj;
                                         
    public Transform orientation;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

    }

    // Update is called once per frame
    void Update()
    {   //same y position of player, same x and z position of camera.
        Vector3 offset = new Vector3(transform.position.x, player.position.y, transform.position.z);

        Vector3 view = player.position - offset;
        orientation.forward = view.normalized;
        float horizontalinput = Input.GetAxis("Horizontal");// returns 1 or -1 depending on keys pressed
        float verticalinput = Input.GetAxis("Vertical");
        //calculating where the camera rotates based on player input
        Vector3 inputDirection = orientation.forward * verticalinput + orientation.right * horizontalinput;
        if (inputDirection != Vector3.zero)
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDirection.normalized, Time.fixedDeltaTime * rotationSpeed);
        }
    }
}
