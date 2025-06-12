using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using VContainer;

/// <summary>
/// Unified Game UI controller for tag game using UI Toolkit
/// Combines Game Message, Timer, It Player display, and Score Board functionality
/// </summary>
public class GameUIToolkit : MonoBehaviour
{
    // UI Elements
    private Label _gameMessageLabel;
    private Label _timerValue;
    private Label _itValue;
    private Label _scoreBoardContent;
    private VisualElement _scoreBoardContainer;
    
    // VContainer依存注入
    private IGameManagerService _gameManagerService;
    
    [Inject]
    public void Construct(IGameManagerService gameManagerService)
    {
        _gameManagerService = gameManagerService;
    }
    
    public enum MessageType
    {
        Default,
        Info,
        Warning,
        Success,
        Error
    }
    
    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        
        // Query UI elements
        _gameMessageLabel = root.Q<Label>("game-message-label");
        _timerValue = root.Q<Label>("timer-value");
        _itValue = root.Q<Label>("it-value");
        _scoreBoardContainer = root.Q<VisualElement>("score-board-container");
        _scoreBoardContent = root.Q<Label>("score-board-content");
        
        // Initially clear the message
        if (_gameMessageLabel != null)
        {
            _gameMessageLabel.text = "";
        }
        
        // Always show score board
        if (_scoreBoardContainer != null)
        {
            _scoreBoardContainer.style.display = DisplayStyle.Flex;
        }
    }
    
    private void Update()
    {
        if (_gameManagerService == null) return;
        
        UpdateTimer();
        UpdateItPlayer();
        UpdateScoreBoard();
    }
    
    // Game Message Methods
    
    public void SetMessage(string message)
    {
        SetMessage(message, MessageType.Default);
    }
    
    public void SetMessage(string message, MessageType messageType = MessageType.Default)
    {
        if (_gameMessageLabel == null) return;
        
        _gameMessageLabel.text = message;
        
        // Remove all type classes
        _gameMessageLabel.RemoveFromClassList("info");
        _gameMessageLabel.RemoveFromClassList("warning");
        _gameMessageLabel.RemoveFromClassList("success");
        _gameMessageLabel.RemoveFromClassList("error");
        
        // Add appropriate class based on message type
        switch (messageType)
        {
            case MessageType.Info:
                _gameMessageLabel.AddToClassList("info");
                break;
            case MessageType.Warning:
                _gameMessageLabel.AddToClassList("warning");
                break;
            case MessageType.Success:
                _gameMessageLabel.AddToClassList("success");
                break;
            case MessageType.Error:
                _gameMessageLabel.AddToClassList("error");
                break;
        }
        
        ShowMessage();
    }
    
    public void ShowMessage()
    {
        if (_gameMessageLabel != null && !string.IsNullOrEmpty(_gameMessageLabel.text))
        {
            _gameMessageLabel.style.display = DisplayStyle.Flex;
        }
    }
    
    public void HideMessage()
    {
        if (_gameMessageLabel != null)
        {
            _gameMessageLabel.style.display = DisplayStyle.None;
        }
    }
    
    public void ClearMessage()
    {
        if (_gameMessageLabel != null)
        {
            _gameMessageLabel.text = "";
            _gameMessageLabel.style.display = DisplayStyle.None;
        }
    }
    
    // Timer Methods
    
    private void UpdateTimer()
    {
        if (_timerValue == null || _gameManagerService == null) return;
        
        if (_gameManagerService.GameState == 1)
        {
            var elapsedTime = _gameManagerService.GetElapsedTime();
            _timerValue.text = elapsedTime.ToString("F2");
        }
        else
        {
            _timerValue.text = "0.00";
        }
    }
    
    // It Player Methods
    
    private void UpdateItPlayer()
    {
        if (_itValue == null || _gameManagerService == null) return;
        
        var itIndex = _gameManagerService.ItIndex;
        var playerNames = _gameManagerService.PlayerNames;
        
        if (itIndex >= 0 && itIndex < playerNames.Count)
        {
            _itValue.text = playerNames[itIndex];
        }
        else
        {
            _itValue.text = "---";
        }
    }
    
    // Score Board Methods
    
    private void UpdateScoreBoard()
    {
        if (_scoreBoardContent == null || _gameManagerService == null) return;
        
        var playerNames = _gameManagerService.PlayerNames;
        var playerScores = _gameManagerService.PlayerScores;
        
        if (playerNames == null || playerScores == null || playerNames.Count == 0)
        {
            _scoreBoardContent.text = "No scores available";
            return;
        }
        
        // Create sorted score list
        var scoreEntries = playerNames
            .Select((playerName, index) => new { 
                Name = playerName, 
                Score = index < playerScores.Count ? playerScores[index] : 0f 
            })
            .OrderBy(entry => entry.Score)
            .ToList();
        
        // Build score text with simple string concatenation
        var scoreText = "";
        for (int i = 0; i < scoreEntries.Count; i++)
        {
            if (i > 0) scoreText += "\n";
            scoreText += $"{i + 1}. {scoreEntries[i].Name}: {scoreEntries[i].Score:F1}";
        }
        
        _scoreBoardContent.text = scoreText;
    }
    
}