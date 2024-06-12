using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            DontDestroyOnLoad(this.gameObject);
        }
    }

    [SerializeField]
    private GameMessageUI messageUI;
    private List<float> playerScores = new List<float>();
    private int itIndex;
    private float gameLength = 10.0f;
    public float TimerValue = 0.0f;


    public void StartGame()
    {
        playerScores.Clear();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            playerScores.Add(0);
        }
        itIndex = Random.Range(1, PhotonNetwork.PlayerList.Length + 1);
        PhotonNetwork.CurrentRoom.SetItIndex(itIndex, PhotonNetwork.ServerTimestamp);
        PhotonNetwork.CurrentRoom.StartGame(PhotonNetwork.ServerTimestamp);

        InvokeRepeating("SendScore", 0, 0.1f);
    }

    private bool IsAllPlayerReady()
    {
        if (PhotonNetwork.PlayerList.Length < 2)
        {
            return false;
        }
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.IsReady())
            {
                return false;
            }
        }
        return true;
    }

    private void SendScore()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (itIndex == player.ActorNumber)
            {
                player.SetScore((int)playerScores[player.ActorNumber - 1]);
            }
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.InRoom) { return; }
        PhotonNetwork.CurrentRoom.TryGetStartTime(out int gameState);
        Debug.Log("Game State: " + gameState);
        // マスタークライアントのみで実行
        if (PhotonNetwork.IsMasterClient)
        {
            if (gameState == 0)
            {
                if (IsAllPlayerReady())
                {
                    StartGame();
                }
            }
            else if (gameState == 1)
            {
                playerScores[itIndex - 1] += Time.deltaTime;
                PhotonNetwork.CurrentRoom.TryGetStartTime(out int timestamp);
                float elapsedTime = Mathf.Max(0f, unchecked(PhotonNetwork.ServerTimestamp - timestamp) / 1000f);
                if (elapsedTime >= gameLength)
                {
                    CancelInvoke("SendScore");
                    PhotonNetwork.CurrentRoom.EndGame();
                }
            }
        }

        // 全てのクライアントで実行
        if (gameState == 0)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                messageUI.SetMessage("Wait for other player to ready");
                PhotonNetwork.LocalPlayer.SetReady(true);
            }
        }
    }

    public override void OnJoinedRoom()
    {
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(GameRoomProperty.KeyItIndex))
        {
            PhotonNetwork.CurrentRoom.TryGetItIndex(out itIndex);
        }
        if (propertiesThatChanged.ContainsKey(GameRoomProperty.KeyGameState))
        {
            PhotonNetwork.CurrentRoom.TryGetGameState(out int gameState);
            if (gameState == 2)
            {
                messageUI.SetMessage("Game Over");
            }
        }
    }
}
