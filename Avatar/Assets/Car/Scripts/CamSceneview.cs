using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using Unity.Netcode;

public class CamSceneview : NetworkBehaviour
{
    [SerializeField] private Camera sceneviewCamera;
    [SerializeField] private GameObject thirdpersoncam;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private WaypointPlacer waypointPlacer;
    GameObject player;
    ThirdPersonController controller;
    PrometeoCarController carController;

    void Start()
    {
        sceneviewCamera.enabled = false;
        thirdpersoncam = GameObject.FindGameObjectWithTag("PlayerFollowCamera");
        waypointPlacer.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        player = GameObject.FindGameObjectWithTag("Player");
        controller = player.GetComponent<ThirdPersonController>();
    
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (sceneviewCamera.enabled)
            {
                sceneviewCamera.enabled = false;
                mainCamera.SetActive(true);
                controller.gameObject.SetActive(true);
              waypointPlacer.enabled = false;
            }
            else
            {
                sceneviewCamera.enabled = true;
                mainCamera.SetActive(false);
                controller.gameObject.SetActive(false);
                waypointPlacer.enabled = true;
            }
        }
    }
}
