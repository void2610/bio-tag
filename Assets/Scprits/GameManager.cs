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
        if (PhotonNetwork.IsMasterClient)
        {
            if (!PhotonNetwork.CurrentRoom.IsGameStarted())
            {
                if (IsAllPlayerReady())
                {
                    StartGame();
                }
            }
            else
            {
                playerScores[itIndex - 1] += Time.deltaTime;
            }
        }

        if (PhotonNetwork.CurrentRoom != null && !PhotonNetwork.CurrentRoom.IsGameStarted())
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
    }
}
