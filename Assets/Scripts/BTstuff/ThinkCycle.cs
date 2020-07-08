using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ThinkCycle : MonoBehaviour
{
    private SlotOrganizer slotter;
    private List<BTNode> AItrees;
    private IEnumerator tempRoutine;
    public float thinkDelay;

    void Start()
    {        
        AItrees = new List<BTNode>();
        slotter = this.GetComponent<SlotOrganizer>();
        foreach(AIHandler ai in slotter.AllEnemiesInScene) {
            //Debug.Log(ai.transform.position);
            if (ai.GetType() == typeof(GruntHandler)) {
                AItrees.Add(buildTree((GruntHandler)ai));
                break;
            }
        }

        //nce all trees are in, start thinking process
        StartCoroutine("RunTrees", thinkDelay);

    }

    IEnumerator RunTrees(float delay) {
        while (true) {
            yield return new WaitForSeconds(delay);
            //evaluate combat slots

            foreach(BTNode tree in AItrees) {
                tree.Evaluate(delay);

            }
        }
    }

    public BTNode buildTree(GruntHandler ai) { 
        BTSetup builder = new BTSetup();
        return builder
                .EmplaceSelector("main selector")
                    .EmplaceTask("stealth", t => ai.VerifyStealth()) 
                    .EmplaceSelector("combat sequence")
                        .EmplaceConditional("verify", t => ai.VerifyCombatIncapable()) 
                        .EmplaceTask("engage", t => ai.EngageDriver())
                    .FinishNonTask()
                .FinishNonTask()
                .Build();

    } 

    BTStatus alwaysFalse() {
        return BTStatus.SUCCESS;
    }

    BTStatus alwaysTrue() {
        Debug.Log("stealth has FAILED, breaking into combat");
        //stop detection script, start stamina script?
        return BTStatus.RUNNING;
    }

}
