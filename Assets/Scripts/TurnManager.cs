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

    GettingCharacterData, //
    //DataNotReady,
    InitiatingTurn,
    AwaitingInput,
    OnReady,
    OnDash,
    OnAttack,
    AttackOver,
    OnBackdash,
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
    [SerializeField] private Character character;
    public GameState gameState;
    private List<CharacterData> sortedCharacterList;
    public int animPlayerIndex;
    public int currentPlayerIndex;
    private int currentOponentIndex;
    private int friendLives;
    private int enemyLives;

    public List<PlayOrderData> playOrderList;
    public List<PlayOrderData> sortedPlayOrderList;
    private Queue<PlayOrderData> playOrderDataQue;
    private Queue<int> playOrderIndex = new Queue<int>();
    //public event Action<Queue<int>> playOrderBar;
    public CombatReadinessData combatReadinessData;
    public List<CombatReadinessData> combatReadinessList;
    public List<CombatReadinessData> sortedCombatReadinessList;
    [SerializeField] private CombatReadinessBar combatReadinessBar;
    [SerializeField] private Character characterScript;

    //public List<int> indx;
    public GameObject _gameObjectP;
    public GameObject _gameObjectO;
    public float speed;
    public float speedAdjuster;

    [SerializeField] List<Animator> characterAnimRefs;

    private Vector3 moveVector;
    public Vector3 playerPosition;
    public Vector3 enemyPosition;
    public float timeElapsed;

    public bool isPlayerReady;
    public bool readyToClickEnemy;
    public bool dashFinished;
    public bool attackFinished;
    public bool healthAlreadyUpdated;
    public bool backdashFinished;


    public int getcurrentPlayerIndex()
    {
        return currentPlayerIndex;
    }


    /// <summary>
    /// characterData state related functions
    /// </summary>
    public void getCharacterData()
    {
        //Debug.Log("getCharacterData starts");
        //Debug.Log("Receiving character data in TurnManager");

        //if(character.getCharacterDataList() )
        //sortedCharacterList.Clear();
        sortedCharacterList = character.getCharacterDataList();
        //Debug.Log("sorted character data list: " + sortedCharacterList.Count);
        //dataReceived = true;

        // print character data
        //printCharacterList(characters);
        // generatePlayOrder
        //Debug.Log("Calling generatePlayOrder");
        if (sortedCharacterList != null)
        {
            generatePlayOrder();
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



    void generatePlayOrder()
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
        //Debug.Log("sortedPlayOrderList: " + sortedPlayOrderList.Count);



        // take only indexes for playing turn order
        //generatePlayOrderIndex(sortedPlayOrderList);
        //Debug.Log("length: " + sortedPlayOrderDataList.Count);
        //enqueSortedPlayOrderDataList(sortedPlayOrderList);
        //printSortedPlayOrderIndex();

        // if turn is never assigned, assign the first turn


        if (sortedPlayOrderList.Count == NumCharacters * NumLaps)
        {
            //Debug.Log("sortedPlayOrderList: " + sortedPlayOrderList.Count);
            gameState = GameState.InitiatingTurn;
        }
        else
        {
            Debug.Log("regenerating character data.");
            gameState = GameState.GettingCharacterData;
        }

        //Debug.Log("generatePlayOrder ends");
    }




    /// <summary>
    /// initiatingturn state related functions
    /// </summary>
    void initiateTurn()
    {
        //Debug.Log("initiateTutn starts");

        resetBools();
        //Debug.Log("sortedPlayOrderList: " + sortedPlayOrderList.Count);
        //printPlayOrderList(sortedPlayOrderList);

        // Cleans up dead characters from last turn
        //removeDeadPlayer(sortedPlayOrderList, sortedCharacterList); 

        // remove the previous player from the play order list
        if (sortedPlayOrderList.Count > 0)
        {
            //sortedPlayOrderList.RemoveAt(0);
        }

        //Debug.Log("sortedPlayOrderList: " + sortedPlayOrderList.Count);
        //printPlayOrderList(sortedPlayOrderList);
        //handleNoHealth();
        //Debug.Log("playOrderDataList count: " + sortedPlayOrderList.Count);
        //printPlayOrderList(sortedPlayOrderList);
        //printCharacterList(sortedCharacterList);


        printAttackPowerData();
        printHealthScore();

        // play order bar update
        //sortedPlayOrderList.Insert(0, playOrderData);
        processCombatReadiness(sortedPlayOrderList);

        //currentPlayerIndex = playOrderIndex.Dequeue();
        //PlayOrderData playOrderData = playOrderDataQue.Dequeue();
        PlayOrderData playOrderData = sortedPlayOrderList[0];
        //sortedPlayOrderList.RemoveAt(0);

        currentPlayerIndex = playOrderData.playerIndex;
        animPlayerIndex = currentPlayerIndex;
        string currentPlayerName = sortedCharacterList[currentPlayerIndex].character_name;
        Position position = sortedCharacterList[currentPlayerIndex].character_position;
        Debug.Log("Current Player: " + currentPlayerName + " at " + currentPlayerIndex);
        Debug.Log(currentPlayerName + " Speed: " + speed);


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

        // reset animation booleans
        //resetAnimBoolean();


        // game state update
        //gameState = GameState.AwaitingInput;
        //gameState = GameState.OnReady;
        //Debug.Log(gameState + " in initiateTurn");

        //Sanim.SetBool("isMyTurn", true);
        //Debug.Log(animator.isMyTurn + " in initiate turn");
        //Debug.Log("initiateTurn ends");
    }



    void processCombatReadiness(List<PlayOrderData> playOrderDataList)
    {
        //Debug.Log("processCombatReadiness starts");
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
        gameState = GameState.OnReady;
        readyToClickEnemy = true;

        //Debug.Log("processCombatReadiness ends");
    }




    /// <summary>
    /// OnReady state related functions
    /// </summary>

    void turnOnReadyOS()
    {
        //Debug.Log("turnOnReadyOs starts");
        // animation:
        // idle => ready_os
        if (currentPlayerIndex < 4)
        {
            characterAnimRefs[currentPlayerIndex].SetBool("idleLOOP", false);
            characterAnimRefs[currentPlayerIndex].SetBool("readyOS", true);
            //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
            //Debug.Log("ready_os");
            //Debug.Log("current player: " + currentPlayerIndex);
        }
        else // character index: 4,5,6,7
        {
            gameState = GameState.AwaitingInput;            
        }

        //isPlayerReady = true;
    }



    void printHealthScore()
    {
        Debug.Log("Health: " + sortedCharacterList[0].character_health + " : " + sortedCharacterList[1].character_health + " : "
        + sortedCharacterList[2].character_health + " : " + sortedCharacterList[3].character_health + " : " + sortedCharacterList[4].character_health
        + " : " + sortedCharacterList[5].character_health + " : " + sortedCharacterList[6].character_health + " : " + sortedCharacterList[7].character_health);
    }






    /// <summary>
    /// awaitingInput state related functions
    /// </summary>
    public void handleInput(string clickedEnemyName)
    {
        //Debug.Log("handleAwaitingInput starts");
        //anim.SetBool("isMyTurn", false);


        //Debug.Log("Player: " + sortedCharacterData[currentPlayerIndex].character_name);
        //printPlayOrder(playOrderDataList);

        Debug.Log(sortedCharacterList[currentPlayerIndex].character_name + " selected " + clickedEnemyName + " to attack.");
        currentOponentIndex = indexSortedCharacterData(clickedEnemyName);

        if (sortedCharacterList[currentOponentIndex].character_health > 0)
        {
            //Debug.Log("Friend Position:Enemy Position = " + (1 + currentPlayerIndex) + ":" + (1 + currentEnemyIndex));
            getMoveData();

            if (currentPlayerIndex < 4)
            {
                //characterAnimRefs[currentPlayerIndex].SetBool("idle", false);
                //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", true);
                characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
                characterAnimRefs[currentPlayerIndex].SetBool("dashOS", true);
                //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
                //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
                //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
                //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
                //Debug.Log("dash_os");              
            }
            else
            {
                gameState = GameState.OnDash;
                //Debug.Log(gameState + " in handleAwaitingInputPhase");                
            }

            readyToClickEnemy = false;
        }
        else
        {
            Debug.Log("The selected enemy is already dead. Choose another enemy.");
            readyToClickEnemy = true;
        }

        //readyToClick = false;
        //Debug.Log("handleAwaitingInput ends");
    }

    void turnOnDashOS()
    {
        //Debug.Log("turnOnDashOs starts");
        // animation:
        // ready_loop => dash_os
        if (currentPlayerIndex < 4)
        {
            //characterAnimRefs[currentPlayerIndex].SetBool("idle", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", true);
            characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
            characterAnimRefs[currentPlayerIndex].SetBool("dashOS", true);
            //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
            //Debug.Log("ready_os");
            //Debug.Log("current player: " + currentPlayerIndex);
        }
        else // character index: 4,5,6,7
        {
            //gameState = GameState.AwaitingInput; 
            gameState = GameState.OnDash;           
        }

        //dashOS = true;    
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








    /// <summary>
    ///  attack related functions
    /// </summary>
    void handleFinishedAttack()
    {
        //Debug.Log("handleFinishedAttack starts");

        // animation:
        // dash_loop => attack
        if (animPlayerIndex < 4)
        {
            //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
            //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", true);
            characterAnimRefs[animPlayerIndex].SetBool("dashLOOP", false);
            characterAnimRefs[animPlayerIndex].SetBool("attackOS", true);
            //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
            //Debug.Log("attack, dashLOOP false, attackOS true");
        }
        else
        {
            gameState = GameState.AttackOver;
        }
    }

    void updateStats()
    {
        //Debug.Log("updateStats starts");

        updateHealth();
        removeDeadPlayer(); // updateLives() executed in there

        //Debug.Log("after attack");
        printAttackPowerData();
        printHealthData("after attack");

        // update remaining lives
        // if no lives left on eather team, game is over
        updateLives();

        // animation:
        // backdash_os => backdash_loop
        if (animPlayerIndex < 4)
        {
            //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
            //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("attackOS", false);
            characterAnimRefs[animPlayerIndex].SetBool("backdashOS", true);
            //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", true);
            //Debug.Log("backdashOS true");
        }
        else
        {
            gameState = GameState.OnBackdash;            
        }
    }






    //void removeDeadPlayer(List<PlayOrderData> _playOrderList, List<CharacterData> _characterList)
    void removeDeadPlayer()
    {
        List<PlayOrderData> _playOrderList = sortedPlayOrderList;
        List<CharacterData> _characterList = sortedCharacterList;

        //Debug.Log("before: " + _playOrderList.Count);
        //character.printCharacterData(_characterList);
        foreach (var c in _characterList)
        {
            int indx = -1;
            if (c.character_health <= 0 && c.is_character_alive == true)
            {
                //i = (int)c.character_position;
                indx = c.character_index;

                // remove the dead character from the play order list
                _playOrderList.RemoveAll(_playOrderList => _playOrderList.playerIndex == indx);
                c.is_character_alive = false;
                Debug.Log("index removed: " + indx + " health: " + c.character_health);

                // destroy character object and icon object
                destroyCharacterAndIcon();
            }
        }
        //Debug.Log("after: " + _playOrderList.Count);
        //character.printCharacterData(_characterList);
        //updateLives();
    }



    void destroyCharacterAndIcon() //TODO: rename to more appropriate name for cleanup
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


    /// <summary>
    ///  stats and print functions
    /// </summary>
    /// 
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


    void getMoveData()
    {
        // find player and oponent objects
        _gameObjectP = GameObject.Find(sortedCharacterList[currentPlayerIndex].character_name);
        _gameObjectO = GameObject.Find(sortedCharacterList[currentOponentIndex].character_name);

        // player speed
        speed = sortedCharacterList[currentPlayerIndex].character_speed;
        //Debug.Log(sortedCharacterList[currentPlayerIndex].character_name + " speed: " + speed);

        // calculate the vector from player position to oponent position    
        playerPosition = _gameObjectP.transform.position;
        enemyPosition = _gameObjectO.transform.position;
        moveVector = enemyPosition - playerPosition;
        Vector3 normalizedMoveVector = moveVector.normalized;
        moveVector = moveVector - normalizedMoveVector;

        Vector3 startPosition = playerPosition;
        Vector3 targetPosition = playerPosition + moveVector;
        //Debug.Log("start: " + startPosition);
        //Debug.Log("target: " + targetPosition);
        //Debug.Log("move vector: " + moveVector);

        if (currentPlayerIndex < 4)
        {
            //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
            //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", true);
            characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
            characterAnimRefs[currentPlayerIndex].SetBool("dashOS", true);
            //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
            //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
            //Debug.Log("readyLOOP false, dash_os true");
        }
        //gameState = GameState.OnDash;
    }


    void resetBools()
    {
        isPlayerReady = false;      // To make sure that turnOnReadyOS() is executed only once in Update()
        readyToClickEnemy = false;  // To make sure that choosing enemy is acceptable only when ready
        //dashFinished = false;     
        attackFinished = false;   // To make sure that handleFinishedAttack() is exucuted only once in Update()
        healthAlreadyUpdated = false;   // To make sure that updateStats() is executed only once in Update()
        //backdashFinished = false;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Debug.Log("Manager script Start, executed.");
        //Debug.Log("TurnManager, Start executed");
        //Debug.Log("GameState in TurnManager Start: " + gameState);

        currentPlayerIndex = -1;
        animPlayerIndex = 0;
        speed = 10f;

        //Debug.Log("CharacterAnimRefs Parameters:");
        for (int i = 0; i < characterAnimRefs.Count; i++)
        {
            characterAnimRefs[i].SetBool("idleLOOP", true);
            characterAnimRefs[i].SetBool("readyOS", false);
            characterAnimRefs[i].SetBool("readyLOOP", false);
            characterAnimRefs[i].SetBool("dashOS", false);
            characterAnimRefs[i].SetBool("dashLOOP", false);
            characterAnimRefs[i].SetBool("attackOS", false);
            characterAnimRefs[i].SetBool("backdashOS", false);
            characterAnimRefs[i].SetBool("backdashLOOP", false);
            //Debug.Log(characterAnimRefs[i].GetBool("isMyTurn"));
        }

        timeElapsed = 0;

        //printCharacterData();
    }


    // Update is called once per frame
    void Update()
    {
        //Debug.Log(aanimator.isMyTurn + " in Update");
        //Debug.Log("GameState in TurnManager Update: " + gameState);

        if (gameState == GameState.GameOver)
        {
            return;

            //Debug.Log("Quitting Game!");
            //UnityEditor.EditorApplication.isPlaying = false;
        }
        else if (gameState == GameState.GettingCharacterData)
        {
            getCharacterData();
        }
        else if (gameState == GameState.InitiatingTurn)
        {
            initiateTurn();
        }
        else if (gameState == GameState.OnReady && isPlayerReady == false)
        {
            turnOnReadyOS();
            isPlayerReady = true;
        }
        else if (gameState == GameState.AwaitingInput)
        {
            //turnOnDashOS();
        }

        else if (gameState == GameState.OnDash)
        {
            // Optional: apply easing (smooth start/end) using Mathf.SmoothStep
            speedAdjuster = 0.1f;
            float step = speed * Time.deltaTime * speedAdjuster;
            
            _gameObjectP.transform.position = Vector3.MoveTowards(_gameObjectP.transform.position, playerPosition + moveVector, step);

            if (_gameObjectP.transform.position == playerPosition + moveVector)
            {
                //Debug.Log("ready to attack");
                gameState = GameState.OnAttack;
                //dashFinished = true;
            }
        }
        else if (gameState == GameState.OnAttack && attackFinished == false)
        {
            handleFinishedAttack();
            attackFinished = true;
        }
        else if (gameState == GameState.AttackOver && healthAlreadyUpdated == false)
        {
            updateStats();
            healthAlreadyUpdated = true;
        }
        else if (gameState == GameState.OnBackdash)
        {
            Vector3 targetPosition = playerPosition;

            //Debug.Log("moveback start: " + startPosition);
            //Debug.Log("current pos: " + _gameObjectP.transform.position);
            //Debug.Log("moveback target: " + playerPosition);
            //Debug.Log("move vector: " + moveVector);

            // Calculate the interpolation value (percentage of duration passed)
            // Optional: apply easing (smooth start/end) using Mathf.SmoothStep
            speedAdjuster = 0.1f;
            float step = speed * Time.deltaTime * speedAdjuster;

            // Move the object a step closer to the target position on every update
            _gameObjectP.transform.position = Vector3.MoveTowards(_gameObjectP.transform.position, playerPosition, step);

            if (_gameObjectP.transform.position == playerPosition)
            {
                // animation: back to the original position
                // backdash_loop => idle_loop
                if (animPlayerIndex < 4)
                {
                    characterAnimRefs[currentPlayerIndex].SetBool("idleLOOP", true);
                    //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
                    //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
                    //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", false);
                    //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
                    //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
                    //characterAnimRefs[animPlayerIndex].SetBool("backdashOS", true);
                    characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
                    //Debug.Log("backdashOS true");
                }
                Debug.Log("Player is back to idle.");
                //backdashFinished = true;

                // if there still is any player in the queue, 
                // remove the previous player from the play order list
                // and update lives
                // if not game over, then initiate the next turn
                if (sortedPlayOrderList.Count > 0)
                {
                    sortedPlayOrderList.RemoveAt(0);
                    updateLives();
                    if(gameState != GameState.GameOver)
                    {
                        gameState = GameState.InitiatingTurn;                        
                    }
                    else
                    {
                        Debug.Log("Game Over! Bye.");
                    }
                }
            }
        }
    }
}



















/*







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




               switch (gameState)
               {
                   //case GameState.DataNotReady:
                   case GameState.GettingCharacterData:
                       getCharacterData();
                       break;

                   case GameState.InitiatingTurn:
                       initiateTurn();
                       break;

                   case GameState.AwaitingInput:

                       break;

                   case GameState.OnReady:

                       break;

                   case GameState.OnDash:
                       if(myRoutine == null)
                       {
                           handleDash();                    
                       }

                       break;

                   case GameState.OnAttack:
                       if(myRoutine == null)
                       {
                           handleFinishedAttack();                    
                       }

                       break;

                   case GameState.AttackOver:
                       if(myRoutine == null)
                       {
                           updateStats();                    
                       }

                       break;

                   case GameState.OnBackdash:
                       if(myRoutine == null)
                       {
                           handleBackdash();                    
                       }


                       break;

                   default:
                       break;
               }





   IEnumerator MovebackFromEnemy()
   {
       Debug.Log("Moving back from enemy");

       //float duration = 10f; // The total time the movement should take
       float duration = 100f / sortedCharacterList[currentPlayerIndex].character_speed;
       //float attackProximityIndex = 0.9f;

       // find player and oponent objects
       //_gameObjectP = GameObject.Find(sortedCharacterList[currentPlayerIndex].character_name);
       //_gameObjectO = GameObject.Find(sortedCharacterList[currentOponentIndex].character_name);

       // calculate the vector from player position to oponent position
       //moveVector = (_gameObjectO.transform.position - _gameObjectP.transform.position);
       //Vector3 normalizedMoveVector = moveVector.normalized;
       //moveVector = moveVector - normalizedMoveVector;
       //moveVector = (_gameObjectO.transform.position - _gameObjectP.transform.position)*attackProximityIndex;        

       //Vector3 startPosition = _gameObjectP.transform.position;
       //Vector3 targetPosition = startPosition + moveVector;



       // MoveBack
       Vector3 startPosition = _gameObjectP.transform.position;
       Vector3 targetPosition = playerPosition;

       Debug.Log("moveback start: " + startPosition);
       Debug.Log("moveback target: " + targetPosition);
       Debug.Log("move vector: " + moveVector);

       float timeElapsed = 0;

       while (timeElapsed < duration)
       {
           // Calculate the interpolation value (percentage of duration passed)
           float t = timeElapsed / duration;

           // Optional: apply easing (smooth start/end) using Mathf.SmoothStep
           // t = Mathf.SmoothStep(0f, 1f, t); 

           // Update position using Lerp
           _gameObjectP.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

           // Increment the time elapsed by the time since the last frame
           timeElapsed += Time.deltaTime;

           yield return null; // Wait until the next frame
       }

       //Debug.Log("speed: " + sortedCharacterList[currentPlayerIndex].character_speed);
       // Ensure the object reaches the exact target position at the end
       _gameObjectP.transform.position = targetPosition;

       //yield return new WaitForSeconds(1); // Pauses here for 2 seconds

       if (animPlayerIndex < 4)
       {
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", true); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
           characterAnimRefs[animPlayerIndex].SetBool("backdashLOOP", false);
           Debug.Log("backdashLOOP false");
       }



       // game state update
       gameState = GameState.InitiatingTurn;
       //gameState = GameState.AwaitingInput;

       Debug.Log("Moved back");

       if (myRoutine != null)
       {
           StopCoroutine(myRoutine);
           myRoutine = null;
       }
   }





   IEnumerator MoveToTarget()
   {
       Debug.Log("MoveToTarget starts");
       //yield return new WaitForSeconds(1); // Pauses here for 2 seconds
       //Debug.Log("name: " + sortedCharacterList[currentPlayerIndex].character_name + ", speed: " + sortedCharacterList[currentPlayerIndex].character_speed);
       float duration = 10f; // The total time the movement should take
       duration = 100f / sortedCharacterList[currentPlayerIndex].character_speed;
       //float attackProximityIndex = 0.9f;

       // find player and oponent objects
       _gameObjectP = GameObject.Find(sortedCharacterList[currentPlayerIndex].character_name);
       _gameObjectO = GameObject.Find(sortedCharacterList[currentOponentIndex].character_name);



       // calculate the vector from player position to oponent position
       moveVector = (_gameObjectO.transform.position - _gameObjectP.transform.position);
       Vector3 normalizedMoveVector = moveVector.normalized;
       moveVector = moveVector - normalizedMoveVector;
       //moveVector = (_gameObjectO.transform.position - _gameObjectP.transform.position)*attackProximityIndex;        

       Vector3 startPosition = _gameObjectP.transform.position;
       Vector3 targetPosition = startPosition + moveVector;

       float timeElapsed = 0;

       //Debug.Log("speed: " + sortedCharacterList[currentPlayerIndex].character_speed);
       while (timeElapsed < duration)
       {




           // Calculate the interpolation value (percentage of duration passed)
           float t = timeElapsed / duration;

           // Optional: apply easing (smooth start/end) using Mathf.SmoothStep
           // t = Mathf.SmoothStep(0f, 1f, t); 

           // Update position using Lerp
           _gameObjectP.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

           // Increment the time elapsed by the time since the last frame
           timeElapsed += Time.deltaTime;

           yield return null; // Wait until the next frame
       }

       yield return new WaitForSeconds(1); // Pauses here for 2 seconds


       // animation:
       // dash_loop => attack
       if (animPlayerIndex < 4)
       {
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", true);
           characterAnimRefs[animPlayerIndex].SetBool("dashLOOP", false);
           characterAnimRefs[animPlayerIndex].SetBool("attackOS", true);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
           Debug.Log("attack, dashLOOP false, attackOS true");
       }



       //Debug.Log("speed: " + sortedCharacterList[currentPlayerIndex].character_speed);
       // Ensure the object reaches the exact target position at the end
       _gameObjectP.transform.position = targetPosition + new Vector3(0f, 0f, 3f);


       //Debug.Log("Coroutine started! Waiting 2 seconds...");
       //pause for a second
       //yield return new WaitForSeconds(1); 
       //and update health and destroy the oponent if applicable
       updateHealth();
       updateLives();
       removeDeadPlayer();
       //pause another second before move back
       yield return new WaitForSeconds(1); // Pauses here for 2 seconds




       // MoveBack
       startPosition = _gameObjectP.transform.position - new Vector3(0f, 0f, 3f);
       targetPosition = startPosition - moveVector;

       timeElapsed = 0;


       // animation:
       // attack => backdash_os
       if (currentPlayerIndex < 4)
       {
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", true);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
           Debug.Log("backdash_os");
       }

       //Pause(3);

       // animation:
       // backdash_os => backdash_loop
       if (animPlayerIndex < 4)
       {
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
           characterAnimRefs[animPlayerIndex].SetBool("backdashOS", true);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", true);
           Debug.Log("backdashOS true");
       }

       while (timeElapsed < duration)
       {
           // Calculate the interpolation value (percentage of duration passed)
           float t = timeElapsed / duration;

           // Optional: apply easing (smooth start/end) using Mathf.SmoothStep
           // t = Mathf.SmoothStep(0f, 1f, t); 

           // Update position using Lerp
           _gameObjectP.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

           // Increment the time elapsed by the time since the last frame
           timeElapsed += Time.deltaTime;

           yield return null; // Wait until the next frame
       }

       //Debug.Log("speed: " + sortedCharacterList[currentPlayerIndex].character_speed);
       // Ensure the object reaches the exact target position at the end
       _gameObjectP.transform.position = targetPosition;

       //yield return new WaitForSeconds(1); // Pauses here for 2 seconds

       if (animPlayerIndex < 4)
       {
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", true); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
           characterAnimRefs[animPlayerIndex].SetBool("backdashLOOP", false);
           Debug.Log("backdashLOOP false");
       }



       // game state update
       gameState = GameState.InitiatingTurn;
       //gameState = GameState.AwaitingInput;

       //Debug.Log("MoveToTarget ends");
   }






   void handleBackdash()
   {
       Debug.Log("handleBackdash starts");
       // animation:
       // backdash_os => backdash_loop
       if (animPlayerIndex < 4)
       {
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
           //characterAnimRefs[animPlayerIndex].SetBool("backdashOS", true);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", true);
           Debug.Log("backdashOS true");
       }

       backdashRoutine = StartCoroutine(MovebackFromEnemy());
   }


   void handleAttack()
   {
       Debug.Log("handleAttack starts");

       // animation:
       // ready_loop => dash_os
       if (currentPlayerIndex < 4)
       {
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           characterAnimRefs[currentPlayerIndex].SetBool("dashOS", true);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
           Debug.Log("readyLOOP false, dash_os true");
       }

       //Pause(3);

       // animation:
       // dash_os => dash_loop
       if (currentPlayerIndex < 4)
       {
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", true);
           //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
           //Debug.Log("dash_loop");              
       }


       Debug.Log(gameState + " in handdleAttack");
       //Pause(3);
       //indx = new List<int> { currentPlayerIndex, currentOponentIndex };
       //attackAnimationData = getAttackAnimationData(indx, sortedCharacterList);
       //Debug.Log("Invoking AttackAnimation");
       //AttackAnimation?.Invoke(_gameObjectP, _gameObjectO);

       // call handleAttackAnimation function to move player close to oponent
       //handleAttackAnimation();

       StartCoroutine(MoveToTarget());


       //Debug.Log("before attack");
       //printCharacterData();
       //printHealthData("before attack");
       printAttackPowerData();

       updateHealth();

       //Debug.Log("after attack");
       printHealthData("after attack");
       //Pause(3);
       //printCharacterData();

       // destroy a character if no health
       //destroyCharacterAndIcon();

       //gameState = GameState.AttackOver;
       //gameState = GameState.InitiateTurn;



       //Pause(3);
       // animation:
       // backdash_loop =>idle
       if (currentPlayerIndex < 4)
       {
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", true); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
           Debug.Log("idle");
       }




       gameState = GameState.AwaitingInput;
       Debug.Log(gameState + " in handdleAttack");

       // update remaining lives
       // if no lives left on eather team, game is over
       updateLives();
       //checkForWin();
       //Debug.Log(getWinner() + " won the game");

       //isMyTurn = false;
       Debug.Log("handleAttack ends");
   }



   IEnumerator Pause(int num)
   {
       Debug.Log("Coroutine started! Waiting 3 seconds...");
       yield return new WaitForSeconds(num); // Pauses here for 3 seconds
       Debug.Log("3 seconds passed! Coroutine finished.");
   }



   IEnumerator MoveToEnemy()
   {
       Debug.Log("MoveToEnemy starts");
       //yield return new WaitForSeconds(1); // Pauses here for 2 seconds
       //Debug.Log("name: " + sortedCharacterList[currentPlayerIndex].character_name + ", speed: " + sortedCharacterList[currentPlayerIndex].character_speed);
       float duration = 10f; // The total time the movement should take
       duration = 100f / sortedCharacterList[currentPlayerIndex].character_speed;
       //float attackProximityIndex = 0.9f;

       // find player and oponent objects
       _gameObjectP = GameObject.Find(sortedCharacterList[currentPlayerIndex].character_name);
       _gameObjectO = GameObject.Find(sortedCharacterList[currentOponentIndex].character_name);



       // calculate the vector from player position to oponent position
       //moveVector = (_gameObjectO.transform.position - _gameObjectP.transform.position);
       //Vector3 normalizedMoveVector = moveVector.normalized;
       //moveVector = moveVector - normalizedMoveVector;
       //moveVector = (_gameObjectO.transform.position - _gameObjectP.transform.position)*attackProximityIndex;        

       playerPosition = _gameObjectP.transform.position;
       enemyPosition = _gameObjectO.transform.position;
       moveVector = enemyPosition - playerPosition;
       Vector3 normalizedMoveVector = moveVector.normalized;
       moveVector = moveVector - normalizedMoveVector;


       Vector3 startPosition = playerPosition;
       Vector3 targetPosition = playerPosition + moveVector;
       Debug.Log("start: " + startPosition);
       Debug.Log("target: " + targetPosition);
       Debug.Log("move vector: " + moveVector);

       float timeElapsed = 0;

       //Debug.Log("speed: " + sortedCharacterList[currentPlayerIndex].character_speed);
       while (timeElapsed < duration)
       {




           // Calculate the interpolation value (percentage of duration passed)
           float t = timeElapsed / duration;

           // Optional: apply easing (smooth start/end) using Mathf.SmoothStep
           // t = Mathf.SmoothStep(0f, 1f, t); 

           // Update position using Lerp
           _gameObjectP.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

           // Increment the time elapsed by the time since the last frame
           timeElapsed += Time.deltaTime;

           yield return null; // Wait until the next frame
       }

       yield return new WaitForSeconds(1); // Pauses here for 2 seconds

       gameState = GameState.OnAttack;

       if (myRoutine != null)
       {
           StopCoroutine(myRoutine);
           myRoutine = null;
       }
   }




   void handleDash()
   {
       Debug.Log("handleDash starts");

       // animation:
       // ready_loop => dash_os
       if (currentPlayerIndex < 4)
       {
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           characterAnimRefs[currentPlayerIndex].SetBool("dashOS", true);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
           Debug.Log("readyLOOP false, dash_os true");
       }

       dashRoutine = StartCoroutine(MoveToEnemy());


   }



   public void handleAwaitingInputPhase(string clickedEnemyName)
   {
       //Debug.Log("handleAwaitingInput starts");
       //anim.SetBool("isMyTurn", false);


       //Debug.Log("Player: " + sortedCharacterData[currentPlayerIndex].character_name);
       //printPlayOrder(playOrderDataList);

       Debug.Log(sortedCharacterList[currentPlayerIndex].character_name + " selected " + clickedEnemyName + " to attack.");
       currentOponentIndex = indexSortedCharacterData(clickedEnemyName);

       if (sortedCharacterList[currentOponentIndex].character_health > 0)
       {
           //Debug.Log("Friend Position:Enemy Position = " + (1 + currentPlayerIndex) + ":" + (1 + currentEnemyIndex));

           gameState = GameState.OnAttack;
           readyToClick = false;
           //Debug.Log(gameState + " in handleAwaitingInputPhase");


           if (currentPlayerIndex < 4)
           {
               //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
               //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
               //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
               //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", true);
               //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
               //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
               //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
               //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
               //Debug.Log("dash_os");              
           }


       }
       else
       {
           Debug.Log("The selected enemy is already dead. Choose another enemy.");
           readyToClick = true;
       }

       //readyToClick = false;
       //Debug.Log("handleAwaitingInput ends");
   }


   void resetAnimBoolean()
   {
       for (int i = 0; i < characterAnimRefs.Count; i++)
       {
           characterAnimRefs[i].SetBool("idle", false);
           characterAnimRefs[i].SetBool("readyOS", false);
           characterAnimRefs[i].SetBool("readyLOOP", false);
           characterAnimRefs[i].SetBool("dashOS", false);
           characterAnimRefs[i].SetBool("dashLOOP", false);
           characterAnimRefs[i].SetBool("attackOS", false);
           characterAnimRefs[i].SetBool("backdashOS", false);
           characterAnimRefs[i].SetBool("backdashLOOP", false);

           Debug.Log("player index to reset: " + i);
       }

   }

IEnumerator MoveToTarget()
   {
       //Debug.Log("MoveToTarget starts");
       //Debug.Log("name: " + sortedCharacterList[currentPlayerIndex].character_name + ", speed: " + sortedCharacterList[currentPlayerIndex].character_speed);
       float duration = 0.5f; // The total time the movement should take
       duration = 25f/sortedCharacterList[currentPlayerIndex].character_speed;
       //float attackProximityIndex = 0.9f;

       // find player and oponent objects
       _gameObjectP = GameObject.Find(sortedCharacterList[currentPlayerIndex].character_name);
       _gameObjectO = GameObject.Find(sortedCharacterList[currentOponentIndex].character_name);


       // animation:
       // attack_os => attack_on
       if(currentPlayerIndex < 4)
       {
           // trigger player attack animation
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", true);
           //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
           Debug.Log("attack_on");         
       }

       // calculate the vector from player position to oponent position
       moveVector = (_gameObjectO.transform.position - _gameObjectP.transform.position);
       Vector3 normalizedMoveVector = moveVector.normalized;
       moveVector = moveVector - normalizedMoveVector;
       //moveVector = (_gameObjectO.transform.position - _gameObjectP.transform.position)*attackProximityIndex;        

       Vector3 startPosition = _gameObjectP.transform.position;
       Vector3 targetPosition = startPosition + moveVector;

       float timeElapsed = 0;

       //Debug.Log("speed: " + sortedCharacterList[currentPlayerIndex].character_speed);
       while (timeElapsed < duration)
       {




           // Calculate the interpolation value (percentage of duration passed)
           float t = timeElapsed / duration;

           // Optional: apply easing (smooth start/end) using Mathf.SmoothStep
           // t = Mathf.SmoothStep(0f, 1f, t); 

           // Update position using Lerp
           _gameObjectP.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

           // Increment the time elapsed by the time since the last frame
           timeElapsed += Time.deltaTime;

           yield return null; // Wait until the next frame
       }

       // animation:
       // dash_loop => attack
       if(currentPlayerIndex < 4)
       {
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", true);
           characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
           characterAnimRefs[currentPlayerIndex].SetBool("attackOS", true);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
           Debug.Log("attack");              
       }

       Pause(3);


       //Debug.Log("speed: " + sortedCharacterList[currentPlayerIndex].character_speed);
       // Ensure the object reaches the exact target position at the end
       _gameObjectP.transform.position = targetPosition + new Vector3(0f, 0f, 3f);


       //Debug.Log("Coroutine started! Waiting 2 seconds...");
       //pause for a second
       yield return new WaitForSeconds(1); 
       //and update health and destroy the oponent if applicable
       updateHealth();
       updateLives();
       removeDeadPlayer();
       //pause another second before move back
       yield return new WaitForSeconds(1); // Pauses here for 2 seconds


       // MoveBack
       startPosition = _gameObjectP.transform.position - new Vector3(0f, 0f, 3f);
       targetPosition = startPosition - moveVector;

       timeElapsed = 0;


       // animation:
       // attack => backdash_os
       if(currentPlayerIndex < 4)
       {
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", true);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
           Debug.Log("backdash_os");              
       }

       Pause(3);

       // animation:
       // backdash_os => backdash_loop
       if(currentPlayerIndex < 4)
       {
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", false); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", true);
           Debug.Log("backdash_loop");              
       }

       while (timeElapsed < duration)
       {
           // Calculate the interpolation value (percentage of duration passed)
           float t = timeElapsed / duration;

           // Optional: apply easing (smooth start/end) using Mathf.SmoothStep
           // t = Mathf.SmoothStep(0f, 1f, t); 

           // Update position using Lerp
           _gameObjectP.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

           // Increment the time elapsed by the time since the last frame
           timeElapsed += Time.deltaTime;

           yield return null; // Wait until the next frame
       }

       //Debug.Log("speed: " + sortedCharacterList[currentPlayerIndex].character_speed);
       // Ensure the object reaches the exact target position at the end
       _gameObjectP.transform.position = targetPosition;

       if(currentPlayerIndex < 4)
       {
           //characterAnimRefs[currentPlayerIndex].SetBool("idle", true); 
           //characterAnimRefs[currentPlayerIndex].SetBool("readyOS", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("readyLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("dashOS", false);
           characterAnimRefs[currentPlayerIndex].SetBool("dashLOOP", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("attack", false);
           //characterAnimRefs[currentPlayerIndex].SetBool("backdashOS", false);
           characterAnimRefs[currentPlayerIndex].SetBool("backdashLOOP", false);
           Debug.Log("backdash_loop");              
       }



       // game state update
       gameState = GameState.InitiatingTurn;
       //gameState = GameState.AwaitingInput;

       //Debug.Log("MoveToTarget ends");
   }


   void handleGameFlow()
   {
       switch (gameState)
       {
           //case GameState.DataNotReady:
           //    break;

           case GameState.AwaitingInput:
               //choosePlayer();
               break;

           case GameState.AttackOn:
               //Debug.Log(isMyTurn);
               handleAttack();
               break;

           case GameState.InitiatingTurn:
               //Debug.Log(isMyTurn);
               initiateTurn();
               break;

           default:
               break;
       }
   }




   IEnumerator Pause(int num)
   {
       Debug.Log("Coroutine started! Waiting 3 seconds...");
       yield return new WaitForSeconds(num); // Pauses here for 3 seconds
       Debug.Log("3 seconds passed! Coroutine finished.");
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
       //_gameObjectP.transform.position = Vector3.Lerp(_gameObjectP.transform.position, _gameObjectP.transform.position + moveVector, 1f);

       // Code here runs after the delay
       //Invoke("microTimer", 2f);


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

   public void sendSortedCharacterData(List<CharacterData> characters)
   {
       Debug.Log("sendSortedCharacterData starts");
       //Debug.Log("Receiving character data in TurnManager");
       sortedCharacterList = characters;
       //dataReceived = true;

       // print character data
       printCharacterList(characters);
       // generatePlayOrder
       Debug.Log("Calling generatePlayOrder");
       generatePlayOrder();

       gameState = GameState.AwaitingInput;

       //Debug.Log("sendSortedCharacterDate successful.");
       //Debug.Log("GameState in TurnManager, sendSortedCharacterData: " + gameState);
       Debug.Log("sendSortedCharacterData ends");
   }



   public void getCharacterData2(List<CharacterData> characters)
   {
       Debug.Log("getCharacterData starts");
       //Debug.Log("Receiving character data in TurnManager");
       sortedCharacterList = characters;
       //dataReceived = true;

       // print character data
       //printCharacterList(characters);
       // generatePlayOrder
       Debug.Log("Calling generatePlayOrder");
       generatePlayOrder();

       gameState = GameState.AwaitingInput;

       //Debug.Log("sendSortedCharacterDate successful.");
       //Debug.Log("GameState in TurnManager, sendSortedCharacterData: " + gameState);
       Debug.Log("getCharacterData ends");
   }


















       if (gameState == GameState.GameOver)
       {
           Debug.Log("Quitting Game!");
           return;


           //UnityEditor.EditorApplication.isPlaying = false;
       }
       else if (gameState == GameState.DataNotReady)
       {
           Debug.Log("Character Data Not Ready Yet!");   
           characterScript.loadCharacterData();         

       }

       else if (gameState == GameState.AwaitingInput)
       {
           Debug.Log("Waiting for player to choose oponent to attack.");

       }


       else if (gameState == GameState.AttackOn)
       {
           Debug.Log("Attacking oponent!");
           handleAttack();            

       }

       else if (gameState == GameState.InitiateTurn)
       {
           Debug.Log("Moving on to the next player in line.");
           initiateTurn();
       }
       else
       {
           Debug.Log("error?");
           return;
       }

*/
