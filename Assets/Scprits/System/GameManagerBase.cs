using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public abstract class GameManagerBase : MonoBehaviour
{
    public static GameManagerBase Instance;
    protected virtual void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(this.gameObject);
    }

    [SerializeField] protected ItMarker itMarker;
    [SerializeField] protected float gameLength = 60.0f;
    public int? GameState { get; protected set; } = 0;
    public int itIndex;
    public float lastTagTime;
    public List<float> playerScores = new ();
    public List<string> playerNames = new ();

    private PlayerBase _mainPlayer;

    public virtual void ChangeIt(int index) { }

    public void SetMainPlayer(PlayerBase p) => _mainPlayer = p;
    public PlayerBase GetMainPlayer() => _mainPlayer;

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
