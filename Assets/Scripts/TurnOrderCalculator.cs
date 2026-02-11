using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Rendering;
using System.Reflection;
using System.Runtime.CompilerServices;



[Serializable]
public class PlayOrderData
{
    public int playerIndex;
    public int playerSpeed;
    public float lapTime;
}





public class TurnOrderCalculator : MonoBehaviour
{
    private List<CharacterData> sortedCharacterDataList;
    [SerializeField] private CombatReadinessBar combatReadinessBar;
    [SerializeField] private TurnManager turnManager;
    public List<PlayOrderData> playOrderList;
    [SerializeField] private Character character;
    private int previousPlayerIndex = -1;
    private int currentPlayerIndex;
    private int nextPlayerIndex;

    public const int NumCharacters = 8;
    public const int NumLaps = 100;
    public const int TrackLength = 701;

    private bool isDataReady = true;
    

    private void OnEnable()
    {
        turnManager.OnCharacterDataAquired += GetCharacterData;   
        turnManager.OnNextTurnReady += UpdatePlayOrder;
        //turnManager.OnPlayOrderCalculated += ProcessCombatReadiness;     
    }

    private void OnDisable()
    {
        turnManager.OnCharacterDataAquired -= GetCharacterData; 
        turnManager.OnNextTurnReady -= UpdatePlayOrder;
        //turnManager.OnPlayOrderCalculated -= ProcessCombatReadiness; 
    }



    // executed when invoked by event: 
    // OnCharacterDataAquired
    private void GetCharacterData(List<CharacterData> characterDataList)
    {
        Debug.Log("TurnOrderCalculator, getCharacterData starts");
        Debug.Log("Receiving character data from TurnManager");

        //if(character.getCharacterDataList() )
        //sortedCharacterList.Clear();
        //sortedCharacterDataList = character.getCharacterDataList();
        sortedCharacterDataList = characterDataList;
        //Debug.Log("sorted character data list: " + sortedCharacterList.Count);
        //dataReceived = true;

        // print character data
        //printCharacterList(characters);
        // generatePlayOrder
        //Debug.Log("Calling generatePlayOrder");
        if (sortedCharacterDataList != null)
        {
            GeneratePlayOrder();
        }
        else
        {
            Debug.Log("character data list is empty.");
        }


        //gameState = GameState.AwaitingInput;

        //Debug.Log("sendSortedCharacterDate successful.");
        //Debug.Log("GameState in TurnManager, sendSortedCharacterData: " + gameState);
        //Debug.Log("getCharacterData ends");
    }



    void GeneratePlayOrder()
    {
        //Debug.Log("generatePlayOrder starts");
        int player_index;
        int player_speed;
        float lap_time;

        playOrderList.Clear();


        // lap time on each loop of distance 701 of 6 loops
        // example of track distances: 701, 3457, 23
        for (int j = TrackLength; j <= TrackLength * NumLaps; j = j + TrackLength)
        {
            // for each character of 8
            for (int i = 0; i < NumCharacters; i++)
            {
                player_index = sortedCharacterDataList[i].character_index;
                player_speed = sortedCharacterDataList[i].character_speed;

                // calculate time when a character passes the start line.
                // time = cumulative distance from the start / speed
                // if j = trackLength, the value of time is calculated for one lap.
                // if j = trackLenght + trackLength, time is calculated for 2 laps.
                // ...
                // if j = trackLength * numLaps, time is calculated for 6 laps.
                lap_time = (j * 1.0f) / sortedCharacterDataList[i].character_speed;
                //Debug.Log("speed: " + sortedCharacterData[i].character_speed + ", lap time: " + lap_time);

                // lap times are recorded with character index and speed
                playOrderList.Add(new PlayOrderData
                {
                    playerIndex = player_index,
                    playerSpeed = player_speed,
                    lapTime = lap_time
                });
            }

        }

        //Debug.Log("length of palyerOrder: " + playOrderDataList.Count);
        //playOrderDataList.Sort((x,y) => x.lapTime.CompareTo(y.lapTime));
        //printPlayOrder(playOrderList);

        // sort lap time data, 
        // primary order by increasing lap time: in the order that characters passed the start line until all completed 6 laps. 
        // secondary order by decreasing speed: higher speed comes ahead of lower speed when there are the same time records. 
        //var 
        playOrderList = playOrderList.OrderBy(player => player.lapTime)
           .ThenByDescending(player => player.playerSpeed)
           .ToList();


        // output
        //printPlayOrder(sortedPlayOrderList);
        //Debug.Log("sortedPlayOrderList: " + sortedPlayOrderList.Count);

        currentPlayerIndex = playOrderList[0].playerIndex;
        nextPlayerIndex = playOrderList[1].playerIndex;

        // take only indexes for playing turn order
        //generatePlayOrderIndex(sortedPlayOrderList);
        //Debug.Log("length: " + sortedPlayOrderDataList.Count);
        //enqueSortedPlayOrderDataList(sortedPlayOrderList);
        //printSortedPlayOrderIndex();

        // if turn is never assigned, assign the first turn


        if (playOrderList.Count == NumCharacters * NumLaps)
        {
            //Debug.Log("sortedPlayOrderList: " + sortedPlayOrderList.Count);
            turnManager.gameState = GameState.InitiatingTurn;
        }
        else
        {
            Debug.Log("regenerating character data.");
            turnManager.gameState = GameState.GettingCharacterData;
        }

        //Debug.Log("generatePlayOrder ends");
    }



    private void UpdatePlayOrder(List<PlayOrderData> sortedPlayOrderList)
    {
        previousPlayerIndex = sortedPlayOrderList[0].playerIndex;
        sortedPlayOrderList.RemoveAt(0);
        currentPlayerIndex = sortedPlayOrderList[0].playerIndex;
        nextPlayerIndex = sortedPlayOrderList[1].playerIndex;
        playOrderList = sortedPlayOrderList;

        isDataReady = true;
    }




    public List<PlayOrderData> GetPlayOrderList()
    {
        if(isDataReady == true)
        {
            isDataReady = false;
            return playOrderList;            
        }
        else
        {
            throw new Exception("Error! Play order is not updated yet."); 
        }

    }


    public int GetPreviousCharacter()
    {        
        return previousPlayerIndex;   
    }

    public int GetCurrentCharacter()
    {        
        return currentPlayerIndex;   
    }

    public int GetNextCharacter()
    {
        return nextPlayerIndex; 
    }

    public bool IsDataReady()
    {   Debug.Log("isDataReady is " + isDataReady);
        
        return isDataReady;
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
