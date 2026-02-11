using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
//using UnityEngine.UI;




public class ClickableSprite : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private CombatReadinessBar comabtReadinessBar;
    //public event Action<int> OnPlayerSelectedEnemyToAttack; // event declaration for enemy selected to attack

    bool validChoice;
    private HashSet<string> friends = new HashSet<string>
        {
          "Sword Man", "Spear Soldier", "Hammer Man", "Brown Horse"
        };

    private HashSet<string> enemies = new HashSet<string>
        {
          "Green Eyes", "Black Horse", "Sword Pirate", "Green Sword"
        };

    private HashSet<string> choice;


    private void OnEnable()
    {
        //turnManager.OnPlayerSelectedEnemyToAttack += EnemySelected;
    } 

    private void OnDisable()
    {
        //turnManager.OnPlayerSelectedEnemyToAttack -= EnemySelected;
    }


    void EnemySelected(int index)
    {
        Debug.Log("///////////////////////////////////////////////////////////////////////");
        Debug.Log("in TurnManager, enemy selected in ClickableSprite.cs, index: " + index);
        Debug.Log("////////////////////////////////////////////////////////////////////////");
    }

    void OnMouseDown()
    {
        //Debug.Log("OnMouseDown starts");
        // This function is called when the mouse button is pressed over the collider
        //Debug.Log(gameObject.name + " is selected to attack.");
        int idx = turnManager.GetcurrentPlayerIndex();

        if (idx <= 3)
        {
            choice = enemies;
        }
        else if (idx >= 4)
        {
            choice = friends;
        }

        if (choice.Contains(gameObject.name) && turnManager.gameState == GameState.AwaitingInput && comabtReadinessBar.IsCombatReadinessBarUpdated())
        //if (choice.Contains(gameObject.name) && turnManager.readyToClick == true)
        {
            //turnManager.handleAwaitingInputPhase(gameObject.name);
            Debug.Log("Clicked " + gameObject.name);
            //OnPlayerSelectedEnemyToAttack?.Invoke(idx);
            turnManager.handleInput(gameObject.name);

        }
        else
        {
            Debug.Log("invalid choice. choose a right enemy");
        }

        //Debug.Log("OnMouseDown ends");

    }

    void Start()
    {
        Debug.Log("ClickableSprite, Start executed.");
        //OnPlayerSelectedEnemyToAttack?.Invoke(99);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
