using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface GenericState { //represents the current MOVEMENT status of a character

    IEnumerator OnStateEnter();

    IEnumerator OnStateExit();

}
