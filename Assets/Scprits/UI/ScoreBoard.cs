using System;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{
    private TextMeshProUGUI Label => this.GetComponent<TextMeshProUGUI>();

    private StringBuilder _builder;
    private float _elapsedTime;
    private bool _isNetworked = false;

    private void Start()
    {
        _builder = new StringBuilder();
        _elapsedTime = 0f;

        if (GameManagerBase.Instance.GetType() == typeof(NetworkGameManager))
        {
            _isNetworked = true;
        }
    }

    private void Update()
    {
        // まだルームに参加していない場合は更新しない


        // 0.1秒毎にテキストを更新する
        _elapsedTime += Time.deltaTime;
        if (_isNetworked)
        {
            if (_elapsedTime > 0.1f)
            {
                if (!PhotonNetwork.InRoom) { return; }
                _elapsedTime = 0f;
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
                var diff = p1.GetScore() - p2.GetScore();
                if (diff != 0)
                {
                    return diff;
                }
                return p1.ActorNumber - p2.ActorNumber;
            }
        );

        _builder.Clear();
        foreach (var player in players)
        {
            _builder.AppendLine($"{player.NickName} : {player.GetScore()}");
        }
        Label.text = _builder.ToString();
    }

    private void UpdateLabel()
    {
        var s = GameManagerBase.Instance.playerScores;
        var n = GameManagerBase.Instance.playerNames;

        // StringBuilderを使用してスコアを表示
        _builder.Clear();
        var playerData = new List<(string name, float score)>();

        for (var i = 0; i < s.Count; i++)
        {
            playerData.Add((n[i], s[i]));
        }

        // スコアが少ない順にソート
        playerData.Sort((p1, p2) => p1.score.CompareTo(p2.score));

        foreach (var player in playerData)
        {
            var score = (int)player.score;
            _builder.AppendLine($"{player.name} : {score}");
        }
        Label.text = _builder.ToString();
    }
}
