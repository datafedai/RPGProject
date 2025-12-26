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

    public void processCombatReadinessBar(List<CombatReadinessData> sortedCombatReadinessList)
    {
        //Debug.Log("CRBar count: " + sortedCombatReadinessList.Count);
        //turnManager.printCombatReadiness(sortedCombatReadinessList);

        int indx;
        int z = -3;

        for (int k = 0; k < sortedCombatReadinessList.Count; k++)
        {
            //Debug.Log(sortedCombatReadinessList[k].heightPercentile);
            indx = sortedCombatReadinessList[k].playerIndex;
            //Debug.Log("indx: " + indx);
            string iconTag = indx.ToString();
            float mov = 8*(sortedCombatReadinessList[k].heightPercentile) / 100;
            
            z = 2*k;
            Vector3 newPosition = new Vector3(-9f, -4f, 0f) + new Vector3(0f, mov, z);
            GameObject icon = GameObject.FindWithTag(iconTag);

            if(icon != null)
            {
                icon.transform.position = newPosition;                
            }

        }
    }




    public void processCombatReadinessbar2(List<CombatReadinessData> sortedCombatReadinessList)
    {
        Debug.Log("CRBar count: " + sortedCombatReadinessList.Count);
        int i = 0;
        foreach (Transform childTransform in transform)
        {

            GameObject child = childTransform.gameObject;

            // Do something with the child GameObject
            Debug.Log("Iterating child: " + child.tag);

            if (child.name == "zero")
            {
                i = 0;
            }
            if (child.name == "one")
            {
                i = 1;
            }
            if (child.name == "two")
            {
                i = 2;
            }
            if (child.name == "three")
            {
                i = 3;
            }
            if (child.name == "four")
            {
                i = 4;
            }
            if (child.name == "five")
            {
                i = 5;
            }
            if (child.name == "six")
            {
                i = 6;
            }
            if (child.name == "seven")
            {
                i = 7;
            }


            Vector3 currentPosition = child.transform.position;
            //Debug.Log("curent position: " + currentPosition);
            //Debug.Log("CRBar count: " + sortedCombatReadinessList.Count);
            for (int j = 0; j < sortedCombatReadinessList.Count; j++)
            {
                if (sortedCombatReadinessList[j].playerIndex == i)
                {
                    float mov = (sortedCombatReadinessList[j].heightPercentile) / 100;
                    Vector3 newPosition = new Vector3(-9f, -4f, 0f) + new Vector3(0f, mov * 8, 0f);
                    child.transform.position = newPosition;
                }

            }

            //Debug.Log(child.name + " percentile: " + sortedCombatReadinessList[i].heightPercentile + " , " + 8*mov);



        }

    }

}
