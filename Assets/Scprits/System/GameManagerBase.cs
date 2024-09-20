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
    [SerializeField]
    protected ItMarker itMarker;
    [SerializeField]
    protected float gameLength = 60.0f;
    public int itIndex;
    public float lastTagTime;
    public List<float> playerScores = new List<float>();
    public List<string> playerNames = new List<string>();

    public virtual void ChangeIt(int index)
    {
    }

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
