using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableItemScript : MonoBehaviour
{
    public GameObject myHands; //reference to your hands/the position where you want your object to go
    bool canpickup; //a bool to see if you can or cant pick up the item
    GameObject objectToPickUp; // the gameobject onwhich you collided with
    public bool hasItem; // a bool to see if you have an item in your hand
    public static PickableItemScript instance;
    public Camera camera;

    List<GameObject> list = new List<GameObject>();


    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        canpickup = false;    //setting both to false
        hasItem = false;
        camera = GameObject.FindGameObjectWithTag("PlayerFollowCamera").GetComponent<Camera>();


    }

    // Update is called once per frame
    void Update()
    {
        if (canpickup == true) // if you enter thecollider of the objecct
        {
            if (Input.GetKeyDown(KeyCode.F) && hasItem == false)
            {
                objectToPickUp.GetComponent<Rigidbody>().isKinematic = true;   //makes the rigidbody not be acted upon by forces
                objectToPickUp.transform.position = myHands.transform.position; // sets the position of the object to your hand position
                objectToPickUp.transform.parent = myHands.transform; //makes the object become a child of the parent so that it moves with the hands
                hasItem = true;
                BoxCollider[] bc = objectToPickUp.GetComponents<BoxCollider>();
                bc[1].enabled = false;

                Quaternion myRotation = Quaternion.identity;
                myRotation.eulerAngles = new Vector3(-7.5f, 172, -260);
                objectToPickUp.transform.rotation = myRotation;
                //list.Add(gunToPickUp);

            }


        }

        if (Input.GetKeyDown(KeyCode.G) && hasItem == true) // if you have an item and get the key to remove the object, again can be any key
        {
            objectToPickUp.GetComponent<Rigidbody>().isKinematic = false; // make the rigidbody work again

            objectToPickUp.transform.parent = null; // make the object not be a child of the hands
            hasItem = false;

            BoxCollider[] bc = objectToPickUp.GetComponents<BoxCollider>();
            bc[1].enabled = true;


        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && hasItem == true && objectToPickUp.name == "Dynamite") // holding dynamite and click to use
        {
            objectToPickUp.GetComponent<Rigidbody>().isKinematic = false; // make the rigidbody work again

            objectToPickUp.transform.parent = null; // make the object not be a child of the hands
            hasItem = false;

            BoxCollider[] bc = objectToPickUp.GetComponents<BoxCollider>();
            bc[1].enabled = true;

            Rigidbody rb = objectToPickUp.GetComponent<Rigidbody>();
            //Vector3 cameraDirection = camera.transform.forward;
            rb.AddForce(Vector3.up * 2.5f);
            rb.AddForce(Vector3.forward * 10f);

        }
    }
    private void OnTriggerEnter(Collider other) // to see when the player enters the collider
    {
        Debug.Log("Picakable object found");
        if (other.gameObject.tag == "PickableObject") //on the object you want to pick up set the tag to be anything, in this case "object"
        {
            canpickup = true;  //set the pick up bool to true
            objectToPickUp = other.gameObject; //set the gameobject you collided with to one you can reference
            Debug.Log("Picakable object found");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        canpickup = false; //when you leave the collider set the canpickup bool to false

    }
}
