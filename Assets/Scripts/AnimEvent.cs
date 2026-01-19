using UnityEngine;
//using System;
//using System.Collections;
using System.Collections.Generic;
//using System.Linq.Expressions;
//using JetBrains.Annotations;
//using Unity.VisualScripting;
//using UnityEditor.Rendering;

//using UnityEngine.UI;
//using System.Linq;
//using UnityEngine.Rendering;
//using System.Reflection;

public class AnimEvent : MonoBehaviour
{

    [SerializeField] private TurnManager turnManager;
    [SerializeField] List<Animator> characterAnimRefs;
    private int index;


    public void PrintEventMessage(string message)
    {
        Debug.Log("Event triggered: " + message);
    }



    void HandleAnim_Idle_LOOP(int characterToAnimate)
    {
        
    }

    void HandleAnim_Ready_OS(int characterToAnimate)
    {
        index = turnManager.getcurrentPlayerIndex();
        characterAnimRefs[index].SetBool("readyOS", false);
        characterAnimRefs[index].SetBool("readyLOOP", true);      
        Debug.Log("setting readyLOOP true for " + index );

    }

    void HandleAnim_Ready_LOOP(int characterToAnimate)
    {
  
    }

    void HandleAnim_Dash_OS(int characterToAnimate)
    {
        //int i = turnManager.getcurrentPlayerIndex();
        characterAnimRefs[index].SetBool("dashOS", false);
        characterAnimRefs[index].SetBool("dashLOOP", true);      
        Debug.Log("setting dashLOOP true for " + index );
    }

    void HandleAnim_Dash_LOOP(int characterToAnimate)
    {
        
    }

    void HandleAnim_Attack_OS(int characterToAnimate)
    {
        //int i = turnManager.getcurrentPlayerIndex();
        characterAnimRefs[index].SetBool("attackOS", false);
        characterAnimRefs[index].SetBool("backdashOS", true);      
        Debug.Log("setting backdashOS true for " + index );
        
    }

    void HandleAnim_Backdash_OS(int characterToAnimate)
    {
        //int i = turnManager.getcurrentPlayerIndex();
        characterAnimRefs[index].SetBool("backdashOS", false);
        characterAnimRefs[index].SetBool("backdashLOOP", true);      
        Debug.Log("setting backdashLOOP true for " + index );
    }

    void HandleAnim_Backdash_LOOP(int characterToAnimate)
    {
        characterAnimRefs[index].SetBool("backdashLOOP", false);      
        Debug.Log("setting backdashLOOP false for " + index );
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("AninEvent script started");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
