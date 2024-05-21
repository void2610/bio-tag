using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class GameManager : NetworkBehaviour
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
        }
        DontDestroyOnLoad(this.gameObject);
    }

    //private List<float> playerScores = new List<float>();
    // private int playerCount = 0;
    // private int itIndex;
    public NetworkVariable<float> TimerValue = new NetworkVariable<float>();
    public NetworkVariable<bool> OnGame = new NetworkVariable<bool>();

    public void StartGame()
    {
        // playerCount = PlayerManager.Instance.GetPlayerCount();
        // playerScores.Clear();
        // for (int i = 0; i < playerCount; i++)
        // {
        //     playerScores.Add(0);
        // }
        // itIndex = Random.Range(0, playerCount);
        OnGame.Value = true;
    }

    public void ChangeIt(int index)
    {
        //itIndex = index;
    }

    public override void OnNetworkSpawn()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        TimerValue.Value = 0;
        OnGame.Value = false;

        //3秒後にゲームを開始
        Invoke("StartGame", 3);
    }

    public void Start()
    {
        if (!NetworkManager.Singleton.IsServer) return;
    }

    public void Update()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (OnGame.Value)
        {
            TimerValue.Value += Time.deltaTime;
        }
    }
}
