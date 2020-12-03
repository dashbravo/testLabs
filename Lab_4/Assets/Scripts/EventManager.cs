using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager
{
    //interface (typedef) public declaration for the puase instance
    public delegate void VoidDelegateVoid();

    //instances that handle the pause and unpause fucntions, they must be static events
    public static event VoidDelegateVoid HandleEvent1; //this is pause!
    public static event VoidDelegateVoid HandleEvent2; //call is unpause!

    //these functions are activators for the events. 
    public static void InvokeEvent1() //this is the activator for pause event
    {
        if (HandleEvent1 != null)
            HandleEvent1();

        //HandleEvent1.Invoke(); this is the same as the line above, another way to call the function to invoke the event. 
    }
  
    public static void InvokeEvent2() //this is the activator for unpause event
    {
        if (HandleEvent2 != null)
            HandleEvent2();
    }

}
