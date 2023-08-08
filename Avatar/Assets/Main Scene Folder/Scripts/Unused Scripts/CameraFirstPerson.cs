using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
using System.Net;
using System;

public class CameraFirstPerson : MonoBehaviour
{
    [SerializeField] private float sensitivity = 50f;
    
    
    private float xRotation = 0f;
    private float yRotation = 0f;
    public float defaultFOV = 56f;
    public float zoomedFOV = 30f;
    public bool isZoomed = false;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            isZoomed = !isZoomed;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += mouseX;
        yRotation = Mathf.Clamp(90f, yRotation, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        //playerBody.Rotate(Vector3.up * mouseX);
        Camera cam = this.GetComponent<Camera>();
        if (isZoomed)
        {
            cam.fieldOfView = zoomedFOV;
        }
        else
        {
            cam.fieldOfView = defaultFOV;
        }

    }


}
