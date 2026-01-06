using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using System.Runtime.CompilerServices; // Required for List
using System.Linq; // Required for LINQ


[Serializable]
public class CharacterData
{
    // Identification
    public int character_index;
    public string character_name;
    // Attributes
    public int character_speed;
    public int character_health;
    public int character_attack_power;
    // Status
    public bool is_character_alive;
    public Position character_position; // North, East, South, West, Friendly/Enemy, denoted by enums 0-7
}

[Serializable]
public class SpeedPower
{
    public int speed;
    public int power;
}

public class Character : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;

    public const int NumCharacters = 8;
    private int characterIndex;
    private string characterName;
    private bool isAlive;
    private int speed;
    private int health;
    private int attackPower;
    private Position characterPosition;

    public List<CharacterData> characterDataList;

    private List<int> speedList;
    private List<int> powerList;
    private List<int> randomNumList;
    public List<SpeedPower> speedPowerList;
    public List<string> characterNames;


    void Die()
    {

    }

    public void printCharacterData(List<CharacterData> characterDataList)
    {
        foreach (var eachCharacterEntry in characterDataList)
        {
            Debug.Log("Index: " + eachCharacterEntry.character_index + ", "
            + "Name: " + eachCharacterEntry.character_name + ", "
            + "isAlive? " + eachCharacterEntry.is_character_alive + ", "
            + "Speed: " + eachCharacterEntry.character_speed + ", "
            + "Position: " + eachCharacterEntry.character_position + ", "
            + "Health: " + eachCharacterEntry.character_health + ", "
            + "AttackPower: " + eachCharacterEntry.character_attack_power);
        }

    }


    List<int> getSpeed(int minSpeed, int maxSpeed)
    {
        List<int> speedValues = new List<int>(8);

        for (int i = 0; i < NumCharacters; i++)
        {
            speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
            speedValues.Add(speed);
        }

        speedValues.Sort((x, y) => y.CompareTo(x));
        return speedValues;
    }

    void printSpeedValues(List<int> speedValues)
    {
        for (int i = 0; i < 8; i++)
            Debug.Log("speed values: " + speedList[i]);
    }

    List<int> getUniqueRandomNumbers(int minNum, int maxNum)
    {
        List<int> valuesList = new List<int>(8);
        int value;

        for (int i = 0; i < 8; i++)
        {
            do
            {
                value = UnityEngine.Random.Range(minNum, maxNum + 1);
            } while (valuesList.Contains(value));

            valuesList.Add(value);
        }

        return valuesList;
    }



    List<int> getAttackPower(int minPower, int maxPower)
    {
        List<int> powerValues = new List<int>(8);

        for (int i = 0; i < 8; i++)
        {
            attackPower = UnityEngine.Random.Range(minPower, maxPower);
            powerValues.Add(attackPower);
        }

        powerValues.Sort((x, y) => x.CompareTo(y));
        return powerValues;
    }

    void printPowerValues(List<int> powerValues)
    {
        for (int i = 0; i < 8; i++)
            Debug.Log("power values: " + powerValues[i]);
    }

    public void loadCharacterData()
    {
        //Debug.Log("loadingCharacterData starts");
        speedList = getUniqueRandomNumbers(20, 80);
        speedList.Sort((x, y) => y.CompareTo(x));
        //printSpeedValues(speedList);

        powerList = getUniqueRandomNumbers(10, 30);
        powerList.Sort((x, y) => x.CompareTo(y));
        //printPowerValues(powerList);

        // combine random speed(descending) and random power(ascending) 
        for(int k = 0; k < 8; k++)
        {
            speedPowerList.Add(new SpeedPower
            {
                speed = speedList[k],
                power = powerList[k]
            });
        }

        // shuffle speedPowerList
        List<SpeedPower> shuffledSpeedPowerList = speedPowerList.OrderBy(i => System.Guid.NewGuid()).ToList();



        if (characterNames != null)
        {
            characterDataList.Clear();
            // loop each character of characterNames list
            for (int i = 0; i < characterNames.Count; i++)
            {
                // populate characterName, speed, and characterPosition variables
                characterIndex = i;
                characterName = characterNames[i];
                //speed = UnityEngine.Random.Range(30, 100);
                //speed = speedList[i];
                //attackPower = UnityEngine.Random.Range(30, 100);
                //attackPower = powerList[i];
                speed = shuffledSpeedPowerList[i].speed;
                attackPower = shuffledSpeedPowerList[i].power; 

                characterPosition = (Position)(i); // saved as Friend_North, Friend_East, ...

                // create a list of type CharacterData to contain character data into
                if (characterDataList == null)
                {
                    characterDataList = new List<CharacterData>();
                }


                // add character data to list
                characterDataList.Add(new CharacterData
                {
                    character_index = characterIndex,
                    character_name = characterName,
                    is_character_alive = isAlive,
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
        characterDataList.Sort((x, y) => x.character_index.CompareTo(y.character_index));

        //Debug.Log("size of character data: " + characterDataList.Count);
        //printCharacterData(characterDataList);

        // send character data to Turnmanager
        //Debug.Log("Sending character data from Character to Turnmanager");
        //turnManager.getCharacterData2(characterDataList);

        //Debug.Log("loadingCharacterData ends");
    }

    public List<CharacterData> getCharacterDataList()
    {
        //Debug.Log("1, size of characterDataList: " + characterDataList.Count);
        if(characterDataList.Count == 0)
        {
            loadCharacterData();
            //Debug.Log("2, size of characterDataList: " + characterDataList.Count);
            return characterDataList;
        }
        else
        {
            //Debug.Log("3, size of characterDataList: " + characterDataList.Count);
            return characterDataList;
        }



        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Debug.Log("Character, Start executed.");
        //Debug.Log("GameState in Character.cs: " + turnManager.gameState);
        // initialize data
        isAlive = true;
        health = 100;
        //attackPower = 50;
        characterNames = new List<string>()
        {
          "Sword Man", "Spear Soldier", "Hammer Man", "Brown Horse",
          "Green Eyes", "Black Horse", "Sword Pirate", "Green Sword"
        };

        // call MainFunction to populate data into characterDataList
        //loadCharacterData();

    }

    // Update is called once per frame
    void Update()
    {

    }

}


