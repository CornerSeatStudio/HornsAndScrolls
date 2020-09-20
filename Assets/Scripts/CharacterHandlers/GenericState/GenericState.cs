using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//Finite state Machine for managing stances, combat modes, etc
public interface GenericState { 

    IEnumerator OnStateEnter();

    IEnumerator OnStateExit();

}
