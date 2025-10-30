using UnityEngine;
using UnityEngine.VFX;
using VitalRouter;
using BioTag.GameUI;

public class SingleMainPlayer : PlayerBase
{
    private void OnTriggerEnter(Collider other)
    {
        if (Gm?.GameState != 1) return;
        Router.Default.PublishAsync(new PlayerTaggedCommand(Index));
    }
}
