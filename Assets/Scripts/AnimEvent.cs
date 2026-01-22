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


    void HandleAnim_Idle_LOOP(int characterToAnimate)
    {
        
    }

    void HandleAnim_Ready_OS(int characterToAnimate)
    {
        index = turnManager.getcurrentPlayerIndex();

        characterAnimRefs[index].SetBool("readyOS", false);
        characterAnimRefs[index].SetBool("readyLOOP", true);      

        turnManager.gameState = GameState.AwaitingInput;
        //Debug.Log("setting readyLOOP true for " + index );
    }

    void HandleAnim_Ready_LOOP(int characterToAnimate)
    {
  
    }

    void OnReady()
    {
        Debug.Log("OnReady function starts");        
    }

    void HandleAnim_Dash_OS(int characterToAnimate)
    {
        characterAnimRefs[index].SetBool("dashOS", false);
        characterAnimRefs[index].SetBool("dashLOOP", true);   
        turnManager.gameState = GameState.OnDash;
    
        //Debug.Log("setting dashLOOP true for " + index );
    }

    void HandleAnim_Dash_LOOP(int characterToAnimate)
    {
        
    }

    void OnDash()
    {
        Debug.Log("OnDash function starts");
    }



    void HandleAnim_Attack_OS(int characterToAnimate)
    {
        characterAnimRefs[index].SetBool("attackOS", false);
        turnManager.gameState = GameState.AttackOver;
     
        //Debug.Log("setting attackOS false for " + index );  
    }

    void OnAttack()
    {
        Debug.Log("OnAttack function starts");
    }

    void HandleAnim_Backdash_OS(int characterToAnimate)
    {
        characterAnimRefs[index].SetBool("backdashOS", false);
        characterAnimRefs[index].SetBool("backdashLOOP", true);      
        turnManager.gameState = GameState.OnBackdash;

        //Debug.Log("setting backdashLOOP true for " + index );
    }

    void HandleAnim_Backdash_LOOP(int characterToAnimate)
    {

    }

    void OnBackdash()
    {
        Debug.Log("OnBackdash function starts");
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Debug.Log("AninEvent script started");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
