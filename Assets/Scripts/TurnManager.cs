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


[Serializable]
public class CombatReadinessData
{
    public int playerIndex;
    public int playerSpeed;
    public float lapTime;
    public float currentTime;
    public float currentPosition;
    public float heightPercentile;
}


public class TurnManager : MonoBehaviour
{
    public Position characterPositions;
    public GameState gameState;
    private List<CharacterData> sortedCharacterList;
    private int currentActiveCharacterIndex;
    public int currentPlayerIndex;
    private int friendLives;
    private int enemyLives;
    private int currentOponentIndex;
    public List<PlayOrderData> playOrderList;
    public List<PlayOrderData> sortedPlayOrderList;
    private Queue<PlayOrderData> playOrderDataQue;
    private Queue<int> playOrderIndex = new Queue<int>();
    //public event Action<Queue<int>> playOrderBar;
    public CombatReadinessData combatReadinessData;
    public List<CombatReadinessData> combatReadinessList;
    public List<CombatReadinessData> sortedCombatReadinessList;
    //public GameObject childObject;
    [SerializeField] private CombatReadinessBar combatReadinessBar;


    public int getcurrentPlayerIndex()
    {
        return currentPlayerIndex;
    }

    public void sendSortedCharacterData(List<CharacterData> characters)
    {
        sortedCharacterList = characters;
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
                player_index = sortedCharacterList[i].character_index;
                player_speed = sortedCharacterList[i].character_speed;

                // calculate time when a character passes the start line.
                // time = cumulative distance from the start / speed
                // if j = trackLength, the value of time is calculated for one lap.
                // if j = trackLenght + trackLength, time is calculated for 2 laps.
                // ...
                // if j = trackLength * numLaps, time is calculated for 6 laps.
                lap_time = (j * 1.0f) / sortedCharacterList[i].character_speed;
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
        //printPlayOrder(playOrderDataList);

        // sort lap time data, 
        // primary order by increasing lap time: in the order that characters passed the start line until all completed 6 laps. 
        // secondary order by decreasing speed: higher speed comes ahead of lower speed when there are the same time records. 
        //var 
        sortedPlayOrderList = playOrderList.OrderBy(player => player.lapTime)
           .ThenByDescending(player => player.playerSpeed)
           .ToList();


        // output
        printSortedPlayOrder(sortedPlayOrderList);

        // take only indexes for playing turn order
        //generatePlayOrderIndex(sortedPlayOrderList);
        //Debug.Log("length: " + sortedPlayOrderDataList.Count);
        enqueSortedPlayOrderDataList(sortedPlayOrderList);
        //printSortedPlayOrderIndex();

        // if turn is never assigned, assign the first turn
        if (currentPlayerIndex == -1)
        {
            initiateTurn();
        }
    }


    void enqueSortedPlayOrderDataList(List<PlayOrderData> sortedPlayOrderDataList)
    {
        playOrderDataQue = new Queue<PlayOrderData>();

        for (int i = 0; i < sortedPlayOrderDataList.Count; i++)
        {
            //Debug.Log("hello");
            playOrderDataQue.Enqueue(sortedPlayOrderDataList[i]);
        }

        //Debug.Log("queued playOrderDataList length: " + playOrderDataListQue.Count);

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

        Debug.Log(sortedCharacterList[currentPlayerIndex].character_name + " selected " + clickedEnemyName + " to attack.");
        currentOponentIndex = indexSortedCharacterData(clickedEnemyName);

        if (sortedCharacterList[currentOponentIndex].character_health > 0)
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
        for (int i = 0; i < sortedCharacterList.Count; i++)
        {
            //Debug.Log(enemyName + " : " + sortedCharacterData[i].character_name + " : " + sortedCharacterData[i].character_speed);
            if (enemyName == sortedCharacterList[i].character_name)
            {
                //Debug.Log("Clicked Enemy Index: " + i);
                //currentEnemyIndex = i;
                return i;
            }
        }

        return -1;
    }


    void printPlayOrderList(List<PlayOrderData> _playOrderList)
    {
        Debug.Log("Printing play order list data ....");
        for (int i = 0; i < _playOrderList.Count; i++)
        {
            int _index = _playOrderList[i].playerIndex;
            int _speed = _playOrderList[i].playerSpeed;
            float _lap_time = _playOrderList[i].lapTime;

            Debug.Log("index: " + _index + ", speed: " + _speed + ", lap time: " + _lap_time);
        }
    }

    void printCharacterList(List<CharacterData> _characterList)
    {
        foreach (var p in _characterList)
        {
            Debug.Log($"index: {(int)p.character_position}, name: {p.character_name}, speed: {p.character_speed}, attack power: {p.character_attack_power}, health: {p.character_health}");
        }
    }

    void removeDeadPlayer(List<PlayOrderData> _playOrderList, List<CharacterData> _characterList)
    {
        Debug.Log("before: " + _playOrderList.Count);
        foreach (var c in _characterList)
        {
            int i = -1;
            if (c.character_health <= 0 && c.is_character_alive == true)
            {
                i = (int)c.character_position;
                _playOrderList.RemoveAll(_playOrderList => _playOrderList.playerIndex == i);
                c.is_character_alive = false;
                Debug.Log("index removed: " + i + " health: " + c.character_health);


            }
        }

        Debug.Log("after: " + _playOrderList.Count);
        updateLives();

    }


    void initiateTurn()
    {
        //Debug.Log("playOrderDataList count: " + sortedPlayOrderList.Count);
        //printPlayOrderList(sortedPlayOrderList);
        //printCharacterList(sortedCharacterList);
        removeDeadPlayer(sortedPlayOrderList, sortedCharacterList);

        //currentPlayerIndex = playOrderIndex.Dequeue();
        //PlayOrderData playOrderData = playOrderDataQue.Dequeue();
        PlayOrderData playOrderData = sortedPlayOrderList[0];
        //sortedPlayOrderList.RemoveAt(0);

        currentPlayerIndex = playOrderData.playerIndex;
        string currentPlayerName = sortedCharacterList[currentPlayerIndex].character_name;
        Position position = sortedCharacterList[currentPlayerIndex].character_position;
        Debug.Log("Current Player: " + currentPlayerName + " at " + position);


        //Debug.Log("dequed data: " + "index, " + playOrderData.playerIndex + "  speed, " + playOrderData.playerSpeed + "  lap, " + playOrderData.lapTime);
        /*
        while (sortedCharacterList[currentPlayerIndex].character_health <= 0)
        {
            Debug.Log("skipping index: " + currentPlayerIndex + " because of no health score");
            //currentPlayerIndex = playOrderIndex.Dequeue();
            //playOrderData = playOrderDataQue.Dequeue();

            playOrderData = sortedPlayOrderList[0];
            sortedPlayOrderList.RemoveAt(0);

            currentPlayerIndex = playOrderData.playerIndex;
            currentPlayerName = sortedCharacterList[currentPlayerIndex].character_name;
            position = sortedCharacterList[currentPlayerIndex].character_position;
            Debug.Log("Revised Current Player: " + currentPlayerName + " at " + position);
        }
        */


        // sortedPlayOrderDataListQue length
        //Debug.Log("play order ramaining Que: " + playOrderDataListQue.Count);


        if (currentPlayerIndex <= 3)
        {
            Debug.Log("Click on a character in the RIGHT side to attack.");
        }
        else
        {
            Debug.Log("Click on a character in the LEFT side to attack.");
        }

        //int nextPlayerIndex = playOrderIndex.Peek();
        //PlayOrderData nextPlayerData = playOrderDataQue.Peek();
        PlayOrderData nextPlayerData = sortedPlayOrderList[1];

        int nextPlayerIndex = nextPlayerData.playerIndex;
        string nextPlayerName = sortedCharacterList[nextPlayerIndex].character_name;
        position = sortedCharacterList[nextPlayerIndex].character_position;

        //Debug.Log("Next Player: " + nextPlayerName);
        Debug.Log("next player: " + nextPlayerName + " at " + position);

        // play order bar update
        //sortedPlayOrderList.Insert(0, playOrderData);
        processPlayOrderBar(sortedPlayOrderList);

        // remove the current player from the order list
        sortedPlayOrderList.RemoveAt(0);


        // game state update
        gameState = GameState.AwaitingInput;
    }


    void processPlayOrderBar(List<PlayOrderData> playOrderDataList)
    {

        combatReadinessList = new List<CombatReadinessData>();
        PlayOrderData playOrderData = playOrderDataList[0];

        //Debug.Log("playOderData:" + "index, " + playOrderData.playerIndex + " currentTime, " + playOrderData.lapTime + " speed, " + playOrderData.playerSpeed);

        for (int i = 0; i < 8; i++)
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
                    float pos = (playOrderData.lapTime * playOrderDataList[j].playerSpeed) % 702;

                    combatReadinessData.currentPosition = (int)pos;

                    combatReadinessData.heightPercentile = ((702.0f - pos) / 702.0f)*100;
                    //playOrderBar.Enqueue(playerOrderBarData);

                    combatReadinessList.Add(combatReadinessData);

                    //Debug.Log("index: lapTime: " + i + " : " + playOrderDataList[j].lapTime);
                    break;
                }

            }
        }



        //Debug.Log(playOrderBarList.Count);
        //sortedPlayOrderDataList = playOrderDataList.OrderBy(player => player.lapTime)
        //   .ThenByDescending(player => player.playerSpeed)
        //   .ToList();
        // var
        sortedCombatReadinessList = combatReadinessList.OrderByDescending(player => player.currentPosition)
        //sortedCombatReadinessList = combatReadinessList.OrderBy(player => player.playerIndex)
                .ToList();

        foreach(var p in sortedCombatReadinessList)
        {
            //Debug.Log($"index: {p.playerIndex}, lapTime: {p.lapTime}, currentTime: {p.currentTime}, currentPosition: {p.currentPosition}, heightPercentile: {p.heightPercentile}");
        }

        // move and position each number sprite
        combatReadinessBar.processCombatReadinessbar(sortedCombatReadinessList);
        //combatReadinessBar.Update(sortedCombatReadinessList);
        
    }


    void printHealthData(string timePoint)
    {
        Debug.Log("Health " + timePoint + " : " + sortedCharacterList[0].character_health + " : " + sortedCharacterList[1].character_health + " : "
        + sortedCharacterList[2].character_health + " : " + sortedCharacterList[3].character_health + " : " + sortedCharacterList[4].character_health
        + " : " + sortedCharacterList[5].character_health + " : " + sortedCharacterList[6].character_health + " : " + sortedCharacterList[7].character_health);
    }

    void printAttackPowerData()
    {
        Debug.Log("Character AttackPower: " + sortedCharacterList[0].character_attack_power + " : " + sortedCharacterList[1].character_attack_power + " : "
        + sortedCharacterList[2].character_attack_power + " : " + sortedCharacterList[3].character_attack_power + " : " + sortedCharacterList[4].character_attack_power
        + " : " + sortedCharacterList[5].character_attack_power + " : " + sortedCharacterList[6].character_attack_power + " : " + sortedCharacterList[7].character_attack_power);

    }

    void printCharacterData()
    {
        for (int i = 0; i < sortedCharacterList.Count; i++)
        {
            currentActiveCharacterIndex = i;
            Debug.Log("Index: " + ((int)sortedCharacterList[currentActiveCharacterIndex].character_position) + ", "
            + "Name: " + sortedCharacterList[currentActiveCharacterIndex].character_name + ", "
            + "isActive? " + sortedCharacterList[currentActiveCharacterIndex].is_character_alive + ", "
            + "Speed: " + sortedCharacterList[currentActiveCharacterIndex].character_speed + ", "
            + "Position: " + sortedCharacterList[currentActiveCharacterIndex].character_position + ", "
            + "Health: " + sortedCharacterList[currentActiveCharacterIndex].character_health + ", "
            + "AttackPower: " + sortedCharacterList[currentActiveCharacterIndex].character_attack_power);

            //gameState = GameState.Playing;
        }
    }

    void executePlayerCommand()
    {

    }

    void handleNoHealth()
    {
        // for each character
        for (int i = 0; i < sortedCharacterList.Count; i++)
        {
            // if no Health score
            if (sortedCharacterList[i].character_health <= 0)
            {
                // get the character without Health score to fight
                GameObject gameObject = GameObject.Find(sortedCharacterList[i].character_name);

                // if not already destroyed
                if (gameObject != null)
                {
                    // destroy player
                    Destroy(gameObject);
                    Debug.Log(sortedCharacterList[i].character_name + " died.");
                    //buttonGreenEyes.gameObject.SetActive(false);


                    // destroy player icon on CombatReadinessBar
                    //Debug.Log("dead character name: " + gameObject.name);
                // destroy icon on CombatReadinessBar 
                string characterName = sortedCharacterList[i].character_name;
                string childName = "";
                if(characterName == "Sword Man")
                {
                    childName = "zero"; 
                }          
                if(characterName == "Spear Soldier")
                {
                    childName = "one";
                }
                if(characterName == "Hammer Man")
                {
                    childName = "two";
                }
                if(characterName == "Brown Horse")
                {
                    childName = "three";
                }
                if(characterName == "Green Eyes")
                {
                    childName = "four";
                }
                if(characterName == "Black Horse")
                {
                    childName = "five";
                }
                if(characterName == "Sword Pirate")
                {
                    childName = "six";
                }
                if(characterName == "Green Sword")
                {
                    childName = "seven";
                }

                if(childName != "")
                {
                    GameObject dest = GameObject.Find(childName);
                    Destroy(dest);
                    Debug.Log(dest + " destroyed");
                }
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
        for (int i = 0; i < sortedCharacterList.Count; i++)
        {
            int k = sortedCharacterList[i].character_index;
            // if friend has no Health score
            if (k < 4 && sortedCharacterList[k].character_health <= 0)
            {
                // decrease the number of alive lives
                friendLives--;
            }
            // for enemy
            else if (k > 3 && sortedCharacterList[k].character_health <= 0)
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
        string playerName = sortedCharacterList[currentPlayerIndex].character_name;
        string enemyName = sortedCharacterList[currentOponentIndex].character_name;

        int playerAttckPower = sortedCharacterList[currentPlayerIndex].character_attack_power;
        int enemyAttackPower = sortedCharacterList[currentOponentIndex].character_attack_power;

        int playerHealth = sortedCharacterList[currentPlayerIndex].character_health;
        int enemyHealth = sortedCharacterList[currentOponentIndex].character_health;

        //Debug.Log(playerName + " : " + playerHealth + " : " + playerAttckPower);
        //Debug.Log(enemyName + " : " + enemyHealth + " : " + enemyAttackPower);
        //Debug.Log(playerName + " attacked " + enemyName);

        int newPlayerHealth = playerHealth;
        int newEnemyHealth = enemyHealth - playerAttckPower;

        sortedCharacterList[currentPlayerIndex].character_health = newPlayerHealth;
        sortedCharacterList[currentOponentIndex].character_health = newEnemyHealth;

        playerHealth = sortedCharacterList[currentPlayerIndex].character_health;
        enemyHealth = sortedCharacterList[currentOponentIndex].character_health;

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
