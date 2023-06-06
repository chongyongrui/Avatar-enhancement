using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using StarterAssets;
public class ModelSpawner : NetworkBehaviour
{
    public GameObject modelPrefab;
    public GameObject spawnedModel;
    private List<GameObject> spawnedPrefabs = new List<GameObject>();
    [SerializeField] Vector3 spawnPosition = new Vector3(0.18f, -0.74f, 4.41f);

    [SerializeField] private LayerMask layer;   
    public ThirdPersonController playerController;
    private PrometeoCarController carController;

    private  NetworkVariable<bool> spawnedstatus = new NetworkVariable<bool>();
    private void Start()
{
    

    //carController = spawnedModel?.GetComponent<PrometeoCarController>();
    if (carController == null)
    {
        return;
    }
}
    private void Update()
    {
        if (!IsOwner) { return; }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer))
            {
                if (spawnedModel == null)
                {
                    SpawnModelServerRpc();
                }
                else
                {
                    DestroyModelServerRpc();
                }
            }
        }
    }
    [ServerRpc (RequireOwnership = true)]
    private void SpawnModelServerRpc()
    {
        Collider[] colliders = Physics.OverlapSphere(spawnPosition, 1f, layer);

        if (colliders.Length > 0)
        {
            // Collision detected, choose an alternative position or handle it as desired
            Debug.Log("Collision detected at the spawn point.");
            return;
        }
        spawnedModel = Instantiate(modelPrefab, new Vector3(0.18f, -0.74f, 4.41f), Quaternion.Euler(0f, -63f, 0f));
        spawnedPrefabs.Add(spawnedModel);
        carController = spawnedModel.GetComponent<PrometeoCarController>();
        if (carController != null)
        {
            carController.parent = this;
        }
        spawnedModel.GetComponent<PrometeoCarController>().parent = this;
        spawnedModel.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyModelServerRpc()
    {
        GameObject destroy = spawnedModel;
        destroy.GetComponent<NetworkObject>().Despawn();
        spawnedPrefabs.Remove(spawnedPrefabs[0]);
        Destroy(spawnedModel);
        spawnedModel = null;
    }
  
}
