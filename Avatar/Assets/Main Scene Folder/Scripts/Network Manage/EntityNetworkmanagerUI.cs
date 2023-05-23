using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

using UnityEngine.UI;

public class EntityNetworkmanagerUI : Singleton<EntityNetworkmanagerUI>
{
     [SerializeField] private Button executePhysicsButton;

    private bool hasServerStarted;

    void Start()
    {
    
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            hasServerStarted = true;
        };

        executePhysicsButton.onClick.AddListener(() => 
        {
            if (!hasServerStarted)
            {
                Debug.Log("Server has not started...");
                return;
            }
            EntitySpawner.Instance.SpawnObjects();
        });
    }
}
