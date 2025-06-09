using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class OfflinePlayerGameManager : GameManagerBase
{
    [SerializeField] private GameObject mainPlayerPrefab;
    [SerializeField] private GameObject subPlayerPrefab;
    [SerializeField] private List<GameUIToolkit> gameUIs = new List<GameUIToolkit>();
    [SerializeField] private int npcCount = 1;

    private readonly List<GameObject> _players = new ();
    private bool _isPlayerReady = false;
    private float _startTime;

    public override void StartGame()
    {
        GameState = 1;
        SendMessageToAllUIs("", GameUIToolkit.MessageType.Default); // Clear messages
        playerScores.Clear();
        for (var i = 0; i < npcCount + 1; i++)
        {
            playerScores.Add(0);
        }
        itIndex = Random.Range(0, npcCount + 1);
        _startTime = Time.time;
        itMarker.SetTarget(_players[itIndex].transform);
    }
    
    private void SendMessageToAllUIs(string message, GameUIToolkit.MessageType messageType)
    {
        foreach (var uiToolkit in gameUIs)
        {
            if (uiToolkit != null)
            {
                if (string.IsNullOrEmpty(message))
                {
                    uiToolkit.ClearMessage();
                }
                else
                {
                    uiToolkit.SetMessage(message, messageType);
                }
            }
        }
    }
    
    public float GetElapsedTime()
    {
        return Time.time - _startTime;
    }

    public override void ChangeIt(int index)
    {
        if (Time.time - this.lastTagTime > 1 && this.itIndex != index && this.GameState == 1)
        {
            itIndex = index;
            lastTagTime = Time.time;
            itMarker.SetTarget(_players[itIndex].transform);
        }
    }

    protected override bool IsAllPlayerReady()
    {
        return _isPlayerReady;
    }

    protected float GetPlayerDistance()
    {
        return Vector3.Distance(_players[0].transform.position, _players[1].transform.position);
    }

    protected override void Awake()
    {
        base.Awake();
        
        // If no GameUIToolkits are assigned in inspector, find all in the scene
        if (gameUIs.Count == 0)
        {
            gameUIs.AddRange(FindObjectsByType<GameUIToolkit>(FindObjectsSortMode.None));
        }
        
        var mainP = Instantiate(mainPlayerPrefab, new Vector3(Random.Range(-3f, 3f), 1, Random.Range(-3f, 3f)), Quaternion.identity);
        _players.Add(mainP);
        mainP.GetComponent<PlayerBase>().index = 0;
        playerNames.Add(PlayerPrefs.GetString("PlayerName", "No Name"));
        for (var i = 1; i < npcCount + 1; i++)
        {
            var subP = Instantiate(subPlayerPrefab, new Vector3(Random.Range(-3f, 3f), 1, Random.Range(-3f, 3f)), Quaternion.identity);
            _players.Add(subP);
            subP.GetComponent<PlayerBase>().index = i;
            playerNames.Add("Player" + i);
        }
        
        Debug.Log ("displays connected: " + Display.displays.Length);
        // Display.displays[0] は主要なデフォルトのディスプレイで、常にオンです。ですから、インデックス 1 から始まります。
        for (var i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }

    protected override void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        GameState = 0;
        SendMessageToAllUIs("Press F to Start Game", GameUIToolkit.MessageType.Info);
    }

    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            ChangeIt(1);
        }
        if (GameState == 0)
        {
            if (IsAllPlayerReady())
            {
                StartGame();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                _isPlayerReady = true;
            }
        }
        else if (GameState == 1)
        {
            playerScores[itIndex] += Time.deltaTime;
            if (GetElapsedTime() >= gameLength)
            {
                GameState = 2;
                Debug.Log("Game Over");
            }
            UDP.instance.SendData(GetPlayerDistance());
        }
        else if (GameState == 2)
        {
            SendMessageToAllUIs("Game Over", GameUIToolkit.MessageType.Info);
        }
    }
}
