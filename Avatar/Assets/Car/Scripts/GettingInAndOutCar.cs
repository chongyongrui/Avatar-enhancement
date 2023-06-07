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
    public Transform carCameraPosition;

    private GameObject interactingPlayer;
    private ThirdPersonController interactingPlayerController;
    private bool isInsideCar = false;
    private Transform previousParent;

    private CinemachineVirtualCamera playerCamera;
    [SerializeField] GameObject humanModel;

    public override void OnNetworkSpawn ()
    {
        if (IsOwner)
        {//"?." checks if variable is null before referencing
            interactingPlayer = GameObject.FindGameObjectWithTag("Player");
            interactingPlayerController = interactingPlayer?.GetComponent<ThirdPersonController>();
            carController.enabled = false;

            if (playerCamera == null)
            {
                playerCamera = GameObject.FindGameObjectWithTag("CarCamera").GetComponent<CinemachineVirtualCamera>();
                playerCamera.gameObject.SetActive(false);
                playerCamera.Follow = transform.GetChild(0).transform;
                //playerCamera.LookAt = transform;
                
                humanModel.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

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
            {
                Rigidbody carRigidbody = carController.GetComponent<Rigidbody>();
                carRigidbody.velocity = Vector3.zero;
                GetOutOfCarServerRPC();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
private void GetInCarServerRPC()
{
    if (carController == null)
    {
        return;
    }
    if (interactingPlayer == null)
    {
        return;
    }
humanModel.SetActive(true);
    interactingPlayerController.ThirdPersonCam.gameObject.SetActive(false);
    interactingPlayerController.FirstPersonCam.gameObject.SetActive(false);
    isInsideCar = true;
    interactingPlayerController.enabled = false;
    carController.enabled = true;

    previousParent = interactingPlayer.transform.parent;
    interactingPlayer.transform.SetParent(transform);
    interactingPlayer.transform.localPosition = Vector3.zero;
    interactingPlayer.transform.localRotation = Quaternion.identity;
    interactingPlayer.SetActive(false);

    // Adjust the player's camera follow settings to face the front of the car
    playerCamera.gameObject.SetActive(true);
}

    [ServerRpc(RequireOwnership = false)]
    private void GetOutOfCarServerRPC()
    {
        if (!isInsideCar)
            return;

        if (interactingPlayerController == null)
            return;

        isInsideCar = false;
        humanModel.SetActive(false);
        interactingPlayerController.enabled = true;
        interactingPlayerController.ThirdPersonCam.gameObject.SetActive(true);
        carController.carSpeed = 0f;
        carController.enabled = false;

        interactingPlayer.transform.SetParent(previousParent);
        interactingPlayer.transform.position = transform.position + transform.right * 4f;
        interactingPlayer.SetActive(true);

        playerCamera.gameObject.SetActive(false);

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
