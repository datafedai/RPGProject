using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
//using UnityEngine.UI;

public class ClickableSprite : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;

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

    void OnMouseDown()
    {
        //Debug.Log("OnMouseDown starts");
        // This function is called when the mouse button is pressed over the collider
        //Debug.Log(gameObject.name + " is selected to attack.");

            if (turnManager.getcurrentPlayerIndex() <= 3)
            {
                choice = enemies;
            }
            else if (turnManager.getcurrentPlayerIndex() >= 4)
            {
                choice = friends;
            }

            //if (choice.Contains(gameObject.name) && turnManager.gameState == GameState.AwaitingInput)
            if (choice.Contains(gameObject.name) && turnManager.readyToClick == true)
            {
                turnManager.handleAwaitingInputPhase(gameObject.name);
            }
            else
            {
                Debug.Log("invalid choice. choose a right enemy");
            }
        
        //Debug.Log("OnMouseDown ends");

    }

    void Start()
    {
        //Debug.Log("ClickableSprite, Start executed.");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
