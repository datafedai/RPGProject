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

[Serializable]
public class PlayOrderData
{
    public int playerIndex;
    public int playerSpeed;
    public float lapTime;
}

public class TurnManager : MonoBehaviour
{
    public Position characterPositions;
    public GameState gameState;
    private List<CharacterData> sortedCharacterData;
    private int currentActiveCharacterIndex;
    public int currentPlayerIndex;
    private int friendLives;
    private int enemyLives;
    private int currentOponentIndex;
    public List<PlayOrderData> playOrderDataList;
    public List<PlayOrderData> sortedPlayOrderDataList;
    private Queue<int> playOrderIndex = new Queue<int>();

    public int getcurrentPlayerIndex()
    {
        return currentPlayerIndex;
    }

    public void sendSortedCharacterData(List<CharacterData> characters)
    {
        sortedCharacterData = characters;
        //dataReceived = true;

        // generatePlayOrder
        generatePlayOrder();

        gameState = GameState.AwaitingInput;

        //Debug.Log("sendSortedCharacterDate successful.");
        //Debug.Log("GameState in TurnManager.cs: " + gameState);
    }

    void generatePlayOrder()
    {
        Debug.Log("Generating play order ...");
        int player_index;
        int player_speed;
        float lap_time;
        int trackLength = 701;
        int numLaps = 6;

        // lap time on each loop of distance 701 of 6 loops
        // example of track distances: 701, 3457, 23
        for (int j = trackLength; j <= trackLength * numLaps; j = j + trackLength)
        {
            // for each character of 8
            for (int i = 0; i < 8; i++)
            {
                player_index = i;
                player_speed = sortedCharacterData[i].character_speed;

                // calculate time when a character passes the start line.
                // time = cumulative distance from the start / speed
                // if j = trackLength, the value of time is calculated for one lap.
                // if j = trackLenght + trackLength, time is calculated for 2 laps.
                // ...
                // if j = trackLength * numLaps, time is calculated for 6 laps.
                lap_time = (j * 1.0f) / sortedCharacterData[i].character_speed;
                //Debug.Log("speed: " + sortedCharacterData[i].character_speed + ", lap time: " + lap_time);

                // lap times are recorded with character index and speed
                playOrderDataList.Add(new PlayOrderData
                {
                    playerIndex = player_index,
                    playerSpeed = player_speed,
                    lapTime = lap_time
                });
            }

        }

        //Debug.Log("length of palyerOrder: " + playOrderDataList.Count);
        //playOrderDataList.Sort((x,y) => x.lapTime.CompareTo(y.lapTime));
        //printPlayOrder(playOrderDataList);

        // sort lap time data, 
        // primary order by increasing lap time: in the order that characters passed the start line until all completed 6 laps. 
        // secondary order by decreasing speed: higher speed comes ahead of lower speed when there are the same time records. 
        var sortedPlayOrderDataList = playOrderDataList.OrderBy(player => player.lapTime)
           .ThenByDescending(player => player.playerSpeed)
           .ToList();


        // output
        printSortedPlayOrder(sortedPlayOrderDataList);

        // take only indexes for playing turn order
        generatePlayOrderIndex(sortedPlayOrderDataList);
        //printSortedPlayOrderIndex();

        // if turn is never assigned, assign the first turn
        if (currentPlayerIndex == -1)
        {
            initiateTurn();
        }
    }

    void printSortedPlayOrder(List<PlayOrderData> sortedDataList)
    {

        foreach (var p in sortedDataList)
        {
            Debug.Log($"Index: {p.playerIndex}, Speed: {p.playerSpeed}, Lap Time: {p.lapTime}");
        }
    }



    void printPlayOrder(List<PlayOrderData> dataList)
    {
        for (int i = 0; i < 8 * 6; i++)
        {
            Debug.Log("index: " + dataList[i].playerIndex + ", speed: " + dataList[i].playerSpeed + ", lap_time: " + dataList[i].lapTime);
        }

    }

    public void generatePlayOrderIndex(List<PlayOrderData> sortedPlayOrderData)
    {
        for (int i = 0; i < sortedPlayOrderData.Count; i++)
        {
            //Debug.Log(sortedPlayOrderData[i].playerIndex);
            playOrderIndex.Enqueue(sortedPlayOrderData[i].playerIndex);
        }

        //Debug.Log(playOrderIndex.Count);
    }

    void printSortedPlayOrderIndex()
    {
        for (int i = 0; i < playOrderIndex.Count; i++)
        {
            if (playOrderIndex.Count > 0)
            {
                // Dequeue removes and returns the element at the front
                int currentPlayerIndex = playOrderIndex.Dequeue();
                Debug.Log($"current player index dequeued and processed: {currentPlayerIndex}. Queue count: {playOrderIndex.Count}");

                int nextPlayerIndex = playOrderIndex.Peek();
                Debug.Log($"Next Player Index to be processed (peeked): {nextPlayerIndex}. Queue count: {playOrderIndex.Count}");
            }
            else
            {
                Debug.LogWarning("Player Index queue is empty. No index to process.");
            }

        }

    }

    public void handleAwaitingInputPhase(string clickedEnemyName)
    {
        //Debug.Log("Player: " + sortedCharacterData[currentPlayerIndex].character_name);
        //printPlayOrder(playOrderDataList);

        Debug.Log(sortedCharacterData[currentPlayerIndex].character_name + " selected " + clickedEnemyName + " to attack.");
        currentOponentIndex = indexSortedCharacterData(clickedEnemyName);

        if (sortedCharacterData[currentOponentIndex].character_health > 0)
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
        currentPlayerIndex = playOrderIndex.Dequeue();
        string currentPlayerName = sortedCharacterData[currentPlayerIndex].character_name;
        Position position = sortedCharacterData[currentPlayerIndex].character_position;
        Debug.Log("Current Player: " + currentPlayerName + " at " + position);

        while (sortedCharacterData[currentPlayerIndex].character_health <= 0)
        {
            Debug.Log("skipping index: " + currentPlayerIndex + " because of no health score");
            currentPlayerIndex = playOrderIndex.Dequeue();
            currentPlayerName = sortedCharacterData[currentPlayerIndex].character_name;
            position = sortedCharacterData[currentPlayerIndex].character_position;
            Debug.Log("Revised Current Player: " + currentPlayerName + " at " + position);
        }


        if(currentPlayerIndex <= 3)
        {
            Debug.Log("Click on a character in the RIGHT side to attack.");            
        }
        else
        {
            Debug.Log("Click on a character in the LEFT side to attack."); 
        }

        int nextPlayerIndex = playOrderIndex.Peek();
        string nextPlayerName = sortedCharacterData[nextPlayerIndex].character_name;
        position = sortedCharacterData[nextPlayerIndex].character_position;

        //Debug.Log("Next Player: " + nextPlayerName);
        //Debug.Log("next player: " + nextPlayerName + " at " + position);
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
        for (int i = 0; i < sortedCharacterData.Count; i++)
        {
            // if no Health score
            if (sortedCharacterData[i].character_health <= 0)
            {
                // get the character without Health score to fight
                GameObject gameObject = GameObject.Find(sortedCharacterData[i].character_name);

                // if not already destroyed
                if (gameObject != null)
                {
                    // destroy
                    Destroy(gameObject);
                    Debug.Log(sortedCharacterData[i].character_name + " died.");
                    //buttonGreenEyes.gameObject.SetActive(false);
                }
            }
        }
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
        string enemyName = sortedCharacterData[currentOponentIndex].character_name;

        int playerAttckPower = sortedCharacterData[currentPlayerIndex].character_attack_power;
        int enemyAttackPower = sortedCharacterData[currentOponentIndex].character_attack_power;

        int playerHealth = sortedCharacterData[currentPlayerIndex].character_health;
        int enemyHealth = sortedCharacterData[currentOponentIndex].character_health;

        //Debug.Log(playerName + " : " + playerHealth + " : " + playerAttckPower);
        //Debug.Log(enemyName + " : " + enemyHealth + " : " + enemyAttackPower);
        //Debug.Log(playerName + " attacked " + enemyName);

        int newPlayerHealth = playerHealth;
        int newEnemyHealth = enemyHealth - playerAttckPower;

        sortedCharacterData[currentPlayerIndex].character_health = newPlayerHealth;
        sortedCharacterData[currentOponentIndex].character_health = newEnemyHealth;

        playerHealth = sortedCharacterData[currentPlayerIndex].character_health;
        enemyHealth = sortedCharacterData[currentOponentIndex].character_health;

        //Debug.Log(playerName + " : " + playerHealth + " : " + playerAttckPower);
        //Debug.Log(enemyName + " : " + enemyHealth + " : " + enemyAttackPower);
    }


    void handleGameFlow()
    {
        switch (gameState)
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
        currentPlayerIndex = -1;

        //printCharacterData();
    }


    // Update is called once per frame
    void Update()
    {
        if (gameState == GameState.GameOver)
        {
            return;
            
            //Debug.Log("Quitting Game!");
            //UnityEditor.EditorApplication.isPlaying = false;
        }
        else
        {
            handleGameFlow();
        }

    }
}
