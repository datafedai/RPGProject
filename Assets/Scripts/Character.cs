//using UnityEngine;


using UnityEngine;
using System;
using System.Collections.Generic; // Required for List




public class Character : MonoBehaviour
{

    private string characterName;
    private bool isActive;
    private int speed;
    public List<CharacterData> characterDataList;


    void Die()
    {

    }

    void MakeActive(CharacterData eachCharacterEntry)
    {
        Debug.Log("Name: " + eachCharacterEntry.character_name + ", "
        + "Speed: " + eachCharacterEntry.character_speed + ", "
        + "isActive? " + eachCharacterEntry.is_active);
    }


    void MainFunction()
    {
        // find sprite objects
        GameObject[] characterSprites = GameObject.FindGameObjectsWithTag("sprite");
        if (characterSprites != null)
        {

            for (int i = 0; i < characterSprites.Length; i++)
            {

                // get child of sprite for character name
                foreach (Transform childTransform in characterSprites[i].transform)
                {
                    GameObject childGameObject = childTransform.gameObject;
                    //Debug.Log("Found child: " + childGameObject.name);
                    characterName = childGameObject.name;
                }

                speed = UnityEngine.Random.Range(1, 100);
                isActive =true;

                // add data: populate List
                if (characterDataList == null)
                {
                    characterDataList = new List<CharacterData>();
                }

                // add character data to list
                characterDataList.Add(new CharacterData { character_name = characterName, is_active = isActive, character_speed = speed });

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
        MainFunction();
    }

    // Update is called once per frame
    void Update()
    {

    }


}



[Serializable]
public class CharacterData
{
    public string character_name;
    public bool is_active;
    public float character_speed;
}
