using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ThinkCycle : MonoBehaviour
{
    //private SlotOrganizer slotter;
    private BTNode root;
    private AIHandler ai;
    private IEnumerator tempRoutine;
    public float thinkDelay = .2f;
    private bool isDead = false;

    void Start()
    {        
        ai = this.GetComponent<AIHandler>();
        buildTree();
        //nce all trees are in, start thinking process
        StartCoroutine("RunTree", thinkDelay);

    }

    IEnumerator RunTree(float delay) {
        while (!isDead) {
            //Debug.Log("running");
            yield return new WaitForSeconds(delay);
            root.Evaluate(Time.deltaTime);
        }
    }

    public void buildTree() { 
        BTSetup builder = new BTSetup();
        this.root = builder
                .EmplaceSelector("main selector")
                    .EmplaceTask("stealth", t => ai.VerifyStealth()) 
                    .EmplaceSelector("combat Selection")
                        .EmplaceConditional("Combat Incapable", t => ai.VerifyCombatIncapable()) 
                        .EmplaceSequencer("Defense selector")
                            .EmplaceConditional("defence conditional", t => ai.DefenceConditional())
                            .EmplaceTask("defend", t => ai.DefenseTask())
                            .PopDepth()
                        .EmplaceSequencer("Close Distance")
                            .EmplaceConditional("range check" , t => ai.CloseDistanceConditional())
                            .EmplaceTask("close distance", t => ai.ChasingTask())
                            .PopDepth()
                        .EmplaceSequencer("offense selector")
                            .EmplaceConditional("offense conditional", t => ai.OffenseConditional())
                            .EmplaceTask("offense task", t => ai.OffenseTask())
                            .PopDepth()
                        .EmplaceSequencer("Instant shove")
                            .EmplaceConditional("insta shove check", t => ai.InstantShoveConditional())
                            .EmplaceTask("insta shove",t => ai.ShovingTask())
                            .PopDepth()
                        .EmplaceSequencer("back away selector")
                            .EmplaceConditional("back away conditional", t => ai.BackAwayConditional())
                            .EmplaceTask("back away", t => ai.BackAwayTask())
                            .PopDepth()
                        .EmplaceTask("Shuffle about", t => ai.ShuffleTask())
                        .PopDepth()
                    .EmplaceTask("kill brain", t => KillBrain())
                    .PopDepth()
                .Build();

    } 

    BTStatus alwaysFalse() {
        return BTStatus.FAILURE;
    }

    BTStatus KillBrain() {
        Debug.Log("killing brain");
        isDead = true;
        return BTStatus.SUCCESS;
    }

}
