using UnityEngine;

public class PlayerNameInputField : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "");
        this.GetComponent<TMPro.TMP_InputField>().text = playerName;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
