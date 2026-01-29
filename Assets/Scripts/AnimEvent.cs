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
    private List<CharacterData> sortedCharacterList;
    [SerializeField] private Character character;
    int index;




    private void OnEnable()
    {
        turnManager.OnCharacterTurnStarted += HandleAnim_Ready_OS;
        turnManager.OnPlayerSelectedEnemyToAttack += HandleAnim_Dash_OS;
        turnManager.OnCharacterDashFinished += HandleAnim_Attack_OS;
        turnManager.OnStatsUpdatedAfterAttack += HandleAnim_Backdash_OS;
        turnManager.OnCharacterBackdashFinished += HandleAnim_Idle_LOOP;
    }

    private void OnDisable()
    {
        turnManager.OnCharacterTurnStarted -= HandleAnim_Ready_OS;
        turnManager.OnPlayerSelectedEnemyToAttack -= HandleAnim_Dash_OS;
        turnManager.OnCharacterDashFinished -= HandleAnim_Attack_OS;
        turnManager.OnStatsUpdatedAfterAttack -= HandleAnim_Backdash_OS;        
        turnManager.OnCharacterBackdashFinished -= HandleAnim_Idle_LOOP;
    }



    // executed when invoked by event: 
    // OnCharacterBackdashFinished
    void HandleAnim_Idle_LOOP(int characterToAnimate)
    {
        characterAnimRefs[characterToAnimate].SetBool("backdashLOOP", false);
        characterAnimRefs[characterToAnimate].SetBool("idleLOOP", true);        
    }


    // executed when invoked by event: 
    // OnCharacterTurnStarted
    void HandleAnim_Ready_OS(int characterToAnimate)
    {
        index = characterToAnimate;
        sortedCharacterList = character.getCharacterDataList();
        //Debug.Log("name: " + sortedCharacterList[characterToAnimate].character_name);
        //Debug.Log("HandleAnim_Idle_Loop, idleLOOP false, readyOS true, for " + gameObject.tag);
        //Debug.Log("HandleAnim_Ready_OS, idleLOOP false, readyOS true"); 

        //Debug.Log("before, value of readyOS: " + characterAnimRefs[characterToAnimate].GetBool("readyOS"));  
        characterAnimRefs[characterToAnimate].SetBool("idleLOOP", false);
        characterAnimRefs[characterToAnimate].SetBool("readyOS", true); 
        //Debug.Log("after, value of readyOS: " + characterAnimRefs[characterToAnimate].GetBool("readyOS"));         
    }

    // executed after readyOS animation by animimation event
    void HandleAnim_Ready_LOOP()
    {
        characterAnimRefs[index].SetBool("readyOS", false);
        characterAnimRefs[index].SetBool("readyLOOP", true); 
        turnManager.gameState = GameState.AwaitingInput;
        //Debug.Log("setting readyLOOP true for " + index ); 
    }


    void OnReady()
    {
        Debug.Log("OnReady function starts");        
    }

    // executed when invoked by event: 
    // OnPlayerSelectedEnemyToAttack
    void HandleAnim_Dash_OS(int characterToAnimate)
    {
        characterAnimRefs[characterToAnimate].SetBool("readyLOOP", false);
        characterAnimRefs[characterToAnimate].SetBool("dashOS", true);   
        //Debug.Log("setting dashLOOP true for " + index );
    }

    // executed after readyOS animation by animimation event
    void HandleAnim_Dash_LOOP()
    {
        characterAnimRefs[index].SetBool("dashOS", false);
        characterAnimRefs[index].SetBool("dashLOOP", true);         
        turnManager.gameState = GameState.OnDash;
    }


    void OnDash()
    {
        Debug.Log("OnDash function starts");
    }


    // executed when invoked by event: 
    // OnCharacterDashFinished
    void HandleAnim_Attack_OS(int characterToAnimate)
    {
        characterAnimRefs[characterToAnimate].SetBool("dashLOOP", false);        
        characterAnimRefs[characterToAnimate].SetBool("attackOS", true);
        //turnManager.gameState = GameState.AttackOver;
     
        //Debug.Log("setting attackOS false for " + index );  
    }

    // executed after readyOS animation by animimation event
    void HandleAnim_Attack_Over()
    {
        Debug.Log("attack is over.");
        characterAnimRefs[index].SetBool("attackOS", false);
        turnManager.gameState = GameState.AttackOver;
    }



    void OnAttack()
    {
        Debug.Log("OnAttack function starts");
    }



    // executed when invoked by event: 
    // OnStatsUpdatedAfterAttack
    void HandleAnim_Backdash_OS(int characterToAnimate)
    {
        characterAnimRefs[characterToAnimate].SetBool("backdashOS", true);
        //Debug.Log("setting backdashLOOP true for " + index );
    }

    // executed after backdashOS animation by animimation event
    void HandleAnim_Backdash_LOOP()
    {
        characterAnimRefs[index].SetBool("backdashOS", false);
        characterAnimRefs[index].SetBool("backdashLOOP", true);      
        turnManager.gameState = GameState.OnBackdash;
    }

    void OnBackdash()
    {
        Debug.Log("OnBackdash function starts");
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Debug.Log("AninEvent script started");
        turnManager.resetAnimRefs();     

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}



/*



    void HandleAnim_Idle_LOOP(int characterToAnimate)
    {
        //index = turnManager.getcurrentPlayerIndex();
        //Debug.Log("HandleAnim_Idle_Loop, idleLOOP false, readyOS true, for " + gameObject.name);
        characterAnimRefs[characterToAnimate].SetBool("idleLOOP", false);
        characterAnimRefs[characterToAnimate].SetBool("readyOS", true);        
    }

    void HandleAnim_Ready_OS(int characterToAnimate)
    {
        index = turnManager.getcurrentPlayerIndex();

        characterAnimRefs[index].SetBool("readyOS", false);
        characterAnimRefs[index].SetBool("readyLOOP", true);      

        turnManager.gameState = GameState.AwaitingInput;
        //Debug.Log("setting readyLOOP true for " + index );
    }

    void HandleAnim_Ready_LOOP(int characterToAnimate, string enemyName)
    {
        characterAnimRefs[index].SetBool("readyLOOP", false);
        characterAnimRefs[index].SetBool("dashOS", true);   
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
        characterAnimRefs[index].SetBool("dashLOOP", false);
        characterAnimRefs[index].SetBool("attackOS", true);         
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

    */