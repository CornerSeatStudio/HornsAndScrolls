using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInventoryData", menuName = "ScriptableObjects/Inventory")]
public class InventoryObject : ScriptableObject
{
    List<InventorySlot> items;

    public void Awake(){
        items = new List<InventorySlot>();
    }

    //add an item into the inventory
    public void AddItem(ItemObject item){
        items.Add(new InventorySlot(item, 1));
    }
    public void AddItem(ItemObject item, int quantity){
        items.Add(new InventorySlot(item, quantity));
    }

    //find the item in the inventory, delete it if its used up
    public void UseItem(ItemObject item, CharacterHandler character){
        InventorySlot slotRef = items.Find(i => i.Item == item);
        if(slotRef == null) Debug.LogError("couldnt use item, ain't in inventory retard");
        if(slotRef.UsedItem(character)) items.Remove(slotRef);
    }

}

[System.Serializable]
public class InventorySlot {
    public ItemObject Item {get; private set;}
    public int Quantity {get; private set; }

    public InventorySlot(ItemObject item, int quantity) {
        this.Item = item;
        this.Quantity = quantity;
    }

    //if picks up another one of those motherfucks
    public void IncreaseItemCount(){
        Quantity++;
    }

    //use item, reduce quanity, returns true if quantity has hit 0
    public bool UsedItem(CharacterHandler character){
        Item.OnUse(character);
        Quantity--;

        return Quantity == 0;
    }
}