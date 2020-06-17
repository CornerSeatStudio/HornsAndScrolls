using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequencer : BTParentNode {
    private string name;
    private List<BTNode> children = new List<BTNode>();

    public Sequencer(string name){
        this.name = name;
    }

    public BTStatus Evaluate(float timeDelta) {
        foreach(BTNode child in children) {
            BTStatus status = child.Evaluate(timeDelta);
            if (status != BTStatus.SUCCESS) {
                return status;
            }

        }

        return BTStatus.SUCCESS;
    } 

    public void AddChild(BTNode child) {
        children.Add(child);
    }

}
