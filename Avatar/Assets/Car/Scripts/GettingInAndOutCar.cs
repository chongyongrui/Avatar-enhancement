using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
public class GettingInAndOutCar : MonoBehaviour
{ public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public  PrometeoCarController carController;

    private GameObject interactingPlayer;
    private ThirdPersonController interactingPlayerController;
    private bool isInsideCar = false;

    private void Update()
    {
        if (!isInsideCar)
        {
            if (interactingPlayer != null && Vector3.Distance(interactingPlayer.transform.position, transform.position) <= interactionDistance)
            {
                if (Input.GetKeyDown(interactKey))
                {
                    GetInCar();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(interactKey))
            {
                GetOutOfCar();
            }
        }
    }

    private void GetInCar()
    {
        isInsideCar = true;
        interactingPlayerController.enabled = false;
        carController.enabled = true;
        interactingPlayer.transform.SetParent(transform);
        interactingPlayer.transform.localPosition = Vector3.zero;
        interactingPlayer.transform.localRotation = Quaternion.identity;
    }

    private void GetOutOfCar()
    {
        isInsideCar = false;
        interactingPlayerController.enabled = true;
        carController.enabled = false;
        interactingPlayer.transform.SetParent(null);
        interactingPlayer.transform.position = transform.position + transform.forward * 2f;
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
}
