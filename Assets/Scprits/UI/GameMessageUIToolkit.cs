using UnityEngine;
using UnityEngine.UIElements;

public class GameMessageUIToolkit : MonoBehaviour
{
    private Label _messageLabel;
    private VisualElement _container;
    
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
        
        _container = root.Q<VisualElement>("game-message-container");
        _messageLabel = root.Q<Label>("game-message-label");
        
        // Initially hide the message
        HideMessage();
    }
    
    public void SetMessage(string message)
    {
        SetMessage(message, MessageType.Default);
    }
    
    public void SetMessage(string message, MessageType messageType = MessageType.Default)
    {
        if (_messageLabel == null) return;
        
        _messageLabel.text = message;
        
        // Remove all type classes
        _messageLabel.RemoveFromClassList("info");
        _messageLabel.RemoveFromClassList("warning");
        _messageLabel.RemoveFromClassList("success");
        _messageLabel.RemoveFromClassList("error");
        
        // Add appropriate class based on message type
        switch (messageType)
        {
            case MessageType.Info:
                _messageLabel.AddToClassList("info");
                break;
            case MessageType.Warning:
                _messageLabel.AddToClassList("warning");
                break;
            case MessageType.Success:
                _messageLabel.AddToClassList("success");
                break;
            case MessageType.Error:
                _messageLabel.AddToClassList("error");
                break;
        }
        
        ShowMessage();
    }
    
    public void ShowMessage()
    {
        if (_container != null)
        {
            _container.style.display = DisplayStyle.Flex;
        }
    }
    
    public void HideMessage()
    {
        if (_container != null)
        {
            _container.style.display = DisplayStyle.None;
        }
    }
    
    public void ClearMessage()
    {
        if (_messageLabel != null)
        {
            _messageLabel.text = "";
        }
        HideMessage();
    }
}