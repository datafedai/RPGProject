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
public class CombatReadinessData
{
    // Important data needed for combat readiness bar
    public int playerIndex;
    public float currentPosition;
    public float heightPercentile;

    // Additional data for calculations
    public int playerSpeed;
    public float lapTime;
    public float currentTime;

}


public class CombatReadinessBar : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;
    public CombatReadinessData combatReadinessData;
    public List<CombatReadinessData> combatReadinessList;
    public List<CombatReadinessData> sortedCombatReadinessList;
    private bool isCombatReadinessBarUpdated = false;
    const int combatReadinessBarMaxHeight = 8; // Max height of the combat readiness bar in Unity units.
    public const int NumCharacters = 8;
    public const int NumLaps = 100;
    public const int TrackLength = 701;



    private void OnEnable()
    {   
        turnManager.OnPlayOrderCalculated += ProcessCombatReadiness;     
    }

    private void OnDisable()
    {
        turnManager.OnPlayOrderCalculated -= ProcessCombatReadiness; 
    }



    private void ProcessCombatReadiness(List<PlayOrderData> playOrderDataList)
    {
        Debug.Log("processCombatReadiness starts");
        combatReadinessList = new List<CombatReadinessData>();
        combatReadinessList.Clear();
        PlayOrderData playOrderData = playOrderDataList[0];
        //printPlayOrderList(playOrderDataList);

        //Debug.Log("playOderData:" + "index, " + playOrderData.playerIndex + " currentTime, " + playOrderData.lapTime + " speed, " + playOrderData.playerSpeed);

        for (int i = 0; i < NumCharacters; i++)
        {
            //Debug.Log("i: " + i);
            //Debug.Log("playOrderDataList count: " + playOrderDataList.Count);
            for (int j = 0; j < playOrderDataList.Count; j++)
            {

                if (playOrderDataList[j].playerIndex == i)
                {
                    combatReadinessData = new CombatReadinessData();
                    //Debug.Log("i, j: " + i + ", " + j);
                    combatReadinessData.playerIndex = i;
                    combatReadinessData.playerSpeed = playOrderDataList[j].playerSpeed;
                    combatReadinessData.lapTime = playOrderDataList[j].lapTime;
                    combatReadinessData.currentTime = playOrderData.lapTime;
                    float trackResetDistance = TrackLength + 1f;
                    float pos = (playOrderData.lapTime * playOrderDataList[j].playerSpeed) % trackResetDistance;

                    combatReadinessData.currentPosition = (int)pos;

                    combatReadinessData.heightPercentile = ((trackResetDistance - pos) / trackResetDistance) * 100;
                    //playOrderBar.Enqueue(playerOrderBarData);

                    combatReadinessList.Add(combatReadinessData);

                    //Debug.Log("index: lapTime: " + i + " : " + playOrderDataList[j].lapTime);
                    break;
                }

            }
        }



        //Debug.Log("Bar list count: " + combatReadinessList.Count);
        //sortedPlayOrderDataList = playOrderDataList.OrderBy(player => player.lapTime)
        //   .ThenByDescending(player => player.playerSpeed)
        //   .ToList();
        // var
        //sortedCombatReadinessList = combatReadinessList.OrderBy(player => player.playerIndex)
        sortedCombatReadinessList = combatReadinessList.OrderByDescending(player => player.currentPosition)
                .ToList();



        // move and position each number sprite
        ProcessCombatReadinessBar(sortedCombatReadinessList);
        //printCombatReadiness(sortedCombatReadinessList);
        //combatReadinessBar.Update(sortedCombatReadinessList);
        turnManager.gameState = GameState.OnReady;
        turnManager.readyToClickEnemy = true;

        //Debug.Log("processCombatReadiness ends");
    }


    private void ProcessCombatReadinessBar(List<CombatReadinessData> sortedCombatReadinessList)
    {
        //Debug.Log("processCombatReadinessBar starts");
        //Debug.Log("CRBar count: " + sortedCombatReadinessList.Count);
        //turnManager.printCombatReadiness(sortedCombatReadinessList);

        int indx;
        int z = -2; // Bottom-most icon's Z-position must be negative to avoid bug where it sometimes doesn't properly appear in front of other icons.

        for (int k = 0; k < sortedCombatReadinessList.Count; k++)
        {
            //Debug.Log(sortedCombatReadinessList[k].heightPercentile);
            indx = sortedCombatReadinessList[k].playerIndex;
            //Debug.Log("indx: " + indx);
            string iconTag = indx.ToString();
            float mov = combatReadinessBarMaxHeight*(sortedCombatReadinessList[k].heightPercentile) / 100;
            
            z = 2*k; // Z-values are spaced out by 2 to avoid z-fighting bugs.
            Vector3 newPosition = new Vector3(-8f, -4f, 0f) + new Vector3(0f, mov, z);
            GameObject icon = GameObject.FindWithTag(iconTag);

            if(icon != null)
            {
                icon.transform.position = newPosition;                
            }

        }

        isCombatReadinessBarUpdated = true;

        //Debug.Log("processCombatReadinessBar ends");
    }

    public bool IsCombatReadinessBarUpdated()
    {
        if(isCombatReadinessBarUpdated == true)
        {
            isCombatReadinessBarUpdated = false;

            return true;
        }
        else
        {
            return false;
        }
    }
}
