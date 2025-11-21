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
    private readonly IGameManagerService _gameManager;
    private readonly GameConfig _gameConfig;
    private Transform _mainPlayerCamera; // メインプレイヤーのカメラへの参照
    
    [Inject]
    public PlayerSpawnService(IObjectResolver container, IGameManagerService gameManager, IPlayerDataService playerDataService, GameConfig gameConfig, PlayerNameUI playerNameUIPrefab)
    {
        _container = container;
        _gameManager = gameManager;
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

        var playerNameUI = nameUI.GetComponent<PlayerNameUI>();
        var name = index == 0 ? _playerDataService.GetPlayerName() : $"Player {index}";

        // プレイヤーのカメラを取得
        var playerCamera = player.GetComponentInChildren<PlayerCamera>();
        Transform cameraTransform = playerCamera != null ? playerCamera.transform : null;

        // メインプレイヤー（index == 0）のカメラを保存
        if (index == 0 && cameraTransform != null)
        {
            _mainPlayerCamera = cameraTransform;
        }
        else
        {
            // サブプレイヤーの場合は、メインプレイヤーのカメラを使用
            cameraTransform = _mainPlayerCamera;

            // メインプレイヤーのカメラが保存されていない場合、SpawnedPlayersから取得
            if (cameraTransform == null && SpawnedPlayers.Count > 0)
            {
                var mainPlayer = SpawnedPlayers[0];
                if (mainPlayer != null)
                {
                    var mainCamera = mainPlayer.GetComponentInChildren<PlayerCamera>();
                    cameraTransform = mainCamera != null ? mainCamera.transform : null;
                }
            }
        }

        playerNameUI.Initialize(player.transform, cameraTransform, name);
        playerBase.Initialize(_gameManager, index);
        
        SpawnedPlayers.Add(player);
        _gameManager.AddPlayerName(name);
        
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
            npcComponent.Initialize(_gameManager, index, target, _gameConfig.fleeParent);

            var playerNameUI = nameUI.GetComponent<PlayerNameUI>();

            // メインプレイヤーのカメラを取得（NPCの名前UIは常にメインプレイヤーのカメラを向く）
            Transform cameraTransform = _mainPlayerCamera;

            // メインプレイヤーのカメラが保存されていない場合、SpawnedPlayersから取得
            if (cameraTransform == null && SpawnedPlayers.Count > 0)
            {
                var mainPlayer = SpawnedPlayers[0];
                if (mainPlayer != null)
                {
                    var mainCamera = mainPlayer.GetComponentInChildren<PlayerCamera>();
                    cameraTransform = mainCamera != null ? mainCamera.transform : null;
                }
            }

            playerNameUI.Initialize(npc.transform, cameraTransform, $"NPC{index}");
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