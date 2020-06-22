using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task : BTNode {
    private string name;
    private Func<float, BTStatus> fnc;

    public Task(string name, Func<float, BTStatus> fnc){
        this.name = name;
        this.fnc = fnc;
    }

    public override BTStatus Evaluate(float timeDelta) {
        return fnc(timeDelta);
    } 

}
