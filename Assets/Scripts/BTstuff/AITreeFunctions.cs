using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITreeFunctions : MonoBehaviour {
    
    //enums for state verification
    public enum WeaponType { MELEE, RANGED };
    public enum AIState { IDLE, PATROL, SLEEPING, COMBAT, DEAD };
    public enum PlayerState { HIDDEN, COMBAT, DEAD };

    //blackboard data - stores information on self/target current
    public AIHandler self;
    public CharacterHandler target; 
    List<AIHandler> proximateAI;

    public AITreeFunctions(AIHandler self, CharacterHandler target, List<AIHandler> proximateAI){
        this.self = self;
        this.target = target;
        this.proximateAI = proximateAI;
    }



    //non combat tree
    public bool ValidifyCombatState() {return true; }//if this succeeds, go to combat routine, otherwise, continue stealth routine 
    public bool PlayerInLOS() { return true;}
    public bool IsSleepingState() {return true;}
    public bool IsPatrolState() {return true;} //make into lambda
    public BTStatus MaintainLOSWhileStationary() {return BTStatus.RUNNING;} //guarrantees both look direction and posiion remain constant
    public BTStatus IncrementSpotTimer() {return BTStatus.RUNNING;}
    public BTStatus DecrementSpotTimer() {return BTStatus.RUNNING;}
    public BTStatus LayDownAndSnoozeMotherfucker() {return BTStatus.RUNNING;} //only transition when neccesary
    public BTStatus ExecutePatrol() {return BTStatus.RUNNING;}
    public BTStatus Idle() {return BTStatus.RUNNING;}

    //slot determiner functions
    public bool IsValidSlot() {return true;}  //if the unit occupying the slot is valid (via cool maths), do nothing, 
    public bool IsInSlot() {return true;}
    public bool IsOutSlot() {return true;}
    public BTStatus MoveToInSlot() {return BTStatus.RUNNING;}
    public BTStatus MoveToOutSlot() {return BTStatus.RUNNING;}

    //combat tree stuff
    public bool IsMelee() {return true;}
    public bool IsRanged() {return true;}
    public BTStatus ChasePlayer() {return BTStatus.RUNNING; }
    public BTStatus SwingWilly() {return BTStatus.RUNNING;}
    public BTStatus SpaceFromTarget() {return BTStatus.RUNNING;}
    public BTStatus BopFoolWithArrow() {return BTStatus.RUNNING;}


}
