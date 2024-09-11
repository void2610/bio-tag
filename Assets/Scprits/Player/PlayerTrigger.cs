using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject player;

    private float lastTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (GameManagerBase.instance.gameState != 1)
        {
            return;
        }

        if (other.CompareTag("ItPlayer") && Time.time - lastTime > 3f)
        {
            if (GameManagerBase.instance.GetType() == typeof(NetworkGameManager))
            {
                // ネットワーク対戦
            }
            else
            {
                // 吹き飛ばされる
                Vector3 direction = (player.transform.position - other.transform.position).normalized;
                direction.y = 1f;
                player.GetComponent<SinglePlayer>().isGrounded = false;
                player.GetComponent<SinglePlayer>().velocity = direction * 7f;

                other.GetComponent<NPC>().Wait(3f);
            }
            lastTime = Time.time;
        }
    }
}
