using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using System.Runtime.CompilerServices;
using Unity.Collections; // Required for List

public class CombatReadinessBar : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;
    const int combatReadinessBarMaxHeight = 8; // Max height of the combat readiness bar in Unity units.

    public void processCombatReadinessBar(List<CombatReadinessData> sortedCombatReadinessList)
    {
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
            Vector3 newPosition = new Vector3(-9f, -4f, 0f) + new Vector3(0f, mov, z);
            GameObject icon = GameObject.FindWithTag(iconTag);

            if(icon != null)
            {
                icon.transform.position = newPosition;                
            }

        }
    }

}
