using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GruntHandler : AIHandler
{
    private bool localSwingWillyFlag = true;
    public BTStatus SwingWilly() {

        if(hitDetection.InMeleeRoutine) { 
            return BTStatus.RUNNING;
        } else if (localSwingWillyFlag) {
            agent.isStopped = true;
            StartCoroutine(hitDetection.InitAttack(weapon.startup, weapon.endlag, weapon.damage));
            localSwingWillyFlag = false;
            return BTStatus.RUNNING;
        } 
        else {           
            localSwingWillyFlag = true;
            agent.isStopped = false;
            return BTStatus.SUCCESS;
        }
    }
}
