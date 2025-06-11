using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

public class TitleUIToolkit : MonoBehaviour
{
    [Inject] private IPlayerDataService _playerDataService;
    [Inject] private ISceneService _sceneService;
    [Inject] private IThemeService _themeService;
    
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
        
        // Subscribe to theme changes and apply current theme
        if (_themeService != null)
        {
            _themeService.ThemeChanged += OnThemeChanged;
            ApplyTheme(_themeService.CurrentTheme);
        }
    }
    
    private void OnDisable()
    {
        // Unregister callbacks to prevent memory leaks
        _playerButton.clicked -= OnPlayerButtonClicked;
        _npcButton.clicked -= OnNpcButtonClicked;
        
        // Unsubscribe from theme changes
        if (_themeService != null)
        {
            _themeService.ThemeChanged -= OnThemeChanged;
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            _themeService?.CycleTheme();
        }
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
    
    private void OnThemeChanged(string newTheme)
    {
        ApplyTheme(newTheme);
    }
    
    private void ApplyTheme(string themeName)
    {
        var uiDocument = GetComponent<UIDocument>();
        if (!uiDocument) return;
        
        var root = uiDocument.rootVisualElement;
        if (root == null) return;
        
        // Remove all theme classes
        foreach (var theme in _themeService.AvailableThemes)
        {
            if (!string.IsNullOrEmpty(theme))
            {
                root.RemoveFromClassList(theme);
            }
        }
        
        // Apply new theme
        if (!string.IsNullOrEmpty(themeName))
            root.AddToClassList(themeName);
    }
}