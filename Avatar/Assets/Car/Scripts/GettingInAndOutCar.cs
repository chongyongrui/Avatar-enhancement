using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using Unity.Netcode;
using Cinemachine;
public class GettingInAndOutCar : NetworkBehaviour
{
    public ModelSpawner modelSpawner;
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public PrometeoCarController carController;
    public Transform carCameraPosition; // Reference to the camera position inside the car

    private GameObject interactingPlayer;
    private ThirdPersonController interactingPlayerController;
    private bool isInsideCar = false;
    private Transform previousParent; // Store the previous parent of the player for camera adjustment
    private CinemachineVirtualCamera playerCamera; // Reference to the player's Cinemachine virtual camera

    public void Start()
    {
        if (IsOwner)
        {
            interactingPlayer = GameObject.FindGameObjectWithTag("Player");
            interactingPlayerController = interactingPlayer?.GetComponent<ThirdPersonController>();
            carController.enabled = false;
            if (playerCamera == null)
            {
                // Get the Cinemachine virtual camera component from the player
                playerCamera = GameObject.FindGameObjectWithTag("CarCamera").GetComponent<CinemachineVirtualCamera>();
                playerCamera.Follow = transform.GetChild(0).transform;
                playerCamera.LookAt = transform;
                playerCamera.gameObject.SetActive(false);

            }
        }
    }

    private void Update()
    {
        if (!isInsideCar)
        {
            if (interactingPlayer != null && Vector3.Distance(interactingPlayer.transform.position, transform.position) <= interactionDistance)
            {
                if (Input.GetKeyDown(interactKey))
                {
                    playerCamera.gameObject.SetActive(true);
                    GetInCarServerRPC();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(interactKey))
            { // Stop the car from moving
                Rigidbody carRigidbody = carController.GetComponent<Rigidbody>();
                carRigidbody.velocity = Vector3.zero;
                GetOutOfCarServerRPC();
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void GetInCarServerRPC()
    {
        interactingPlayerController.ThirdPersonCam.gameObject.SetActive(false);
        interactingPlayerController.FirstPersonCam.gameObject.SetActive(false);
        isInsideCar = true;
        interactingPlayerController.enabled = false;
        carController.enabled = true;

        // Store the previous parent of the player
        previousParent = interactingPlayer.transform.parent;

        // Set the player's parent to the car
        interactingPlayer.transform.SetParent(transform);
        interactingPlayer.transform.localPosition = Vector3.zero;
        interactingPlayer.transform.localRotation = Quaternion.identity;
        interactingPlayer.SetActive(false);

        // Adjust the player's camera follow settings to face the front of the car


    }
    [ServerRpc(RequireOwnership = false)]
    private void GetOutOfCarServerRPC()
    {
        isInsideCar = false;
        interactingPlayerController.enabled = true;
        interactingPlayerController.ThirdPersonCam.gameObject.SetActive(true);
        // interactingPlayerController.FirstPersonCam.gameObject.SetActive(true);
        carController.carSpeed = 0f;
        carController.enabled = false;
        // Reset the player's parent to the previous parent
        interactingPlayer.transform.SetParent(previousParent);

        // Reset the player's camera follow settings to default
        //    playerCamera.LookAt = interactingPlayer.transform;
        //  playerCamera.Follow = interactingPlayer.transform;
        playerCamera.gameObject.SetActive(false);
        // interactingPlayer.transform.position = transform.position + transform.forward * 2f;
        interactingPlayer.SetActive(true);

        // Stop the car from moving
        Rigidbody carRigidbody = carController.GetComponent<Rigidbody>();
        carRigidbody.velocity = Vector3.zero;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (interactingPlayer == null && other.CompareTag("Player"))
        {
            interactingPlayer = other.gameObject;
            interactingPlayerController = interactingPlayer.GetComponent<ThirdPersonController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (interactingPlayer != null && other.gameObject == interactingPlayer)
        {
            interactingPlayer = null;
            interactingPlayerController = null;
        }
    }
    [ServerRpc]
    private void TestServerRpc()
    {
        Debug.Log("The owner id is " + OwnerClientId);
    }
}
