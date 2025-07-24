using UnityEngine;
using System.Collections.Generic;

public interface IPlayerSpawnService
{
    List<GameObject> SpawnedPlayers { get; }
    
    GameObject SpawnPlayer(GameObject playerPrefab, Vector3 position, int index);
    GameObject SpawnNpc(GameObject npcPrefab, Vector3 position, int index, Transform target);
    void RemovePlayer(GameObject player);
    void ClearAllPlayers();
    Vector3 GetRandomSpawnPosition();
}