using UnityEngine;
using TMPro;

public class PlayerNameUI : MonoBehaviour
{
    private TMP_Text playerNameText;
    private GameObject targetPlayer;

    public void SetTargetPlayer(GameObject player, string playerName)
    {
        targetPlayer = player;
        playerNameText = GetComponent<TMP_Text>();
        playerNameText.text = playerName;
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

        this.transform.position = targetPlayer.transform.position + new Vector3(0, 2, 0);

    }
}
