using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager 
{
    //interface (typedef) public delaration for hte pause delegate
    public delegate void VoidDelegateVoid();

    //instances that handle the pause and unpause functions, they must be static events
    public static event VoidDelegateVoid pause;
    public static event VoidDelegateVoid unpause; 

    //functions are not delcared here. I believe they will be in the main camera script and on the monster scripts. 

}
