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
    private string characterName;
    private bool isActive;
    private int speed;
    private int health;
    private int attackPower;
    private Position characterPosition;

    public List<CharacterData> characterDataList;
    public List<string> characterNames;


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


    void MainFunction()
    {
        if (characterNames != null)
        {
            // loop each character of characterNames list
            for (int i = 0; i < characterNames.Count; i++)
            {
                // populate characterName, speed, and characterPosition variables
                characterName = characterNames[i];
                speed = UnityEngine.Random.Range(1, 100);
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

        // print character data
        foreach (CharacterData eachCharacter in characterDataList)
        {
            MakeActive(eachCharacter);
        }


    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialize data
        isActive = false;
        health = 100;
        attackPower = 50;

        characterNames = new List<string>()
        {
          "Sword Man", "Spear Soldier", "Hammer Man", "Brown Horse",
          "Green Eyes", "Black Horse", "Sword Pirate", "Green Sword"
        };

        // call MainFunction to populate data into characterDataList
        MainFunction();
    }

    // Update is called once per frame
    void Update()
    {

    }

}


