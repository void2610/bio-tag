using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Photon.Pun;

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
        itIndex = Random.Range(0, PhotonNetwork.PlayerList.Length - 1);
        PhotonNetwork.CurrentRoom.StartGame(PhotonNetwork.ServerTimestamp);
    }

    public void ChangeIt(int index)
    {
        itIndex = index;
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Invoke("StartGame", 3.0f);
        }
    }
}
