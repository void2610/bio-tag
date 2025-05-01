using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField playerNameInputField;

    private void Start()
    {
        var playerName = PlayerPrefs.GetString("PlayerName", "");
        playerNameInputField.text = playerName;
    }

    public void OnPlayerButtonClicked()
    {
        var playerName = playerNameInputField.text;
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
        SceneManager.LoadScene("WithPlayer");
    }

    public void OnNpcButtonClicked()
    {
        var playerName = playerNameInputField.text;
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
        SceneManager.LoadScene("WithNPC");
    }
}
