using UnityEngine;

public class PlayerNameInputField : MonoBehaviour
{
    void Start()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "");
        this.GetComponent<TMPro.TMP_InputField>().text = playerName;
    }

    void Update()
    {

    }
}
