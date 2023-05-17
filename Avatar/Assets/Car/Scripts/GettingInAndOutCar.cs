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

    private void Start()
    {
        if (IsOwner)
        {
            interactingPlayer = GameObject.FindGameObjectWithTag("Player");
            interactingPlayerController = interactingPlayer?.GetComponent<ThirdPersonController>();
            carController.enabled = false;

            // Get the Cinemachine virtual camera component from the player
            playerCamera = interactingPlayer.GetComponentInChildren<CinemachineVirtualCamera>();
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
                    GetInCarServerRPC();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(interactKey))
            {
                GetOutOfCarServerRPC();
            }
        }
    }
[ServerRpc(RequireOwnership = false)]
    private void GetInCarServerRPC()
    {isInsideCar = true;
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
        playerCamera.LookAt = transform;
        playerCamera.Follow = carCameraPosition;
    }
[ServerRpc(RequireOwnership = false)]
private void GetOutOfCarServerRPC()
{
    isInsideCar = false;
    interactingPlayerController.enabled = true;
    carController.enabled = false;
    carController.carSpeed = 0f;

    // Reset the player's parent to the previous parent
    interactingPlayer.transform.SetParent(previousParent);

    // Reset the player's camera follow settings to default
    playerCamera.LookAt = interactingPlayer.transform;
    playerCamera.Follow = interactingPlayer.transform;

    interactingPlayer.transform.position = transform.position + transform.forward * 2f;
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
        private void TestServerRpc(){
            Debug.Log("The owner id is "+ OwnerClientId);
        }
}
