using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject player;

    private void OnTriggerEnter(Collider other)
    {
        if (GameManagerBase.instance.gameState != 1)
        {
            return;
        }

        if (other.CompareTag("ItPlayer"))
        {
            if (GameManagerBase.instance.GetType() == typeof(NetworkGameManager))
            {
                // ネットワーク対戦
            }
            else
            {
                NPCGameManager.instance.ChangeIt(player.GetComponent<SinglePlayer>().index);
                other.GetComponent<NPC>().Wait(2f);
            }
        }
    }
}
