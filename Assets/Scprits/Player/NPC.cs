using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField]
    protected GameObject playerNameUIPrefab;
    public int index = -1;
    void Start()
    {
        var canvas = GameObject.Find("WorldSpaceCanvas");
        var ui = Instantiate(playerNameUIPrefab, canvas.transform);
        ui.GetComponent<PlayerNameUI>().SetTargetPlayer(this.gameObject, "NPC" + index);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(index);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NPCGameManager.instance.ChangeIt(index);
        }
    }
}
