using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class TitleUIToolkit : MonoBehaviour
{
    private TextField playerNameInput;
    private Button playerButton;
    private Button npcButton;
    
    private void OnEnable()
    {
        // Get the root visual element
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        
        // Query UI elements
        playerNameInput = root.Q<TextField>("player-name-input");
        playerButton = root.Q<Button>("player-button");
        npcButton = root.Q<Button>("npc-button");
        
        // Load saved player name
        string savedName = PlayerPrefs.GetString("PlayerName", "Player");
        playerNameInput.value = savedName;
        
        // Register button callbacks
        playerButton.clicked += OnPlayerButtonClicked;
        npcButton.clicked += OnNpcButtonClicked;
    }
    
    private void OnDisable()
    {
        // Unregister callbacks to prevent memory leaks
        playerButton.clicked -= OnPlayerButtonClicked;
        npcButton.clicked -= OnNpcButtonClicked;
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
        string playerName = playerNameInput.value;
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Player";
        }
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
    }
}