using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITree : MonoBehaviour
{

    BTNode bt;

    void Start()
    {        
        buildTree();
        StartCoroutine("RunTree", 1f); //mono behavior + fov stuff
    }

    //giving end 
    public IEnumerator RunTree(float delay){ //initiate any attack - todo: check what attacks are available here
        //as of this point the attack animation should already have begon
        while(true){
            yield return new WaitForSeconds(delay);
            test();
        }
    }


    public void buildTree(){
        BTSetup builder = new BTSetup();
        this.bt = builder
                    .EmplaceSequencer("sequencer1")
                    .EmplaceTask("task1", t => alwaysTrue())
                    .EmplaceTask("task2", t => alwaysTrue())
                .FinishNonTask()
                .Build();

    }
    

    void test() {
        Debug.Log("thinking");
        Debug.Log(bt.Evaluate(Time.deltaTime));

    }

    BTStatus alwaysFalse() {
        return BTStatus.FAILURE;
    }
    BTStatus alwaysTrue() {
        return BTStatus.SUCCESS;
    }

    BTStatus alwaysRunning() {
        return BTStatus.RUNNING;
    }
}
