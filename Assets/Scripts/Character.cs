using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering; // Required for List


[Serializable]
public class CharacterData
{
    public string character_name;
    public bool is_active;
    public float character_speed;
    public int position;


}


public class Character : MonoBehaviour
{
    private string characterName;
    private bool isActive;
    private int speed;
    public List<CharacterData> characterDataList;
    private int position;
    private int health = 100;
    private int attckPower = 50;
    private List<string> characterNames;


    void Die()
    {

    }

    void MakeActive(CharacterData eachCharacterEntry)
    {
        Debug.Log("Name: " + eachCharacterEntry.character_name + ", "
        + "Speed: " + eachCharacterEntry.character_speed + ", "
        + "isActive? " + eachCharacterEntry.is_active + ", "
        + "Position: " + eachCharacterEntry.position);
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
                isActive =true;

                // add data: populate List
                if (characterDataList == null)
                {
                    characterDataList = new List<CharacterData>();
                }

                position = i + 1;
                // add character data to list
                characterDataList.Add(new CharacterData 
                   { character_name = characterName, is_active = isActive, character_speed = speed, position = position });

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


