using UnityEngine;
using TMPro;

public class PlayerNameUI : MonoBehaviour
{
    private TMP_Text _playerNameText;
    private GameObject _targetPlayer;
    private Transform _playerCamera;
    public int index = -1;

    public void SetTargetPlayer(GameObject player, int index)
    {
        _targetPlayer = player;
        _playerNameText = GetComponent<TMP_Text>();
        _playerNameText.text = "player" + index;
        _playerCamera = GameObject.Find("PlayerCamera" + index)?.transform;
        this.index = index;
    }

    private void Start()
    {
        _playerNameText = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_targetPlayer)
            return;
        if (!_playerCamera)
            _playerCamera = GameObject.Find("PlayerCamera" + index).transform;
        
        this.transform.position = _targetPlayer.transform.position + new Vector3(0, 2.5f, 0);
        // プレイヤーの名前UIをカメラの方向に向ける
        this.transform.rotation = _playerCamera.rotation;
    }
}
