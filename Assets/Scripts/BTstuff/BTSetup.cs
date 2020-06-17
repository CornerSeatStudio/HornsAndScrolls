using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BTStatus { SUCCESS, FAILURE, RUNNING };

public class BTSetup {

    private BTNode currParentNode = null;
    private Stack<BTParentNode> parentNodes = new Stack<BTParentNode>();

    public BTSetup EmplaceTask(string name, Func<float, BTStatus> fnc) {
        if (parentNodes.Count <= 0) {
            throw new System.Exception();
            Debug.LogError("task node must be leaf");
        }

        Task task = new Task(name, fnc);
        parentNodes.Peek().AddChild(task);
        return this;
    }

    public BTSetup EmplaceConditional(string name, Func<float, bool> fnc) {
        return EmplaceTask(name, t => fnc(t) ? BTStatus.SUCCESS : BTStatus.FAILURE);
    }

    public BTSetup EmplaceSequencer(string name) {
        Sequencer sequencer = new Sequencer(name);
        if(parentNodes.Count > 0) {
            parentNodes.Peek().AddChild(sequencer);
        }

        parentNodes.Push(sequencer);
        return this;
    }

    public BTSetup EmplaceSelector(string name) {
        Selector selector = new Selector(name);
        if(parentNodes.Count > 0) {
            parentNodes.Peek().AddChild(selector);
        }

        parentNodes.Push(selector);
        return this;
    }

    public BTSetup FinishNonTask() { //used whenever you want to move back up to the parent
        currParentNode = parentNodes.Pop();
        return this;
    }

    public BTNode Build() {
        if(currParentNode == null) { 
            throw new System.Exception();
            Debug.LogError("no nodes in current BT");
        }

        return currParentNode;
    } //for this to work, currParentNode must point to the root (via correct # of FinishNonTasks)
}
