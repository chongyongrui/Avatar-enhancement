using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSpawner : MonoBehaviour
{public GameObject modelPrefab;
    private GameObject spawnedModel;

    private void OnMouseDown()
    {
        if (spawnedModel == null)
        {
            SpawnModel();
        }
        else
        {
            DestroyModel();
        }
    }

    private void SpawnModel()
    {
        spawnedModel = Instantiate(modelPrefab, new Vector3(2.6f, -0.51f, -2f),  Quaternion.Euler(0f, -63f, 0f));
    }

    private void DestroyModel()
    {
        Destroy(spawnedModel);
        spawnedModel = null;
    }
}
