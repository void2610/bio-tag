using UnityEngine;

public class NPC : MonoBehaviour
{
    public int index = -1;
    void Start()
    {

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
