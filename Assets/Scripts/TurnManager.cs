using System;
using System.Collections.Generic;
using JetBrains.Annotations;
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
    //MyEnum value = (MyEnum)someInt;
    // Position value = (Position)someInt;
    /*
    public static List<string> character_names = new List<string>()
        {
          "Sword Man", "Spear Soldier", "Hammer Man", "Brown Horse", 
          "Green Eyes", "Black Horse", "Sword Pirate", "Green Sword"  
        };
    */


    
    public Position characterPositions;



    void handleAwaitingInputPhase()
    {
        // pos: nsew
        // public enum positions: friendly nwse, enemy nwse
        //Debug.Log(Enum.GetValues(typeof(Position)));
        foreach(Position each in Enum.GetValues(typeof(Position)))
        {
            //Debug.Log(each);
        }



    }


    void executePlayerCommand()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        handleAwaitingInputPhase();




    }

    // Update is called once per frame
    void Update()
    {

    }
}
