using UnityEngine;
using TMPro;

public class GameMessageUI : MonoBehaviour
{
    private TMP_Text _messageText;
    public void SetMessage(string message)
    {
        _messageText.text = message;
    }
    private void Start()
    {
        _messageText = this.GetComponent<TMP_Text>();
    }
}
