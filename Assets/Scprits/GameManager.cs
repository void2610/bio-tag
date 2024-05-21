using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    private void Awake()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Destroy(this);
            return;
        }

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    private List<float> playerScores = new List<float>();
    private int playerCount = 0;
    private bool onGame = false;
    private int itIndex;

    public void StartGame()
    {
        playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
        playerScores.Clear();
        for (int i = 0; i < playerCount; i++)
        {
            playerScores.Add(0);
        }
        itIndex = Random.Range(0, playerCount);
    }

    public void ChangeIt(int index)
    {
        itIndex = index;
    }

    private void Start()
    {
        // string playerName = PlayerPrefs.GetString("PlayerName", "DefaultName");
        // PlayerNetworkManager.Instance.SetPlayerName(playerName);

        //3秒後にゲームを開始
        Invoke("StartGame", 3);
    }

    public void Update()
    {
        if (!IsServer) return;

        if (onGame)
        {
            playerScores[itIndex] += Time.deltaTime;
        }
    }
}
