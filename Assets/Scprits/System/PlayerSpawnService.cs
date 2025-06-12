using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using VContainer;

public class PlayerSpawnService : IPlayerSpawnService
{
    public List<GameObject> SpawnedPlayers { get; private set; } = new ();
    
    private readonly IObjectResolver _container;
    private readonly IPlayerDataService _playerDataService;
    private readonly PlayerNameUI _playerNameUIPrefab;
    private readonly GameConfig _gameConfig;
    
    [Inject]
    public PlayerSpawnService(IObjectResolver container, IPlayerDataService playerDataService, GameConfig gameConfig, PlayerNameUI playerNameUIPrefab)
    {
        _container = container;
        _playerDataService = playerDataService;
        _gameConfig = gameConfig;
        _playerNameUIPrefab = playerNameUIPrefab;
    }
    
    public GameObject SpawnPlayer(GameObject playerPrefab, Vector3 position, int index)
    {
        if (!playerPrefab) return null;
        
        var player = Object.Instantiate(playerPrefab, position, Quaternion.identity);
        var playerBase = player.GetComponent<PlayerBase>();
        var nameUI = Object.Instantiate(_playerNameUIPrefab, GameObject.Find("WorldSpaceCanvas").transform);
        if (playerBase)
        {
            // VContainerで依存注入を実行
            _container.Inject(playerBase);
            
            // PlayerNameUIをInitializeで初期化（VContainer依存注入は不要）
            var playerNameUI = nameUI.GetComponent<PlayerNameUI>();
            var name = _playerDataService.GetPlayerName();
            playerNameUI.Initialize(player.transform, name);
            
            playerBase.index = index;
        }
        
        SpawnedPlayers.Add(player);
        return player;
    }
    
    public GameObject SpawnNpc(GameObject npcPrefab, Vector3 position, int index, Transform target)
    {
        if (!npcPrefab) return null;
        
        var npc = Object.Instantiate(npcPrefab, position, Quaternion.identity);
        var npcComponent = npc.GetComponent<Npc>();
        var nameUI = Object.Instantiate(_playerNameUIPrefab, GameObject.Find("WorldSpaceCanvas").transform);
        if (npcComponent)
        {
            // VContainerで依存注入を実行
            _container.Inject(npcComponent);
            npcComponent.Initialize(index, target, _gameConfig.fleeParent);
            var playerNameUI = nameUI.GetComponent<PlayerNameUI>();
            playerNameUI.Initialize(npc.transform, $"NPC{index}");
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