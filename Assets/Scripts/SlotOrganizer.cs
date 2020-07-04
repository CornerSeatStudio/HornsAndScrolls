using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotOrganizer : MonoBehaviour
{
    public AIHandler[] AllEnemiesInScene { get; set; }
    [SerializeField] private GameObject target;
    public int combatSlotSize = 3;
    public float guaranteeDuration = 3; //determines turnover time between guarnatee an in
    private bool inCombat = true;

    void Start() { //get all enemies in scene, shove em into the allEnemiesInScene list, reference this list for al info
        //must be done before AITree, thus awake before start
        //todo: maybe search by layer instead?
        AllEnemiesInScene = FindObjectsOfType<AIHandler>();
    }

    //triggers via the event of AI taking damage
    public void markEnemyAsHitDriver(AIHandler markedEnemy) { //via unity event, if the player hits an AI, mark as prioritized
        //if cunt isnt dead O
        if (markedEnemy.Health > 0) StartCoroutine(markEnemyAsHit(markedEnemy));
    }

    private IEnumerator markEnemyAsHit(AIHandler markedEnemy) { //markEnemyPocketAsHit -> make all pocketed enemies aggro also
        markedEnemy.CombatSlot = CombatSlot.GUARANTEE;

        if (markedEnemy.GlobalState != AIGlobalState.AGGRO) {
            //for all other ai in the same combat pocket, put them a combat state too
            //just go through array for now lol cause im lazy
            //if the ai is already in combat, this loop 
            markedEnemy.GlobalState = AIGlobalState.AGGRO;
            foreach (AIHandler enemy in AllEnemiesInScene) {
                if (enemy != markedEnemy && enemy.combatPocket == markedEnemy.combatPocket) {
                    markedEnemy.GlobalState = AIGlobalState.AGGRO;
                }

            }

        }

        yield return new WaitForSeconds(guaranteeDuration);
        markedEnemy.CombatSlot = CombatSlot.IN;
    }

    //todo - play with eviction (a guarantee that an enemy wont be in a combat slot)

    void AssignSlots() {
        foreach (AIHandler enemy in AllEnemiesInScene) { //for each enemy in scene
            inCombat = false; //only switch to true if at least one AI is in combat
            enemy.CombatSlot = CombatSlot.OUT; //set all to out, only switch those in later

            //check if combat state & isn't already guaranteed a spot, 
            
            if (enemy.GlobalState != AIGlobalState.AGGRO ) { enemy.Priority = -1; }
            
            else if(enemy.CombatSlot != CombatSlot.GUARANTEE) {
                inCombat = true;

                //archers always have 0 priority TODO
                if(false) { enemy.Priority = 0; }

                else {
                    //ARBRITRARY VALUE -> closer == higher priority
                    enemy.Priority = 100 / (1 + (0.01f * (enemy.transform.position - target.transform.position).sqrMagnitude));
                }

            }  
            
        }


        if (!inCombat) { return; } //if no units are in combat OR if all units in combat are in GUARANTEE state

        else {
            Array.Sort(AllEnemiesInScene, (a, b) => a.Priority.CompareTo(b.Priority));
            for(int i = 0; i < combatSlotSize && i < AllEnemiesInScene.Count(); ++i) {
                if (AllEnemiesInScene[i].Priority == -1 ) {
                    return;
                 } else {
                     AllEnemiesInScene[i].CombatSlot = CombatSlot.IN; 
                 }
                 
            }

        }

    }

}
