using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : BTParentNode {
    private string name;
    private List<BTNode> children = new List<BTNode>();

    public Selector(string name){
        this.name = name;
    }

    public override BTStatus Evaluate(float timeDelta) {
        foreach(BTNode child in children) {
            BTStatus status = child.Evaluate(timeDelta);
            if (status != BTStatus.FAILURE) {
                return status;
            }

        }

        return BTStatus.FAILURE;
    } 

    public override void AddChild(BTNode child) {
        children.Add(child);
    }

}
