using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotOrganizer : MonoBehaviour
{
    public AIHandler[] AllEnemiesInScene { get; set; }
    public int combatSlotSize = 3;
    public float guaranteeDuration = 3; //determines turnover time between guarnatee an in
    private bool inCombat = true;

    void Start() { //get all enemies in scene, shove em into the allEnemiesInScene list, reference this list for al info
        //must be done before AITree, thus awake before start
        AllEnemiesInScene = FindObjectsOfType<AIHandler>();
    }

    //triggers via the event of AI taking damage
    public void markEnemyAsHitDriver(AIHandler markedEnemy) { //via unity event, if the player hits an AI, mark as prioritized
        //if cunt isnt dead
        if (markedEnemy.Health > 0) StartCoroutine(markEnemyAsHit(markedEnemy));
    }

    private IEnumerator markEnemyAsHit(AIHandler markedEnemy) { //markEnemyPocketAsHit -> make all pocketed enemies aggro also
        markedEnemy.AIState = AIState.COMBAT;
        markedEnemy.CombatSlot = CombatSlot.GUARANTEE;
        yield return new WaitForSeconds(guaranteeDuration);
        markedEnemy.CombatSlot = CombatSlot.IN;

    }

    //todo - play with eviction (a guarantee that an enemy wont be in a combat slot)

    void AssignSlots() {
        foreach (AIHandler enemy in AllEnemiesInScene) { //for each enemy in scene
            inCombat = false; //only switch to true if at least one AI is in combat
            enemy.CombatSlot = CombatSlot.OUT; //set all to out, only switch those in later

            //check if combat state & isn't already guaranteed a spot
            if(enemy.AIState == AIState.COMBAT && enemy.CombatSlot != CombatSlot.GUARANTEE) {
                inCombat = true;

                

            } else {
                enemy.Priority = -1;
            }
            
        }

        if (!inCombat) { return; }

        else {
            Array.Sort(AllEnemiesInScene, (a, b) => a.Priority.CompareTo(b.Priority));
            for(int i = 0; i < combatSlotSize; ++i) {
                AllEnemiesInScene[i].CombatSlot = CombatSlot.IN;
            }
        }    


    }

}
