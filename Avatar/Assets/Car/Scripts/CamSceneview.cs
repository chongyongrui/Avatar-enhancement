using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using Unity.Netcode;
public class CamSceneview : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject sceneviewcam;
    [SerializeField] private GameObject thirdpersoncam;
    [SerializeField] private GameObject mainCamera;
    GameObject player;
    ThirdPersonController controller;

    void Start()
    {
        sceneviewcam.SetActive(false);
        thirdpersoncam = GameObject.FindGameObjectWithTag("PlayerFollowCamera");



    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        player = GameObject.FindGameObjectWithTag("Player");
        controller = player.GetComponent<ThirdPersonController>();
    }
    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.H)))
        {
            if (sceneviewcam.activeSelf == true)
            {
                sceneviewcam.SetActive(false);
                mainCamera.SetActive(true);
                controller.gameObject.SetActive(true);
                // thirdpersoncam.SetActive(true);
                //  firstpersoncam.SetActive(true);
            }
            else
            {
                sceneviewcam.SetActive(true);
                mainCamera.SetActive(false);
                controller.gameObject.SetActive(false);
                // thirdpersoncam.SetActive(false);
                //  firstpersoncam.SetActive(false);

            }

        }
    }




}
