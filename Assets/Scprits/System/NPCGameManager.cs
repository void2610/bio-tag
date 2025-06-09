using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NpcGameManager : GameManagerBase
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private GameUIToolkit gameUI;
    [SerializeField] private int npcCount = 1;

    private readonly List<GameObject> _players = new ();
    private bool _isPlayerReady = false;
    private float _startTime;

    public override void StartGame()
    {
        GameState = 1;
        gameUI.ClearMessage();
        playerScores.Clear();
        for (var i = 0; i < npcCount + 1; i++)
        {
            playerScores.Add(0);
        }
        itIndex = Random.Range(0, npcCount + 1);
        _startTime = Time.time;
        itMarker.SetTarget(_players[itIndex].transform);
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
        var p = Instantiate(playerPrefab, new Vector3(Random.Range(-3f, 3f), 1, Random.Range(-3f, 3f)), Quaternion.identity);
        _players.Add(p);
        p.GetComponent<PlayerBase>().index = 0;
        playerNames.Add(PlayerPrefs.GetString("PlayerName", "No Name"));
        for (var i = 1; i < npcCount + 1; i++)
        {
            var n = Instantiate(npcPrefab, new Vector3(Random.Range(-3f, 3f), 1, Random.Range(-3f, 3f)), Quaternion.identity);
            _players.Add(n);
            n.GetComponent<Npc>().index = i;
            n.GetComponent<Npc>().SetTarget(p.transform);
            playerNames.Add("NPC" + i);
        }
    }

    protected override void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        GameState = 0;
        gameUI.SetMessage("Press F to Start Game", GameUIToolkit.MessageType.Info);
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
            gameUI.SetMessage("Game Over", GameUIToolkit.MessageType.Info);
        }
    }
}
