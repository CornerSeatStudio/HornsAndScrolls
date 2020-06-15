using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface BTNode {
    BTStatus Evaluate(float timeDelta);

}

public interface BTParentNode : BTNode {
    void AddChild(BTNode child);

}