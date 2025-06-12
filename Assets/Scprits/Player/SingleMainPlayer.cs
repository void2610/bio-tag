using UnityEngine;
using UnityEngine.VFX;

public class SingleMainPlayer : PlayerBase
{
    private void OnTriggerEnter(Collider other)
    {
        if (Gm?.GameState != 1) return;
        Gm?.ChangeIt(Index);
    }
}
