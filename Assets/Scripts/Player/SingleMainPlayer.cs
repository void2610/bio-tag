using UnityEngine;
using UnityEngine.VFX;
using VitalRouter;
using BioTag.GameUI;

/// <summary>
/// WithPlayerシーンのメインプレイヤー
/// タグ付けイベントをVitalRouter Commandで発行
/// </summary>
public class SingleMainPlayer : PlayerBase
{
    private void OnTriggerEnter(Collider other)
    {
        if (Gm?.GameState != 1) return;

        // 自分自身のTransformを含めてCommandを発行
        Router.Default.PublishAsync(new PlayerTaggedCommand(Index, transform));
    }
}
