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
    [SerializeField]
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
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Invoke("StartGame", 3.0f);
        }
    }
}
