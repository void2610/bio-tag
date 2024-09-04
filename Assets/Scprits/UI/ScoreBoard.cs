using System;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{
    private TextMeshProUGUI label => this.GetComponent<TextMeshProUGUI>();

    private StringBuilder builder;
    private float elapsedTime;
    private bool isNetworked = false;

    private void Start()
    {
        builder = new StringBuilder();
        elapsedTime = 0f;

        if (GameManagerBase.instance.GetType() == typeof(NetworkGameManager))
        {
            isNetworked = true;
        }
    }

    private void Update()
    {
        // まだルームに参加していない場合は更新しない


        // 0.1秒毎にテキストを更新する
        elapsedTime += Time.deltaTime;
        if (isNetworked)
        {
            if (elapsedTime > 0.1f)
            {
                if (!PhotonNetwork.InRoom) { return; }
                elapsedTime = 0f;
                UpdateLabelNetwork();
            }
        }
        else
        {
            UpdateLabel();
        }
    }

    private void UpdateLabelNetwork()
    {
        var players = PhotonNetwork.PlayerList;
        Array.Sort(
            players,
            (p1, p2) =>
            {
                // スコアが少ない順にソートする
                int diff = p1.GetScore() - p2.GetScore();
                if (diff != 0)
                {
                    return diff;
                }
                return p1.ActorNumber - p2.ActorNumber;
            }
        );

        builder.Clear();
        foreach (var player in players)
        {
            builder.AppendLine($"{player.NickName} : {player.GetScore()}");
        }
        label.text = builder.ToString();
    }

    private void UpdateLabel()
    {
        List<float> s = GameManagerBase.instance.playerScores;
        List<string> n = GameManagerBase.instance.playerNames;

        // StringBuilderを使用してスコアを表示
        builder.Clear();
        var playerData = new List<(string name, float score)>();

        for (int i = 0; i < s.Count; i++)
        {
            playerData.Add((n[i], s[i]));
        }

        // スコアが少ない順にソート
        playerData.Sort((p1, p2) => p1.score.CompareTo(p2.score));

        foreach (var player in playerData)
        {
            int score = (int)player.score;
            builder.AppendLine($"{player.name} : {score}");
        }
        label.text = builder.ToString();
    }
}
