using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkenEffectController : MonoBehaviour
{
    SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>(); 
    }

    private void OnEnable()
    {
        EventManager.HandleEvent1 += darkenAlpha;
        EventManager.HandleEvent2 += restoreAlpha;
    }

    private void OnDisable()
    {
        EventManager.HandleEvent1 -= darkenAlpha;
        EventManager.HandleEvent1 -= restoreAlpha; 
    }

    void darkenAlpha()
    {
        Color alphaColor = sr.color;
        alphaColor.a = 1.0f; //assign value to the variable alpha color.
        sr.color = alphaColor; //assign the value to the color to the variable. 
        
        //was missing this line of code and researched this to find the solution
        //https://forum.unity.com/threads/darken-effect-how-to-achieve-this.430586/
    }

    void restoreAlpha()
    {
        Color alphaColor = sr.color;
        alphaColor.a = 0.0f;
        sr.color = alphaColor; 
    }   


}
