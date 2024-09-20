using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NPCGameManager : GameManagerBase
{
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject npcPrefab;
    [SerializeField]
    private GameMessageUI messageUI;
    [SerializeField]
    private int npcCount = 1;

    private List<GameObject> players = new List<GameObject>();

    private bool isPlayerReady = false;
    private float startTime;

    public override void StartGame()
    {
        gameState = 1;
        messageUI.SetMessage("");
        playerScores.Clear();
        for (int i = 0; i < npcCount + 1; i++)
        {
            playerScores.Add(0);
        }
        itIndex = Random.Range(0, npcCount + 1);
        startTime = Time.time;
        itMarker.SetTarget(players[itIndex].transform);
    }

    public float getElapsedTime()
    {
        return Time.time - startTime;
    }

    public override void ChangeIt(int index)
    {
        if (Time.time - this.lastTagTime > 1 && this.itIndex != index && this.gameState == 1)
        {
            itIndex = index;
            lastTagTime = Time.time;
            itMarker.SetTarget(players[itIndex].transform);
        }
    }

    protected override bool IsAllPlayerReady()
    {
        return isPlayerReady;
    }

    protected float GetPlayerDistance()
    {
        return Vector3.Distance(players[0].transform.position, players[1].transform.position);
    }

    protected override void Awake()
    {
        base.Awake();
        var p = Instantiate(playerPrefab, new Vector3(Random.Range(-3f, 3f), 1, Random.Range(-3f, 3f)), Quaternion.identity);
        players.Add(p);
        p.GetComponent<SinglePlayer>().index = 0;
        playerNames.Add(PlayerPrefs.GetString("PlayerName", "No Name"));
        for (int i = 1; i < npcCount + 1; i++)
        {
            var n = Instantiate(npcPrefab, new Vector3(Random.Range(-3f, 3f), 1, Random.Range(-3f, 3f)), Quaternion.identity);
            players.Add(n);
            n.GetComponent<NPC>().index = i;
            n.GetComponent<NPC>().SetTarget(p.transform);
            playerNames.Add("NPC" + i);
        }
    }

    protected override void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        gameState = 0;
    }

    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            ChangeIt(1);
        }
        if (gameState == 0)
        {
            if (IsAllPlayerReady())
            {
                StartGame();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                isPlayerReady = true;
            }
        }
        else if (gameState == 1)
        {
            playerScores[itIndex] += Time.deltaTime;
            if (getElapsedTime() >= gameLength)
            {
                gameState = 2;
                Debug.Log("Game Over");
            }
            UDP.instance.SendData(GetPlayerDistance());
        }
        else if (gameState == 2)
        {
            messageUI.SetMessage("Game Over");
        }
    }
}
