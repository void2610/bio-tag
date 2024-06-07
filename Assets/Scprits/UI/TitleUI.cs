using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField playerNameInputField;

    void Start()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "");
        playerNameInputField.text = playerName;
    }

    public void OnStartButtonClicked()
    {
        string playerName = playerNameInputField.text;
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
        SceneManager.LoadScene("PUN2Test");
    }
}
