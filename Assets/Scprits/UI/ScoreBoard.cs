using System;
using System.Text;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{
    private TextMeshProUGUI label;

    private StringBuilder builder;
    private float elapsedTime;

    private void Start()
    {
        label = this.GetComponent<TextMeshProUGUI>();
        builder = new StringBuilder();
        elapsedTime = 0f;
    }

    private void Update()
    {
        // まだルームに参加していない場合は更新しない
        if (!PhotonNetwork.InRoom) { return; }

        // 0.1秒毎にテキストを更新する
        elapsedTime += Time.deltaTime;
        if (elapsedTime > 0.1f)
        {
            elapsedTime = 0f;
            UpdateLabel();
        }
    }

    private void UpdateLabel()
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
}