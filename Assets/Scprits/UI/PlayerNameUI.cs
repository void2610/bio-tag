using UnityEngine;
using TMPro;

public class PlayerNameUI : MonoBehaviour
{
    private TMP_Text playerNameText;
    private GameObject targetPlayer;
    private Transform playerCamera;

    public void SetTargetPlayer(GameObject player, string playerName)
    {
        targetPlayer = player;
        playerNameText = GetComponent<TMP_Text>();
        playerNameText.text = playerName;
        playerCamera = GameObject.Find("PlayerCamera")?.transform;
    }

    void Start()
    {
        playerNameText = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (targetPlayer == null)
        {
            return;
        }
        if (playerCamera == null)
        {
            playerCamera = GameObject.Find("PlayerCamera").transform;
        }
        this.transform.position = targetPlayer.transform.position + new Vector3(0, 2.5f, 0);
        // プレイヤーの名前UIをカメラの方向に向ける
        this.transform.rotation = playerCamera.rotation;
    }
}
