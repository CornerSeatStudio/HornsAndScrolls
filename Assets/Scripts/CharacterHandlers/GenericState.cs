using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface GenericState { //represents the current MOVEMENT status of a character

    IEnumerator OnStateEnter();

    IEnumerator OnStateExit();

}
