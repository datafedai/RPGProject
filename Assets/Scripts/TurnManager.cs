using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;



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

public enum GameState
{
    DataNotReady,
    AwaitingInput,
    AttackOn,
    InitiateTurn,
    GameOver
}


public class TurnManager : MonoBehaviour
{
    public Position characterPositions;
    public GameState gameState;
    //bool dataReceived = false;
    //bool awaitingPlayerInput = true;
    //int currentPlayerIndex = 0;
    private List<CharacterData> sortedCharacterData;
    private int currentActiveCharacterIndex;
    public int currentPlayerIndex;
    private int friendLives;
    private int enemyLives;
    private int currentEnemyIndex;

    public int getcurrentPlayerIndex()
    {
        return currentPlayerIndex;
    }

    public void sendSortedCharacterData(List<CharacterData> characters)
    {
        sortedCharacterData = characters;
        //dataReceived = true;
        gameState = GameState.AwaitingInput;

        Debug.Log("sendSortedCharacterDate successful.");
        Debug.Log("GameState in TurnManager.cs: " + gameState);
    }


    public void handleAwaitingInputPhase(string clickedEnemyName)
    {
        //Debug.Log("Player: " + sortedCharacterData[currentPlayerIndex].character_name);
        Debug.Log(sortedCharacterData[currentPlayerIndex].character_name + " selected " + clickedEnemyName + " to attack.");
        currentEnemyIndex = indexSortedCharacterData(clickedEnemyName);

        if (sortedCharacterData[currentEnemyIndex].character_health > 0)
        {
            //Debug.Log("Friend Position:Enemy Position = " + (1 + currentPlayerIndex) + ":" + (1 + currentEnemyIndex));

            gameState = GameState.AttackOn;
        }
        else
        {
            Debug.Log("The selected enemy is already dead. Choose another enemy.");
        }
    }


    private int indexSortedCharacterData(string enemyName)
    {
        for (int i = 0; i < sortedCharacterData.Count; i++)
        {
            //Debug.Log(enemyName + " : " + sortedCharacterData[i].character_name + " : " + sortedCharacterData[i].character_speed);
            if (enemyName == sortedCharacterData[i].character_name)
            {
                //Debug.Log("Clicked Enemy Index: " + i);
                //currentEnemyIndex = i;
                return i;
            }
        }

        return -1;
    }



    void initiateTurn()
    {
       // Debug.Log("previous friend player position: " + (1+currentPlayerIndex));

        //Debug.Log(sortedCharacterData[currentPlayerIndex].character_health);
        currentPlayerIndex++;
        while (sortedCharacterData[currentPlayerIndex%8].character_health <= 0)
        {
            Debug.Log("skipping friend at position " + (1+currentPlayerIndex%8) + " because of no health score");
            currentPlayerIndex++;
        }


        currentPlayerIndex = currentPlayerIndex % 8;
        //Debug.Log("Next Friend Player Position and Health: " + (1 + currentPlayerIndex) 
        //+ " : " + sortedCharacterData[currentPlayerIndex].character_health);
        Debug.Log("Next Player: " + sortedCharacterData[currentPlayerIndex].character_name);
        gameState = GameState.AwaitingInput;
    }

    void printHealthData(string timePoint)
    {

        Debug.Log("Health " + timePoint + " : " + sortedCharacterData[0].character_health + " : " + sortedCharacterData[1].character_health + " : "
        + sortedCharacterData[2].character_health + " : " + sortedCharacterData[3].character_health + " : " + sortedCharacterData[4].character_health
        + " : " + sortedCharacterData[5].character_health + " : " + sortedCharacterData[6].character_health + " : " + sortedCharacterData[7].character_health);

    }

    void printAttackPowerData()
    {

        Debug.Log("Character AttackPower: " + sortedCharacterData[0].character_attack_power + " : " + sortedCharacterData[1].character_attack_power + " : "
        + sortedCharacterData[2].character_attack_power + " : " + sortedCharacterData[3].character_attack_power + " : " + sortedCharacterData[4].character_attack_power
        + " : " + sortedCharacterData[5].character_attack_power + " : " + sortedCharacterData[6].character_attack_power + " : " + sortedCharacterData[7].character_attack_power);

    }
    void printCharacterData()
    {
        for (int i = 0; i < sortedCharacterData.Count; i++)
        {
            currentActiveCharacterIndex = i;
            Debug.Log("Name: " + sortedCharacterData[currentActiveCharacterIndex].character_name + ", "
            + "isActive? " + sortedCharacterData[currentActiveCharacterIndex].is_character_active + ", "
            + "Speed: " + sortedCharacterData[currentActiveCharacterIndex].character_speed + ", "
            + "Position: " + sortedCharacterData[currentActiveCharacterIndex].character_position + ", "
            + "Position: " + (1 + (int)sortedCharacterData[currentActiveCharacterIndex].character_position) + ", "
            + "Health: " + sortedCharacterData[currentActiveCharacterIndex].character_health + ", "
            + "AttackPower: " + sortedCharacterData[currentActiveCharacterIndex].character_attack_power);

            //gameState = GameState.Playing;
        }
    }
    void executePlayerCommand()
    {

    }

    void handleNoHealth()
    {
        // for each character
        for(int i = 0; i < sortedCharacterData.Count; i++)
        {
            // if not Health score
            if(sortedCharacterData[i].character_health <= 0)
            {
                // get the character without Health score to fight
                GameObject gameObject = GameObject.Find(sortedCharacterData[i].character_name);

                // if not already destroyed
                if(gameObject != null)
                {
                    // destroy
                    Destroy(gameObject);
                    Debug.Log(sortedCharacterData[i].character_name + " died.");
                    //buttonGreenEyes.gameObject.SetActive(false);
                }
            }
        }
    }

    void choosePlayer()
    {
        string playerName = "HHH";
        Debug.Log("Player: " + playerName);

        //gameState = GameState.AttackOn;
    }

    void handleAttack()
    {
        //Debug.Log("before attack");
        //printCharacterData();
        //printHealthData("before attack");
        printAttackPowerData();

        updateHealth();

        //Debug.Log("after attack");
        printHealthData("after attack");
        //printCharacterData();

        // destroy a character if no health
        handleNoHealth();

        gameState = GameState.InitiateTurn;

        // update remaining lives
        // if no lives left on eather team, game is over
        updateLives();
        //checkForWin();
        //Debug.Log(getWinner() + " won the game");
    }


    private void announceWin()
    {
        if (friendLives == 0 && enemyLives == 0)
        {
            Debug.Log("Tie.");
        }
        else if (enemyLives == 0)
        {
            Debug.Log("Friend won.");
        }
        else
        {
            Debug.Log("Enemy won.");
        }
    }


    void updateLives()
    {
        //Debug.Log("updating remaining lives.");
        friendLives = 4;
        enemyLives = 4;

        // for each character
        for (int i = 0; i < sortedCharacterData.Count; i++)
        {
            // if friend has no Health score
            if (i < 4 && sortedCharacterData[i].character_health <= 0)
            {
                // decrease the number of alive lives
                friendLives--;
            }
            // for enemy
            else if (i > 3 && sortedCharacterData[i].character_health <= 0)
            {
                enemyLives--;
            }
        }

        Debug.Log("Friend Lives:Enemy Lives = " + friendLives + ":" + enemyLives);
        if (enemyLives * friendLives == 0)
        {
            announceWin();
            gameState = GameState.GameOver;
        }

    }


    void updateHealth()
    {
        // update Health score after attack
        //Debug.Log("Updating Health score after attack");
        string playerName = sortedCharacterData[currentPlayerIndex].character_name;
        string enemyName = sortedCharacterData[currentEnemyIndex].character_name;

        int playerAttckPower = sortedCharacterData[currentPlayerIndex].character_attack_power;
        int enemyAttackPower = sortedCharacterData[currentEnemyIndex].character_attack_power;

        int playerHealth = sortedCharacterData[currentPlayerIndex].character_health;
        int enemyHealth = sortedCharacterData[currentEnemyIndex].character_health;

        //Debug.Log(playerName + " : " + playerHealth + " : " + playerAttckPower);
        //Debug.Log(enemyName + " : " + enemyHealth + " : " + enemyAttackPower);
        //Debug.Log(playerName + " attacked " + enemyName);

        int newPlayerHealth = playerHealth;
        int newEnemyHealth = enemyHealth - playerAttckPower;

        sortedCharacterData[currentPlayerIndex].character_health = newPlayerHealth;
        sortedCharacterData[currentEnemyIndex].character_health = newEnemyHealth;

        playerHealth = sortedCharacterData[currentPlayerIndex].character_health;
        enemyHealth = sortedCharacterData[currentEnemyIndex].character_health;

        //Debug.Log(playerName + " : " + playerHealth + " : " + playerAttckPower);
        //Debug.Log(enemyName + " : " + enemyHealth + " : " + enemyAttackPower);
    }


    void handleGameFlow()
    {
        switch(gameState)
        {
            case GameState.DataNotReady:
            break;

            case GameState.AwaitingInput:
            //choosePlayer();
            break;

            case GameState.AttackOn:
            handleAttack();
            break;

            case GameState.InitiateTurn:
            initiateTurn();
            break;

            default:
            break;
        }
    }





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Debug.Log("Manager script Start, executed.");
        Debug.Log("GameState in TurnManager.cs: " + gameState);

        currentActiveCharacterIndex = 0;
        currentPlayerIndex = 0;

        //printCharacterData();
    }


    // Update is called once per frame
    void Update()
    {
        if(gameState == GameState.GameOver)
        {
            Debug.Log("Quitting Game!");
            UnityEditor.EditorApplication.isPlaying = false;
        }
        else
        {
            handleGameFlow();            
        }

    }
}
