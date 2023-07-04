using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoScript : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Item[] itemsToPickUp ;
    //public List<Item> itemsToPick = new List<Item>();

    public void PickUpItem(int id)
    {
        Debug.Log(itemsToPickUp.Length);
        bool result = inventoryManager.AddItem(itemsToPickUp[id]);
        if (result == true)
        {
            Debug.Log("added item");
        }
        else
        {
            Debug.Log("Item not added");
        }
    }

    public void GetSelectedItem()
    {
        Item receivedItem = inventoryManager.GetSelectedItem(false);
        if (receivedItem != null)
        {
            Debug.Log("Received item");
        }
        else
        {
            Debug.Log("Failed to receive item");
        }
    }

    public void UseSelectedItem()
    {
        Item receivedItem = inventoryManager.GetSelectedItem(true);
        if (receivedItem != null)
        {
            Debug.Log("Used item");
        }
        else
        {
            Debug.Log("Failed to use item");
        }
    }
}
