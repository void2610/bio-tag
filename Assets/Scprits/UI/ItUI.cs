using UnityEngine;
using TMPro;

public class ItUI : MonoBehaviour
{
    private TMP_Text _itText;
    private void Start()
    {
        _itText = this.GetComponent<TMP_Text>();
        _itText.text = "";
    }

    private void Update()
    {
        if (GameManagerBase.Instance == null) return;
        
        var itIndex = GameManagerBase.Instance.itIndex;
        if (itIndex >= 0 && itIndex < GameManagerBase.Instance.playerNames.Count)
        {
            _itText.text = $"It: {GameManagerBase.Instance.playerNames[itIndex]}";
        }
    }
}
