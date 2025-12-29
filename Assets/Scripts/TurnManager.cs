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
    // Important data needed for combat readiness bar
    public int playerIndex;
    public float currentPosition;
    public float heightPercentile;

    // Additional data for calculations
    public int playerSpeed;
    public float lapTime;
    public float currentTime;

}

[Serializable]
public class AttackAnimationData
{
    public GameObject characterGameObject;
}


public class TurnManager : MonoBehaviour
{
    public const int NumCharacters = 8;
    public const int NumLaps = 100;
    public const int TrackLength = 701;
    public Position characterPositions;
    public const int Speed = 5;
    [SerializeField] private Character character;
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

    public static TurnManager Instance { get; private set; }
    //public List<AttackAnimationData> attackAnimationData;
    //public event Action<List<AttackAnimationData>> AttackAnimation;
    public event Action<GameObject, GameObject> AttackAnimation;

    public List<int> indx;
    public GameObject _gameObjectP;
    public GameObject _gameObjectO;
    public Vector3 moveVector;



    public int getcurrentPlayerIndex()
    {
        return currentPlayerIndex;
    }

    public void sendSortedCharacterData(List<CharacterData> characters)
    {
        sortedCharacterList = characters;
        //dataReceived = true;

        // print character data
        //printCharacterList(characters);
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



        // lap time on each loop of distance 701 of 6 loops
        // example of track distances: 701, 3457, 23
        for (int j = TrackLength; j <= TrackLength * NumLaps; j = j + TrackLength)
        {
            // for each character of 8
            for (int i = 0; i < NumCharacters; i++)
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
        //printPlayOrder(playOrderList);

        // sort lap time data, 
        // primary order by increasing lap time: in the order that characters passed the start line until all completed 6 laps. 
        // secondary order by decreasing speed: higher speed comes ahead of lower speed when there are the same time records. 
        //var 
        sortedPlayOrderList = playOrderList.OrderBy(player => player.lapTime)
           .ThenByDescending(player => player.playerSpeed)
           .ToList();


        // output
        //printPlayOrder(sortedPlayOrderList);

        // take only indexes for playing turn order
        //generatePlayOrderIndex(sortedPlayOrderList);
        //Debug.Log("length: " + sortedPlayOrderDataList.Count);
        //enqueSortedPlayOrderDataList(sortedPlayOrderList);
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
        for (int i = 0; i < NumCharacters * NumLaps; i++)
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

    void handleNoHealth() //TODO: rename to more appropriate name for cleanup
    {
        //printCharacterList(sortedCharacterList);

        int i;
        // for each character
        for (int j = 0; j < sortedCharacterList.Count; j++)
        {
            i = sortedCharacterList[j].character_index;
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
                    if (characterName == "Sword Man")
                    {
                        childName = "zero";
                    }
                    if (characterName == "Spear Soldier")
                    {
                        childName = "one";
                    }
                    if (characterName == "Hammer Man")
                    {
                        childName = "two";
                    }
                    if (characterName == "Brown Horse")
                    {
                        childName = "three";
                    }
                    if (characterName == "Green Eyes")
                    {
                        childName = "four";
                    }
                    if (characterName == "Black Horse")
                    {
                        childName = "five";
                    }
                    if (characterName == "Sword Pirate")
                    {
                        childName = "six";
                    }
                    if (characterName == "Green Sword")
                    {
                        childName = "seven";
                    }

                    if (childName != "")
                    {
                        GameObject dest = GameObject.Find(childName);
                        if (dest != null)
                        {
                            Destroy(dest);
                        }

                        Debug.Log(dest + " destroyed");
                    }
                }
            }
        }
    }
    void removeDeadPlayer(List<PlayOrderData> _playOrderList, List<CharacterData> _characterList)
    {
        //Debug.Log("before: " + _playOrderList.Count);
        //character.printCharacterData(_characterList);
        foreach (var c in _characterList)
        {
            int indx = -1;
            if (c.character_health <= 0 && c.is_character_alive == true)
            {
                //i = (int)c.character_position;
                indx = c.character_index;
                _playOrderList.RemoveAll(_playOrderList => _playOrderList.playerIndex == indx);
                c.is_character_alive = false;
                Debug.Log("index removed: " + indx + " health: " + c.character_health);

                handleNoHealth();
            }
        }

        //Debug.Log("after: " + _playOrderList.Count);
        //character.printCharacterData(_characterList);
        updateLives();

    }


    void initiateTurn()
    {
        // remove the previous player from the play order list
        sortedPlayOrderList.RemoveAt(0);

        //handleNoHealth();
        //Debug.Log("playOrderDataList count: " + sortedPlayOrderList.Count);
        //printPlayOrderList(sortedPlayOrderList);
        //printCharacterList(sortedCharacterList);
        removeDeadPlayer(sortedPlayOrderList, sortedCharacterList); // Cleans up dead characters from last turn
        //printAttackPowerData();
        //printHealthScore();

        // play order bar update
        //sortedPlayOrderList.Insert(0, playOrderData);
        processCombatReadiness(sortedPlayOrderList);

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





        // game state update
        gameState = GameState.AwaitingInput;
    }



    void processCombatReadiness(List<PlayOrderData> playOrderDataList)
    {
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
        combatReadinessBar.processCombatReadinessBar(sortedCombatReadinessList);
        //printCombatReadiness(sortedCombatReadinessList);
        //combatReadinessBar.Update(sortedCombatReadinessList);

    }

    public void printCombatReadiness(List<CombatReadinessData> combatReadinessList)
    {
        foreach (var p in combatReadinessList)
        {
            Debug.Log($"index: {p.playerIndex}, lapTime: {p.lapTime}, currentTime: {p.currentTime}, currentPosition: {p.currentPosition}, heightPercentile: {p.heightPercentile}");
        }
    }

    void printHealthScore()
    {
        Debug.Log("Health: " + sortedCharacterList[0].character_health + " : " + sortedCharacterList[1].character_health + " : "
        + sortedCharacterList[2].character_health + " : " + sortedCharacterList[3].character_health + " : " + sortedCharacterList[4].character_health
        + " : " + sortedCharacterList[5].character_health + " : " + sortedCharacterList[6].character_health + " : " + sortedCharacterList[7].character_health);
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

    void handleAttackAnimation()
    {
        float attackProximityIndex = 0.9f;

        // find player and oponent objects
        _gameObjectP = GameObject.Find(sortedCharacterList[currentPlayerIndex].character_name);
        _gameObjectO = GameObject.Find(sortedCharacterList[currentOponentIndex].character_name);

        Debug.Log("Player Position: " + _gameObjectP.transform.position);
        Debug.Log("Oponent Position: " + _gameObjectO.transform.position);

        // calculate the vector from player position to oponent position
        moveVector = (_gameObjectO.transform.position - _gameObjectP.transform.position)*attackProximityIndex;

        // move player to attack
        _gameObjectP.transform.position = _gameObjectP.transform.position + moveVector;

        // Code here runs after the delay
        Invoke("MovePlayerBack", 2f);
    }

    void MovePlayerBack()
    {
        Debug.Log("Function called after a delay.");
        // move player back
        _gameObjectP.transform.position = _gameObjectP.transform.position - moveVector;

    }

    void handleAttack()
    {
        //indx = new List<int> { currentPlayerIndex, currentOponentIndex };
        //attackAnimationData = getAttackAnimationData(indx, sortedCharacterList);
        //Debug.Log("Invoking AttackAnimation");
        //AttackAnimation?.Invoke(_gameObjectP, _gameObjectO);

        // call handleAttackAnimation function to move player close to oponent
        handleAttackAnimation();


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

        int playerAttackPower = sortedCharacterList[currentPlayerIndex].character_attack_power;
        int enemyAttackPower = sortedCharacterList[currentOponentIndex].character_attack_power; // unused currently because enemy does not counter-attack

        int playerHealth = sortedCharacterList[currentPlayerIndex].character_health; // unused currently because enemy does not counter-attack
        int enemyHealth = sortedCharacterList[currentOponentIndex].character_health;

        //Debug.Log(playerName + " : " + playerHealth + " : " + playerAttackPower);
        //Debug.Log(enemyName + " : " + enemyHealth + " : " + enemyAttackPower);
        //Debug.Log(playerName + " attacked " + enemyName);

        int newPlayerHealth = playerHealth;
        int newEnemyHealth = enemyHealth - playerAttackPower;

        sortedCharacterList[currentPlayerIndex].character_health = newPlayerHealth;
        sortedCharacterList[currentOponentIndex].character_health = newEnemyHealth;

        playerHealth = sortedCharacterList[currentPlayerIndex].character_health;
        enemyHealth = sortedCharacterList[currentOponentIndex].character_health;

        //Debug.Log(playerName + " : " + playerHealth + " : " + playerAttackPower);
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
