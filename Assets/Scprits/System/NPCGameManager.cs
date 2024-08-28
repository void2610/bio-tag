using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NPCGameManager : GameManagerBase
{
    [SerializeField]
    private GameMessageUI messageUI;
    [SerializeField]
    private int npcCount = 1;

    private bool isPlayerReady = false;
    private float startTime;

    public override void StartGame()
    {
        playerScores.Clear();
        for (int i = 0; i < npcCount + 1; i++)
        {
            playerScores.Add(0);
        }
        itIndex = Random.Range(1, npcCount + 2);
        startTime = Time.time;
    }

    public float getElapsedTime()
    {
        return Time.time - startTime;
    }

    private bool IsAllPlayerReady()
    {
        return isPlayerReady;
    }

    protected override void Start()
    {
        gameState = 0;
    }

    protected override void Update()
    {
        if (gameState == 0)
        {
            if (IsAllPlayerReady())
            {
                StartGame();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                messageUI.SetMessage("Wait for other player to ready");
                isPlayerReady = true;
            }
        }
        else if (gameState == 1)
        {
            playerScores[itIndex - 1] += Time.deltaTime;
            if (getElapsedTime() >= gameLength)
            {
                gameState = 2;
                Debug.Log("Game Over");
            }
        }
        else if (gameState == 2)
        {
            messageUI.SetMessage("Game Over");
        }
    }
}
