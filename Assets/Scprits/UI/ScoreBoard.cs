using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{
    private TextMeshProUGUI Label => this.GetComponent<TextMeshProUGUI>();

    private StringBuilder _builder;
    private float _elapsedTime;

    private void Start()
    {
        _builder = new StringBuilder();
        _elapsedTime = 0f;
    }

    private void Update()
    {
        // 0.1秒毎にテキストを更新する
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime > 0.1f)
        {
            _elapsedTime = 0f;
            UpdateLabel();
        }
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
