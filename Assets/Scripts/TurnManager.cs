using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class TurnManager : MonoBehaviour
{

    public List<string> characterNames = new List<string>()
        {
          "Sword Man", "Spear Soldier", "Hammer Man", "Brown Horse", 
          "Green Eyes", "Black Horse", "Sword Pirate", "Green Sword"  
        };
    // total 8
    public enum positions
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


    void handleAwaitingInputPhase()
    {
        // pos: nsew
        // public enum positions: friendly nwse, enemy nwse



    }


    void executePlayerCommand()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
