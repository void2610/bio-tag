using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class TitleUIToolkit : MonoBehaviour
{
    private TextField _playerNameInput;
    private Button _playerButton;
    private Button _npcButton;
    
    private void OnEnable()
    {
        // Get the root visual element
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        
        // Query UI elements
        _playerNameInput = root.Q<TextField>("player-name-input");
        _playerButton = root.Q<Button>("player-button");
        _npcButton = root.Q<Button>("npc-button");
        
        // Load saved player name
        string savedName = PlayerPrefs.GetString("PlayerName", "Player");
        _playerNameInput.value = savedName;
        
        // Register button callbacks
        _playerButton.clicked += OnPlayerButtonClicked;
        _npcButton.clicked += OnNpcButtonClicked;
    }
    
    private void OnDisable()
    {
        // Unregister callbacks to prevent memory leaks
        _playerButton.clicked -= OnPlayerButtonClicked;
        _npcButton.clicked -= OnNpcButtonClicked;
    }
    
    private void OnPlayerButtonClicked()
    {
        SavePlayerName();
        SceneManager.LoadScene("WithPlayer");
    }
    
    private void OnNpcButtonClicked()
    {
        SavePlayerName();
        SceneManager.LoadScene("WithNPC");
    }
    
    private void SavePlayerName()
    {
        var playerName = _playerNameInput.value;
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "NoName";
        }
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
    }
}