using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class SlotOrganizer : MonoBehaviour
{
    public AIHandler[] AllEnemiesInScene { get; set; }
    public int combatSlotSize = 3;
    public float afterHitAggresiveness = 3; //determines turnover time between guarnatee an in

    void Start() { //get all enemies in scene, shove em into the allEnemiesInScene list, reference this list for al info
        //must be done before AITree, thus awake before start
        AllEnemiesInScene = FindObjectsOfType<AIHandler>();
    }

    //triggers via the event of AI taking damage
    public void markEnemyAsHitDriver(AIHandler markedEnemy) { //via unity event, if the player hits an AI, mark as prioritized
        StartCoroutine(markEnemyAsHit(markedEnemy));
    }

    private IEnumerator markEnemyAsHit(AIHandler markedEnemy) {
        markedEnemy.CombatSlot = CombatSlot.GUARANTEE;
        yield return new WaitForSeconds(this.afterHitAggresiveness);
        markedEnemy.CombatSlot = CombatSlot.IN;

    }

    //todo - play with eviction (a guarantee that an enemy wont be in a combat slot)

    void AssignSlots() {
        foreach (AIHandler enemy in AllEnemiesInScene) { //for each enemy in scene
            //determine their value 

            //check if combat state
            //first, check if theyre dead
            //second, check if they've been hit recently -> guaranteed priority
            //enemy.Priority = 
            //combat pocket - high priority

            //distance proximity
            
            //unit type - range minimal, melee normal

        }


    }

}
