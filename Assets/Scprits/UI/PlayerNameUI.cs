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
        
        // Try to find the player camera if not already set
        if (!_playerCamera)
        {
            var cameraObject = GameObject.Find("PlayerCamera" + index);
            if (cameraObject != null)
            {
                _playerCamera = cameraObject.transform;
            }
        }
        
        this.transform.position = _targetPlayer.transform.position + new Vector3(0, 2.5f, 0);
        
        // プレイヤーの名前UIをカメラの方向に向ける
        if (_playerCamera != null)
        {
            this.transform.rotation = _playerCamera.rotation;
        }
        else
        {
            // If no player camera exists (e.g., for NPCs), face the main camera
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                this.transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward);
            }
        }
    }
}
