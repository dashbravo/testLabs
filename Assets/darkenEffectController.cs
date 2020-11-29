using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class darkenEffectController : MonoBehaviour
{
    void Start()
    {
        Color spriteAlpha = gameObject.GetComponent<SpriteRenderer>().color;
        spriteAlpha.a = 1.0f;
    }

    void OnEnable()
    {
        EventManager.handleEvent1 += Pause;
        EventManager.handleEvent2 += UnPause; 
    }

    void OnDisable()
    {
        EventManager.handleEvent1 -= Pause;
        EventManager.handleEvent2 -= UnPause;
    }

    void Pause()
    {
        Color spriteAlpha = gameObject.GetComponent<SpriteRenderer>().color;        
        spriteAlpha.a = 1.0f;
    }

    void UnPause()
    {
        Color spriteAlpha = gameObject.GetComponent<SpriteRenderer>().color;        
        spriteAlpha.a = 0.0f;
    }
    
}
