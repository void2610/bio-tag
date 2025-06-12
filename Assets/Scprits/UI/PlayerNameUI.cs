using UnityEngine;
using TMPro;

public class PlayerNameUI : MonoBehaviour
{
    private TMP_Text _playerNameText;
    private Transform _targetPlayer;
    private Transform _playerCamera;
    private string _playerName;

    // Remove [Inject] - this method will be called manually after instantiation
    public void Initialize(Transform player, string playerName)
    {
        _targetPlayer = player;
        _playerName = playerName;
        _playerNameText = GetComponent<TMP_Text>();
        _playerNameText.text = _playerName;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_targetPlayer) return;
            
        this.transform.position = _targetPlayer.transform.position + new Vector3(0, 2.5f, 0);
        
        // プレイヤーの名前UIをカメラの方向に向ける
        // if (_playerCamera)
        // {
        //     this.transform.rotation = _playerCamera.rotation;
        // }
    }
}
