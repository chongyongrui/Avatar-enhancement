using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class ModelSpawner : NetworkBehaviour
{public GameObject modelPrefab;
    private GameObject spawnedModel;
    private List<GameObject> spawnedPrefabs = new List<GameObject>();
    private void OnMouseDown()
    { if(!IsOwner){return;}
        if (spawnedModel == null)
        {
            SpawnModelServerRpc();
        }
        else
        {
            DestroyModelServerRpc(); 
        }
    }
    [ServerRpc]
    private void SpawnModelServerRpc()
    {
        spawnedModel = Instantiate(modelPrefab, new Vector3(0.18f, -0.74f,4.41f),  Quaternion.Euler(0f, -63f, 0f));
        spawnedPrefabs.Add(spawnedModel);
        spawnedModel.GetComponent<PrometeoCarController>().parent = this;
        spawnedModel.GetComponent<NetworkObject>().Spawn();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DestroyModelServerRpc()
    {   GameObject destroy = spawnedPrefabs[0];
        destroy.GetComponent<NetworkObject>().Despawn();
        spawnedPrefabs.Remove(destroy);
        Destroy(spawnedModel);
        spawnedModel = null;
    }
}
