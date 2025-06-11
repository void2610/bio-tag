using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

public class TitleUIToolkit : MonoBehaviour
{
    [Inject] private IPlayerDataService _playerDataService;
    [Inject] private ISceneService _sceneService;
    
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
        _playerNameInput.value = _playerDataService.GetPlayerName();
        
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
        _sceneService.LoadPlayerScene();
    }

    private void OnNpcButtonClicked()
    {
        SavePlayerName();
        _sceneService.LoadNpcScene();
    }
    
    private void SavePlayerName()
    {
        var playerName = _playerNameInput.value;
        _playerDataService.SetPlayerName(playerName);
    }
}