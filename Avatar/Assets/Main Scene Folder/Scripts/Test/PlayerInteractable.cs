using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

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

public float xOffset;
public float yOffset;
    public float zOffset;
    public float xRotation;
    public float yRotation;
    public float zRotation;
    public RigBuilder rb;
    public GameObject weapon;
   
  
    private Transform rightHandgrip;
    private Transform leftHandgrip;
    [SerializeField]Transform shoulder;
    private bool hasWeapon;
    [SerializeField]private GameObject testingweapon;

    public TwoBoneIKConstraint lefthandIK;
    public TwoBoneIKConstraint righthandIK;
   
    private void Start()
    {
        weaponPrefabs = new Dictionary<string, GameObject>();
        weaponPrefabs["AK47"] = AK47prefab;
        anim = GetComponent<Animator>();
        AssignAnimationIDs();
         rb = GetComponent<RigBuilder>();
      
       
        Debug.Log("Hello");
        rb.enabled = false;
      
    //     rb.enabled = true;
    //      rightHandgrip = testingweapon.transform.Find("rightgrip").transform;
    //      Debug.Log(rightHandgrip);
    //    leftHandgrip = testingweapon.transform.Find("leftgrip").transform;
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
rb.enabled = true;
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
           weapon = Instantiate(weaponPrefab);

            // Set the weapon's position and rotation based on the placeholder
            Vector3 weaponPosition = weaponPlaceholder.position + new Vector3(xOffset, yOffset, zOffset);
        Quaternion weaponRotation = weaponPlaceholder.rotation * Quaternion.Euler(xRotation, yRotation, zRotation);

        // Set the weapon's position and rotation
        weapon.transform.position = weaponPosition;
        weapon.transform.rotation = weaponRotation;

            // Set the weapon's parent to the shoulder
            weapon.transform.SetParent(shoulder);

            // Update TwoBoneIK targets
            lefthandIK.data.target = weapon.transform.Find("leftgrip").transform;
            righthandIK.data.target = weapon.transform.Find("rightgrip").transform;
            
            // Rebuild the RigBuilder
            rb.Build();

            // Disable the placeholder
            weaponPlaceholder.gameObject.SetActive(false);
             
       
            hasWeapon = true;
       

        }
    }


    // Called from the animation timeline when the weapon placeholder needs to be set
    public void SetWeaponPlaceholder(Transform placeholder)
    {
        weaponPlaceholder = placeholder;
    }
    public void SetHasWeaponTrue(GameObject weaponPrefab)
    {
        anim.SetBool("HasWeapon", true);

    // Calculate the spawn position based on the player's position and offsets
    Vector3 spawnPosition = transform.position +
        transform.forward * xOffset +
        transform.up * yOffset +
        transform.right * zOffset;

    // Instantiate the weapon prefab at the calculated spawn position
    weapon = Instantiate(weaponPrefab, spawnPosition, Quaternion.identity);

    // Set the weapon's rotation based on the player's rotation and offsets
    //Quaternion spawnRotation = Quaternion.Euler(xRotation, yRotation, zRotation);
   // weapon.transform.rotation = transform.rotation * spawnRotation;

    // Set the weapon's parent to the shoulder
    weapon.transform.SetParent(shoulder);

    // Enable Rigidbody if needed
    rb.enabled = true;

    // Update TwoBoneIK targets
    lefthandIK.data.target = weapon.transform.Find("leftgrip").transform;
    righthandIK.data.target = weapon.transform.Find("rightgrip").transform;

    // Rebuild the Rigidbody
    rb.Build();
       
        
        
    }

    // Called from the animation timeline to set the HasWeapon parameter to false
    public void SetHasWeaponFalse()
    {
        anim.SetBool("HasWeapon", false);
    }

}