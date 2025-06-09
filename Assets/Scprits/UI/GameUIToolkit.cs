using UnityEngine;
using UnityEngine.UIElements;
using System.Text;
using System.Linq;

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
    
    // State
    private bool _isScoreBoardEnabled = false;
    private StringBuilder _scoreStringBuilder = new StringBuilder();
    
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
    }
    
    private void Update()
    {
        if (GameManagerBase.Instance == null) return;
        
        UpdateTimer();
        UpdateItPlayer();
        
        if (_isScoreBoardEnabled)
        {
            UpdateScoreBoard();
        }
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
        if (_timerValue == null) return;
        
        var gameManager = GameManagerBase.Instance;
        
        // Check for NpcGameManager or OfflinePlayerGameManager
        if (gameManager.GetType() == typeof(NpcGameManager))
        {
            var npcManager = (NpcGameManager)gameManager;
            if (npcManager.GameState == 1)
            {
                var elapsedTime = npcManager.GetElapsedTime();
                _timerValue.text = elapsedTime.ToString("F2");
            }
            else
            {
                _timerValue.text = "0.00";
            }
        }
        else if (gameManager.GetType() == typeof(OfflinePlayerGameManager))
        {
            var offlineManager = (OfflinePlayerGameManager)gameManager;
            if (offlineManager.GameState == 1)
            {
                var elapsedTime = offlineManager.GetElapsedTime();
                _timerValue.text = elapsedTime.ToString("F2");
            }
            else
            {
                _timerValue.text = "0.00";
            }
        }
        else
        {
            // For other game managers, just show game state
            _timerValue.text = gameManager.GameState == 1 ? "Running" : "0.00";
        }
    }
    
    // It Player Methods
    
    private void UpdateItPlayer()
    {
        if (_itValue == null) return;
        
        var gameManager = GameManagerBase.Instance;
        var itIndex = gameManager.itIndex;
        
        if (itIndex >= 0 && itIndex < gameManager.playerNames.Count)
        {
            _itValue.text = gameManager.playerNames[itIndex];
        }
        else
        {
            _itValue.text = "---";
        }
    }
    
    // Score Board Methods
    
    public void EnableScoreBoard(bool enable)
    {
        _isScoreBoardEnabled = enable;
        if (_scoreBoardContainer != null)
        {
            _scoreBoardContainer.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
    
    private void UpdateScoreBoard()
    {
        if (_scoreBoardContent == null) return;
        
        var gameManager = GameManagerBase.Instance;
        
        _scoreStringBuilder.Clear();
        
        // Create sorted score list
        var scoreEntries = gameManager.playerNames
            .Select((name, index) => new { 
                Name = name, 
                Score = index < gameManager.playerScores.Count ? gameManager.playerScores[index] : 0f 
            })
            .OrderByDescending(entry => entry.Score)
            .ToList();
        
        // Build score text
        for (int i = 0; i < scoreEntries.Count; i++)
        {
            if (i > 0) _scoreStringBuilder.AppendLine();
            _scoreStringBuilder.Append($"{i + 1}. {scoreEntries[i].Name}: {scoreEntries[i].Score:F2}");
        }
        
        _scoreBoardContent.text = _scoreStringBuilder.ToString();
    }
    
}