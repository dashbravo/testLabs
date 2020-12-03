using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager 
{
    //interface (typedef) public delaration for the pause delegate
    public delegate void VoidDelegateVoid();

    //instances that handle the pause and unpause functions, they must be static events
    public static event VoidDelegateVoid handleEvent1;
    public static event VoidDelegateVoid handleEvent2; 

    //mutator or accessors, these are what is called in the other scripts after they have subscribed to the events. 
    public static void Pause()
    {
        if (handleEvent1 != null)
            handleEvent1();
    }

    public static void UnPause()
    {
        if (handleEvent2 != null)
            handleEvent2();
    }


}
