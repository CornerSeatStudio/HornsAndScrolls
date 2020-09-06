using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInventoryData", menuName = "ScriptableObjects/Inventory")]
public class InventoryObject : ScriptableObject
{
    public List<InventorySlot> items = new List<InventorySlot>(); 

    public InventorySlot FindInInventory(string itemName) {
        return items.Find(i => i.item.name == itemName);
    }
    
    public InventorySlot FindInInventory(ItemObject item) {
        return items.Find(i => i.item == item);
    }

    //add an item into the inventory
    public void AddItem(ItemObject item) {
        InventorySlot slotRef = FindInInventory(item);
        if(slotRef == null){
            items.Add(new InventorySlot(item, 1));
        } else {
            slotRef.IncrementItemCount();
        }

        //update UI

    }
    public void AddItem(ItemObject item, int quantity){
        InventorySlot slotRef = FindInInventory(item);
        if(slotRef == null){
            items.Add(new InventorySlot(item, quantity));
        } else {
            slotRef.IncreaseItemCount(quantity);
        }
    }

    //find the item in the inventory, delete it if its used up
    public void UseItem(string itemName, CharacterHandler character){
        InventorySlot slotRef = FindInInventory(itemName);
        if(slotRef != null && slotRef.UsedItem(character)) items.Remove(slotRef);
    }

}

[System.Serializable] public class InventorySlot { 
    [SerializeField] public ItemObject item;
    [SerializeField] public int quantity;

    public InventorySlot(ItemObject item, int quantity) {
        this.item = item;
        this.quantity = quantity;

        //update inventory ui
    }

    //if picks up another one of those motherfucks
    public void IncrementItemCount(){
        quantity++;

        //update inventory ui
    }

    public void IncreaseItemCount(int quantity) {
        this.quantity += quantity;

        //update inventory ui
    }

    //use item, reduce quanity, returns true if quantity has hit 0
    public bool UsedItem(CharacterHandler character){
        item.OnUse(character);
        quantity--;

        return quantity == 0;
    }
}