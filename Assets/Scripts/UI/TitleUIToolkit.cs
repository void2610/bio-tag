using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

[RequireComponent(typeof(UIDocument))]
public class TitleUIToolkit : MonoBehaviour
{
    [Inject] private IPlayerDataService _playerDataService;
    [Inject] private ISceneService _sceneService;
    [Inject] private IThemeService _themeService;
    
    private TextField _playerNameInput;
    private Button _playerButton;
    private Button _npcButton;
    private UIDocument _uiDocument;
    
    private void OnEnable()
    {
        _uiDocument = this.GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;
        
        // UI要素を取得
        _playerNameInput = root.Q<TextField>("player-name-input");
        _playerButton = root.Q<Button>("player-button");
        _npcButton = root.Q<Button>("npc-button");
        
        // プレイヤー名をロード
        _playerNameInput.value = _playerDataService.GetPlayerName();
        
        // コールバックを登録
        _playerButton.clicked += OnPlayerButtonClicked;
        _npcButton.clicked += OnNpcButtonClicked;
        
        // テーマ変更のためのコールバックを登録
        _themeService.ThemeChanged += ApplyTheme;
        ApplyTheme(_themeService.CurrentTheme);
    }
    
    private void OnDisable()
    {
        // コールバックを解除
        _playerButton.clicked -= OnPlayerButtonClicked;
        _npcButton.clicked -= OnNpcButtonClicked;
        _themeService.ThemeChanged -= ApplyTheme;
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
    
    private void SavePlayerName() => _playerDataService.SetPlayerName(_playerNameInput.value);
    
    private void ApplyTheme(string themeName)
    {
        var root = _uiDocument.rootVisualElement;
        if (root == null) return;
        
        // 既存のテーマクラスを削除
        foreach (var theme in _themeService.AvailableThemes)
        {
            if (!string.IsNullOrEmpty(theme))
            {
                root.RemoveFromClassList(theme);
            }
        }
        
        if (!string.IsNullOrEmpty(themeName)) root.AddToClassList(themeName);
    }
}