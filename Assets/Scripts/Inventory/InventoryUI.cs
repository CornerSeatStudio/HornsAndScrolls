using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    InventoryObject playerInventory;

    TextMeshProUGUI healthPotCount;
    TextMeshProUGUI staminaPotCount;

    public void Start(){

        OnInventoryUpdate();
    }

    public void OnInventoryUpdate(){
        healthPotCount.SetText(playerInventory.FindInInventory("HealthPot").quantity.ToString());
        staminaPotCount.SetText(playerInventory.FindInInventory("StaminaPot").quantity.ToString());
    }
}
