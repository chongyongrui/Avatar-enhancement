using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AimTrigger : MonoBehaviour
{
    public List<GameObject> targetObjects = new List<GameObject>();
    public List<Transform> sourceObjects = new List<Transform>();
    [SerializeField] private Rig rig;
    [SerializeField] private MultiAimConstraint gunAim;
    [SerializeField] private MultiAimConstraint headAim;
   private RigBuilder rb;
    [SerializeField] private float aimDuration = 0.3f;

    private GameObject currentTarget;
    private bool hasWeaponSpawned = false;
    private bool isAiming = false;

    private void Start()
    {
        FindTargetObjects();
        rb = GetComponent<RigBuilder>();
    }

    private void OnEnable()
    {
        PlayerInteractable.OnHasWeaponChanged += HandleHasWeaponChanged;
    }

    private void OnDisable()
    {
        PlayerInteractable.OnHasWeaponChanged -= HandleHasWeaponChanged;
    }

    private void HandleHasWeaponChanged(bool value)
    {
        hasWeaponSpawned = value;
        Debug.Log("Weapon has spawned: " + hasWeaponSpawned);
        // Use the hasWeapon value as needed
    }

    private void FindTargetObjects()
    {
        // Find target objects in the scene based on specific criteria
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag("Target");

        foreach (GameObject obj in foundObjects)
        {
            targetObjects.Add(obj);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasWeaponSpawned && targetObjects.Contains(other.gameObject))
        {
            int index = targetObjects.IndexOf(other.gameObject);
            if (index >= 0 && index < targetObjects.Count)
            {
                currentTarget = other.gameObject;
                Debug.Log(currentTarget.name);
                StartCoroutine(AimAtTarget(targetObjects[index].transform));
                Debug.Log(targetObjects[index].name);
            }
        }
    }

   private void OnTriggerExit(Collider other)
{
    if (other.gameObject == currentTarget)
    {
        currentTarget = null;

        if (isAiming)
        {
          
            StartCoroutine(StopAiming());
        }
    }
}

private IEnumerator AimAtTarget(Transform target)
{
    isAiming = true;

    // Clear the previous source objects
    ClearSourceObjects();

    // Add the current source object to the head aim rig
    AddSourceObjectToHead(target);
    rb.Build();
    // Add the current source object to the gun aim rig
    AddSourceObjectToGun(target);

    // Build the rig after modifying the source objects
    yield return new WaitForEndOfFrame();
    rb.Build();

    float elapsedTime = 0f;
    float startWeight = rig.weight;
    float targetWeight = 1f;

    while (elapsedTime < aimDuration)
    {
        float t = elapsedTime / aimDuration;
        rig.weight = Mathf.Lerp(startWeight, targetWeight, t);
        elapsedTime += Time.deltaTime;
        yield return null;
    }

    rig.weight = targetWeight;
}

private IEnumerator StopAiming()
{
    // Decrease the rig weight
    float elapsedTime = 0f;
    float startWeight = rig.weight;
    float targetWeight = 0f;

    while (elapsedTime < aimDuration)
    {
        float t = elapsedTime / aimDuration;
        rig.weight = Mathf.Lerp(startWeight, targetWeight, t);
        elapsedTime += Time.deltaTime;
        yield return null;
    }

    rig.weight = targetWeight;

    // Wait for a short delay before clearing the source objects
    yield return new WaitForSeconds(0.1f);

    // Clear all source objects
    ClearSourceObjects();

    // Build the rig after clearing the source objects
    rb.Build();
}


   private void ClearSourceObjects()
{
    RemoveAllSourceObjects(headAim);
    RemoveAllSourceObjects(gunAim);
    sourceObjects.Clear();

    // Set the sourceObjects to an empty WeightedTransformArray
    headAim.data.sourceObjects = new WeightedTransformArray();
    gunAim.data.sourceObjects = new WeightedTransformArray();
}


   private void AddSourceObjectToHead(Transform sourceObject)
{
    WeightedTransformArray weightedTransformArray = new WeightedTransformArray();
    weightedTransformArray.Add(new WeightedTransform(sourceObject, 1f));
    headAim.data.sourceObjects = weightedTransformArray;
    sourceObjects.Add(sourceObject);
}

private void AddSourceObjectToGun(Transform sourceObject)
{
    WeightedTransformArray weightedTransformArray = new WeightedTransformArray();
    weightedTransformArray.Add(new WeightedTransform(sourceObject, 1f));
    gunAim.data.sourceObjects = weightedTransformArray;
}


    private void RemoveAllSourceObjects(MultiAimConstraint constraint)
    {
        constraint.data.sourceObjects.Clear();
    }
}
