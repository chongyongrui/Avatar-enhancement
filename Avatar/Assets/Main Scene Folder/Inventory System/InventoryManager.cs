using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{

    public static InventoryManager instance; 
    public Item[] startingItems; 
    public InventorySlot[] InventorySlots;
    public int maxStackedItems = 10;
    public GameObject inventoryItemPrefab;

    int selectedSlot = 0;


    private void Awake()
    {
        instance = this; 
    }


    private void Start()
    {
        foreach(var item in startingItems)
        {
            AddItem(item);
        }
        ChangeSelectedSlot(0);
    }

    private void Update()
    {
       if (Input.inputString!= null)
        {
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if(isNumber && number>0 && number <7)
            {
                ChangeSelectedSlot((int)number-1);
            }
        }


       if (Input.GetKeyDown(KeyCode.Comma) && selectedSlot!= 0 ) {
           
            ChangeSelectedSlot(selectedSlot-1);
        }

       if (Input.GetKeyDown(KeyCode.Period) && selectedSlot != 5)
        {
            
            ChangeSelectedSlot(selectedSlot+1);
        }

    }

    public Item GetSelectedItem(bool used)
    {
        InventorySlot slot = InventorySlots[selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null)
        {
            Item item = itemInSlot.item;
            if (used)
            {
                itemInSlot.count--;
                if (itemInSlot.count <= 0)
                {
                    Destroy(itemInSlot.gameObject);
                }
                else
                {
                    itemInSlot.RefreshCount();
                }
            }

            return item;

        }
        return null;
    }

    void ChangeSelectedSlot(int newValue)
    {
        if (selectedSlot >= 0)
        {
            InventorySlots[selectedSlot].Unselect();
        }
        
        InventorySlots[newValue].Select();
        selectedSlot = newValue;
    }


    public bool AddItem(Item item)
    {
        for (int i = 0; i < InventorySlots.Length; i++)
        {
            InventorySlot slot = InventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.item == item && itemInSlot.count < maxStackedItems && itemInSlot.item.stackable)
            {
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return true;
            }

        }


        for (int i = 0; i < InventorySlots.Length; i++)
        {
           InventorySlot slot = InventorySlots[i];
           InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null)
            {
                SpawnNewItemInSlot(item, slot);
                ChangeSelectedSlot(i);

                return true;
            }
            
        }
        return false;
    }

    public void SpawnNewItemInSlot(Item item, InventorySlot slot)
    {
        GameObject newItemGameObject = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGameObject.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item);
    }
}
