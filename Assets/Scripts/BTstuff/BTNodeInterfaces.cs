using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTNode {
    public virtual BTStatus Evaluate(float timeDelta) {
        //default evaluation shoudn't exist
        return BTStatus.FAILURE;                 

    }

}

public class BTParentNode : BTNode {
   public virtual void AddChild(BTNode child) { }

}