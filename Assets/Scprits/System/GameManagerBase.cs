using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class GameManagerBase : MonoBehaviourPunCallbacks
{
    public static GameManagerBase instance;
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public int? gameState { get; protected set; } = 0;
    protected float gameLength = 30.0f;
    protected int itIndex;
    protected List<float> playerScores = new List<float>();

    public virtual void StartGame()
    {
    }

    protected virtual bool IsAllPlayerReady()
    {
        return false;
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
    }
}
