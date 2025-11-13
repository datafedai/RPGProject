//using UnityEngine;


using UnityEngine;
using System;
using System.Collections.Generic; // Required for List




public class Character : MonoBehaviour
{

    private string characterName;
    private bool isActive;
    private float speed;
    public List<CharacterData> characterDataList;


    void Die()
    {

    }

    void MakeActive()
    {
        // find sprite objects
        GameObject[] characterSprites = GameObject.FindGameObjectsWithTag("sprite");
        if (characterSprites != null)
        {

            for (int i = 0; i < characterSprites.Length; i++)
            {
  
                //Debug.Log("sprite transform: " + characters[i].transform);
                foreach (Transform childTransform in characterSprites[i].transform)
                {
                    GameObject childGameObject = childTransform.gameObject;
                    //Debug.Log("Found child: " + childGameObject.name);
                    characterName = childGameObject.name;
                }

                //characterDataEntries.Add(new CharacterDataEntry { characterName = characters[i].name, isActive = true, speed = 10 });
                //Debug.Log(characters[i].name);                
                speed = UnityEngine.Random.Range(1f, 100f);
                isActive = true;
                //Debug.Log("Character name: " + characterName + " : " + i);
                //Debug.Log("Speed: " + speed);


                // add data: populate List
                if (characterDataList == null)
                {
                    characterDataList = new List<CharacterData>();
                }

                // Example of adding data
                //dataList.Add(new MyCustomData { name = "Item 1", value = 10 });
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
        foreach (CharacterData entry in characterDataList)
        {
            Debug.Log("Name: " + entry.character_name + ", " 
            + "Speed: " + Mathf.Round(entry.character_speed * 10.0f) * 0.1f + ", "
            + "isActive? " + entry.is_active);
        }


    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MakeActive();
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
