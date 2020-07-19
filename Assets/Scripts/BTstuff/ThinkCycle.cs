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
            yield return new WaitForSeconds(delay);
            root.Evaluate(Time.deltaTime);
        }
    }

    public void buildTree() { 
        BTSetup builder = new BTSetup();
        this.root = builder
                .EmplaceSelector("main selector")
                    //.EmplaceTask("stealth", t => ai.VerifyStealth()) 
                    .EmplaceTask("testfalse", t => alwaysFalse())
                    .EmplaceSelector("combat sequence")
                        .EmplaceConditional("verify", t => ai.VerifyCombatIncapable()) 
                        .EmplaceTask("engage", t => alwaysFalse())
                    .FinishNonTask()
                .FinishNonTask()
                .Build();

    } 

    BTStatus alwaysFalse() {
        return BTStatus.FAILURE;
    }

    BTStatus KillBrain() {
        isDead = true;
        return BTStatus.SUCCESS;
    }

}
