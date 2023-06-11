using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerInteractable : MonoBehaviour
{
    public static PlayerInteractable Instance { get; private set; }

    [SerializeField] private float interactionRange = 2f;
    private Animator anim;
    private int animPickup;
    private Transform currentInteractable;
    private bool isPickupAnimationPlaying;

    [SerializeField] private Transform handBone; // Reference to the hand bone GameObject;
    [SerializeField] private Transform weaponPlaceholder; // Empty GameObject acting as a placeholder for the weapon;

    // Dictionary to map weapon identifiers to their corresponding prefabs
    [SerializeField] private Dictionary<string, GameObject> weaponPrefabs;

    [SerializeField] private GameObject AK47prefab;
    [SerializeField] private float delayBeforeSpawn;

    [SerializeField] private float xOffset;
    [SerializeField] private float yOffset;
    [SerializeField] private float zOffset;
    [SerializeField] private float xRotation;
    [SerializeField] private float yRotation;
    [SerializeField] private float zRotation;
    private bool hasWeapon;

    private void Start()
    {
        weaponPrefabs = new Dictionary<string, GameObject>();
        weaponPrefabs["AK47"] = AK47prefab;
        anim = GetComponent<Animator>();
        AssignAnimationIDs();
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void AssignAnimationIDs()
    {
        animPickup = Animator.StringToHash("Pickup");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Interactable"))
                {
                    InteractableObject objectInteraction = collider.GetComponent<InteractableObject>();
                    if (objectInteraction != null)
                    {
                        // Face the object
                        Vector3 lookDirection = collider.transform.position - transform.position;
                        lookDirection.y = 0f;
                        transform.rotation = Quaternion.LookRotation(lookDirection);

                        // Trigger the object's interaction


                        // Get the identifier of the weapon
                        string weaponIdentifier = objectInteraction.GetWeaponIdentifier();

                        // Trigger the pickup animation with the weapon identifier
                        TriggerPickupAnimation(collider.transform.position, weaponIdentifier);

                        Debug.Log("Playing");
                        break;
                    }
                }
            }
        }
    }

    public void SetCurrentInteractable(Transform interactable)
    {
        currentInteractable = interactable;
    }

    public void TriggerPickupAnimation(Vector3 objectPosition, string weaponIdentifier)
    {
        // Face the object
        Vector3 lookDirection = objectPosition - transform.position;
        lookDirection.y = 0f;
        transform.rotation = Quaternion.LookRotation(lookDirection);

        // Trigger the pickup animation
        anim.SetTrigger(animPickup);
        isPickupAnimationPlaying = true;

        // Check if the weapon identifier exists in the dictionary
        if (weaponPrefabs.ContainsKey(weaponIdentifier))
        {
            // Get the corresponding weapon prefab
            GameObject weaponPrefab = weaponPrefabs[weaponIdentifier];

            // Call the SpawnWeapon method on the next frame with the correct prefab
            StartCoroutine(DelayedSpawnWeapon(weaponPrefab));
        }
        else
        {
            Debug.LogWarning("No prefab found for weapon identifier: " + weaponIdentifier);
        }
    }

    private IEnumerator DelayedSpawnWeapon(GameObject weaponPrefab)
    {
        //Delay before instantiating the weapon into the hand;
        yield return new WaitForSeconds(delayBeforeSpawn);

        // Spawn the weapon
        if (weaponPrefab != null)
        {
            // Instantiate the weapon prefab
            GameObject weapon = Instantiate(weaponPrefab, handBone);

            // Set the weapon's position and rotation based on the placeholder
            weapon.transform.position = weaponPlaceholder.position;
            weapon.transform.rotation = weaponPlaceholder.rotation;
             weapon.transform.localPosition = new Vector3(xOffset, yOffset, zOffset);
        weapon.transform.localRotation = Quaternion.Euler(xRotation, yRotation, zRotation);
        hasWeapon = true;
        SetHasWeaponTrue();

        }
    }


    // Called from the animation timeline when the weapon placeholder needs to be set
    public void SetWeaponPlaceholder(Transform placeholder)
    {
        weaponPlaceholder = placeholder;
    }
    public void SetHasWeaponTrue()
    {
        anim.SetBool("HasWeapon", true);
    }

    // Called from the animation timeline to set the HasWeapon parameter to false
    public void SetHasWeaponFalse()
    {
        anim.SetBool("HasWeapon", false);
    }

}