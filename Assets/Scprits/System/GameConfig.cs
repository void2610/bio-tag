using UnityEngine;

[System.Serializable]
public class GameConfig
{
    public GameObject playerPrefab;
    public GameObject npcPrefab;
    public Transform fleeParent;
    public float gameLength;
    public int npcCount;
}