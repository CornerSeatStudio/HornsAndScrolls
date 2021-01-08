using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Collections;

//all ai behavior tree stuff lives here
//hard coded cause why not lmao
//includes functionality on selecotrs, sequencers, and tasks only - you can implement more if you want
public class ThinkCycle : MonoBehaviour
{
    private BTNode root;
    private AIHandler ai;
    private IEnumerator tempRoutine;
    public float thinkDelay = .2f;
    private bool isDead = false;

    void Start()
    {        
        //on ai start
        ai = this.GetComponent<AIHandler>();
        buildTree(); 
        StartCoroutine("RunTree", thinkDelay); 

    }


    //jsut runs the "think cycle" every delay until dead
    IEnumerator RunTree(float delay) { 
        while (!isDead) {
            //Debug.Log("running");

            root.Evaluate(Time.deltaTime);
            yield return new WaitForSeconds(delay);

        }
    }

    //this is the BT itself, will be changed a lot
    //cause i didnt want to buy some shitty unity GUI for a BT, doing it by code is feasable
    public void buildTree() { 
        BTSetup builder = new BTSetup();
        this.root = builder
                .EmplaceSelector("main selector")
                    .EmplaceTask("stealth", t => ai.VerifyStealth()) 
                    .EmplaceSequencer("Combat Sequence")
                        .EmplaceConditional("Combat Capable", t => ai.VerifyCombatCapable()) 
                        .EmplaceSelector("combat Selection") 
                            .EmplaceSequencer("Defense sequencer")
                                .EmplaceConditional("defence conditional", t => ai.DefenceConditional())
                                .EmplaceTask("defend", t => ai.DefenseTask())
                                .PopDepth()
                            // .EmplaceSequencer("Charge sequencer")
                            //     .EmplaceConditional("range check" , t => ai.WorthCharging())
                            //     .EmplaceTask("close distance", t => ai.ChargingTask())
                            //     .PopDepth()
                            .EmplaceSequencer("Close Distance")
                                .EmplaceConditional("range check" , t => ai.CloseDistanceConditional())
                                .EmplaceTask("close distance", t => ai.ChasingTask())
                                .PopDepth()
                            // .EmplaceSequencer("offense sequencer")
                            //     .EmplaceConditional("offense conditional", t => ai.OffenseConditional())
                            //     .EmplaceTask("offense task", t => ai.OffenseTask())
                            //     .PopDepth()
                            // .EmplaceSequencer("Instant shove")
                            //     .EmplaceConditional("insta shove check", t => ai.InstantShoveConditional())
                            //     .EmplaceTask("insta shove",t => ai.ShovingTask())
                            //     .PopDepth()
                            .EmplaceSequencer("back away sequencer")
                                .EmplaceConditional("back away conditional", t => ai.BackAwayConditional())
                                .EmplaceTask("back away", t => ai.BackAwayTask())
                                .PopDepth()
                            // .EmplaceSequencer("spacing selector")
                            //     .EmplaceConditional("spacing conditional", t => ai.SpacingConditional())
                            //     .EmplaceTask("spacing task", t => ai.SpacingTask())
                            //     .PopDepth()
                            .EmplaceTask("idle about", t => alwaysRunning())
                            .PopDepth()
                        .PopDepth()
                    .EmplaceTask("kill brain", t => KillBrain())
                    .PopDepth()
                .Build();

    } 

    //temp functions just to test shit out
    BTStatus alwaysRunning() {
        return BTStatus.RUNNING;
    }

    BTStatus alwaysFalse() {
        return BTStatus.FAILURE;
    }

    //specifically for stopping the trhink cycle coroutine
    BTStatus KillBrain() {
        Debug.Log("killing brain");
        isDead = true;
        return BTStatus.SUCCESS;
    }

}