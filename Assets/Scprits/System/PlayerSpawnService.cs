using UnityEngine;
using System.Collections.Generic;
using VContainer;

public class PlayerSpawnService : IPlayerSpawnService
{
    public List<GameObject> SpawnedPlayers { get; private set; } = new ();
    
    public GameObject SpawnPlayer(GameObject playerPrefab, Vector3 position, int index)
    {
        if (!playerPrefab) return null;
        
        var player = Object.Instantiate(playerPrefab, position, Quaternion.identity);
        var playerBase = player.GetComponent<PlayerBase>();
        if (playerBase) playerBase.index = index;
        
        SpawnedPlayers.Add(player);
        return player;
    }
    
    public GameObject SpawnNpc(GameObject npcPrefab, Vector3 position, int index, Transform target)
    {
        if (!npcPrefab) return null;
        
        var npc = Object.Instantiate(npcPrefab, position, Quaternion.identity);
        var npcComponent = npc.GetComponent<Npc>();
        if (npcComponent)
        {
            npcComponent.index = index;
            npcComponent.SetTarget(target);
        }
        
        SpawnedPlayers.Add(npc);
        return npc;
    }
    
    public void RemovePlayer(GameObject player)
    {
        if (player && SpawnedPlayers.Contains(player))
        {
            SpawnedPlayers.Remove(player);
            Object.Destroy(player);
        }
    }
    
    public void ClearAllPlayers()
    {
        foreach (var player in SpawnedPlayers.ToArray())
        {
            RemovePlayer(player);
        }
    }
    
    public Vector3 GetRandomSpawnPosition()
    {
        return new Vector3(
            Random.Range(-3f, 3f), 
            1f, 
            Random.Range(-3f, 3f)
        );
    }
}