using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.UIElements; // Required for List


[Serializable]
public class CharacterData
{
    public string character_name;
    public bool is_character_active;
    public int character_speed;
    public int character_health;
    public int character_attack_power;
    public Position character_position;
}


public class Character : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;
    private string characterName;
    private bool isActive;
    private int speed;
    private int health;
    private int attackPower;
    private Position characterPosition;

    public List<CharacterData> characterDataList;
    public List<string> characterNames;
    private List<int> speedList;
    private List<int> powerList;


    void Die()
    {

    }

    void MakeActive(CharacterData eachCharacterEntry)
    {
        Debug.Log("Name: " + eachCharacterEntry.character_name + ", "
        + "isActive? " + eachCharacterEntry.is_character_active + ", "
        + "Speed: " + eachCharacterEntry.character_speed + ", "
        + "Position: " + eachCharacterEntry.character_position + ", "
        + "Position: " + (1+(int)eachCharacterEntry.character_position) + ", "
        + "Health: " + eachCharacterEntry.character_health + ", "
        + "AttackPower: " + eachCharacterEntry.character_attack_power);
    }


    List<int> getSpeed(int minSpeed, int maxSpeed)
    {
        List<int> speedValues = new List<int>(8);

        for(int i = 0; i < 8; i++)
        {
            speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
            speedValues.Add(speed);
        }

        speedValues.Sort((x,y) => y.CompareTo(x));
        return speedValues;
    }

    void printSpeedValues(List<int> speedValues)
    {
        for(int i = 0; i < 8; i++)
        Debug.Log("speed values: " + speedList[i]);             
    }

    List<int> getAttackPower(int minPower, int maxPower)
    {
        List<int> powerValues = new List<int>(8);

        for(int i = 0; i < 8; i++)
        {
            attackPower = UnityEngine.Random.Range(minPower, maxPower);
            powerValues.Add(attackPower);
        }

        powerValues.Sort((x,y) => x.CompareTo(y));
        return powerValues;
    }

    void printPowerValues(List<int> powerValues)
    {
        for(int i = 0; i < 8; i++)
        Debug.Log("power values: " + powerValues[i]);             
    }    

    void loadCharacterData()
    {
        speedList = getSpeed(30, 100);
        printSpeedValues(speedList);

        powerList = getAttackPower(30, 100);
        printPowerValues(powerList);
        
        if (characterNames != null)
        {
            // loop each character of characterNames list
            for (int i = 0; i < characterNames.Count; i++)
            {
                // populate characterName, speed, and characterPosition variables
                characterName = characterNames[i];
                //speed = UnityEngine.Random.Range(30, 100);
                speed = speedList[i];
                //attackPower = UnityEngine.Random.Range(30, 100);
                attackPower = powerList[i];

                characterPosition = (Position)(i); // saved as Friend_North, Friend_East, ...

                // create a list of type CharacterData to contain character data into
                if (characterDataList == null)
                {
                    characterDataList = new List<CharacterData>();
                }


                // add character data to list
                characterDataList.Add(new CharacterData
                {
                    character_name = characterName,
                    is_character_active = isActive,
                    character_speed = speed,
                    character_position = characterPosition,
                    character_health = health,
                    character_attack_power = attackPower
                });

            }
        }
        else
        {
            Debug.Log("sprite not found!");
        }

        // sort by speed in descending order
        characterDataList.Sort((x, y) => y.character_speed.CompareTo(x.character_speed));
        turnManager.sendSortedCharacterData(characterDataList);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Debug.Log("Characters script, Start executed.");
        Debug.Log("GameState in Character.cs: " + turnManager.gameState);
        // initialize data
        isActive = false;
        health = 100;
        //attackPower = 50;

        characterNames = new List<string>()
        {
          "Sword Man", "Spear Soldier", "Hammer Man", "Brown Horse",
          "Green Eyes", "Black Horse", "Sword Pirate", "Green Sword"
        };

        // call MainFunction to populate data into characterDataList
        loadCharacterData();

    }

    // Update is called once per frame
    void Update()
    {

    }

}


