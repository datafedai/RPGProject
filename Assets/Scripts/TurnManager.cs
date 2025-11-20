using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;



public enum Position
{

    Friend_North,
    Friend_East,
    Friend_South,
    Friend_West,
    Enemy_North,
    Enemy_East,
    Enemy_South,
    Enemy_West
}


public class TurnManager : MonoBehaviour
{
    public Position characterPositions;
    bool dataReceived = false;
    private List<CharacterData> charactersInOrder;
    private int currentActiveCharacterIndex;

    public void sendCharacterOrderData(List<CharacterData> characters)
    {
        dataReceived = true;
        charactersInOrder = characters;
    }


    void handleAwaitingInputPhase()
    {
        /*
        foreach(Position each in Enum.GetValues(typeof(Position)))
        {
            Debug.Log(each);
        }
        */
    }

    void initiateTurn()
    {
        // enemy
        if ((int)charactersInOrder[currentActiveCharacterIndex].character_position > 3)
        {
            Debug.Log("Name: " + charactersInOrder[currentActiveCharacterIndex].character_name + ", "
            + "isActive? " + charactersInOrder[currentActiveCharacterIndex].is_character_active + ", "
            + "Speed: " + charactersInOrder[currentActiveCharacterIndex].character_speed + ", "
            + "Position: " + charactersInOrder[currentActiveCharacterIndex].character_position + ", "
            + "Position: " + (1 + (int)charactersInOrder[currentActiveCharacterIndex].character_position) + ", "
            + "Health: " + charactersInOrder[currentActiveCharacterIndex].character_health + ", "
            + "AttackPower: " + charactersInOrder[currentActiveCharacterIndex].character_attack_power);


        }
        
        if (currentActiveCharacterIndex < 7)
        {
            currentActiveCharacterIndex++;
        }



    }

    void executePlayerCommand()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentActiveCharacterIndex = 0;
        handleAwaitingInputPhase();
    }

    // Update is called once per frame
    void Update()
    {
        if (dataReceived == false)
        {
            return;
        }

        initiateTurn();
    }
}
