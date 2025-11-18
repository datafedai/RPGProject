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
    public string character_position;


}


public class Character : MonoBehaviour
{
    private string characterName;
    private bool isActive = false;
    private int speed = 10;
    private int health = 100;
    private int attackPower = 50;
    private string position;
    public List<CharacterData> characterDataList;
    private List<string> characterNames;
    TurnManager managerScript;


    void Die()
    {

    }

    void MakeActive(CharacterData eachCharacterEntry)
    {
        Debug.Log("Name: " + eachCharacterEntry.character_name + ", "
        + "isActive? " + eachCharacterEntry.is_character_active + ", "
        + "Speed: " + eachCharacterEntry.character_speed + ", "
        + "Position: " + eachCharacterEntry.character_position + ", "
        + "Health: " + eachCharacterEntry.character_health + ", "
        + "AttackPower: " + eachCharacterEntry.character_attack_power);

        //GameState currentState = GameState.Playing;
        //int stateIndex = (int)currentState; // stateIndex will be 1
        string p = eachCharacterEntry.character_position;
        Debug.Log("Test: " + (int)Position.Enemy_East);
    }


    void MainFunction()
    {
        // find sprite objects, total 8
        //GameObject[] characterSprites = GameObject.FindGameObjectsWithTag("sprite");


        //Debug.Log("child of characterSprites[0]: " + characterSprites[0].transform.GetChild(0).name);
        if (characterNames != null)
        {
            // loop each sprite of 8 to get character name and random speed value
            for (int i = 0; i < characterNames.Count; i++)
            {
                // get child name from sprite transform for each sprite
                characterName = characterNames[i];

                // or below
                /*
                foreach (Transform childTransform in characterSprites[i].transform)
                {
                    GameObject childGameObject = childTransform.gameObject;
                    //Debug.Log("Found child: " + childGameObject.name);
                    characterName = childGameObject.name;
                }
                */


                speed = UnityEngine.Random.Range(1, 100);
                isActive = true;

                managerScript = GetComponent<TurnManager>();
                //Debug.Log("Test: " + TurnManager.character_names[i]);

                managerScript.characterPositions = (Position)(i);
                //Debug.Log("for managerScript.characterPos: " + managerScript.characterPositions);
                position = managerScript.characterPositions.ToString();
                //Debug.Log("position test: " + position);


                // add data: populate List
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
                    character_position = position,
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
        characterNames = new List<string>()
        {
          "Sword Man", "Spear Soldier", "Hammer Man", "Brown Horse",
          "Green Eyes", "Black Horse", "Sword Pirate", "Green Sword"
        };

        MainFunction();
    }

    // Update is called once per frame
    void Update()
    {

    }


}


