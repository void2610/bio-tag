using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using VContainer;

public class Npc : MonoBehaviour
{
    [SerializeField] private GameObject playerNameUIPrefab;
    [SerializeField] private float moveSpeed = 8f;
    
    private readonly List<Transform> _fleeAnchors = new List<Transform>();
    private Animator Animator => GetComponent<Animator>();
    private CharacterController CCon => GetComponent<CharacterController>();
    private NavMeshAgent Agent => this.GetComponent<NavMeshAgent>();
    private int _index = -1;
    private bool _isMovable = true;
    private VisualEffect _itEffect;
    private Transform _target;
    private bool _isJumping = false;
    private Transform _currentFleeTarget = null;
    private float _lastFleeCalculationTime = 0f;
    private IGameManagerService _gameManager;
    
    private const float FLEE_RECALCULATION_INTERVAL = 2f; // 2秒ごとに再計算
    
    [Inject]
    public void Construct(IGameManagerService gameManager, Transform fleeParent)
    {
        _gameManager = gameManager;
        foreach (var f in fleeParent.GetComponentsInChildren<Transform>())
        {
            _fleeAnchors.Add(f);
        }
    }

    private void Wait(float time)
    {
        StartCoroutine(WaitCoroutine(time));
    }

    public void Initialize(int index, string playerName, Transform target)
    {
        _index = index;
        _target = target;
        
        // UI設定
        var canvas = GameObject.Find("WorldSpaceCanvas");
        var ui = Instantiate(playerNameUIPrefab, canvas.transform);
        ui.GetComponent<PlayerNameUI>().SetTargetPlayer(this.gameObject, _index);

        // NavMeshAgent設定
        Agent.speed = moveSpeed;
        Agent.autoTraverseOffMeshLink = false; // 手動でオフメッシュリンクを処理

        // VFXをセンサーマネージャーに登録
        var vfx = transform.Find("PlayerTrail").GetComponent<VisualEffect>();
        SensorManager.Instance.AddVFX(vfx);
        _itEffect = transform.Find("ItEffect").GetComponent<VisualEffect>();
        
        // プレイヤー名を登録
        _gameManager?.AddPlayerName(playerName);
    }

    private void Update()
    {
        // VContainerからゲーム状態を取得
        var isGamePlaying = _gameManager?.GameState == 1;
        var currentItIndex = _gameManager?.ItIndex ?? -1;
        
        Agent.isStopped = !_isMovable;
        if (_target && isGamePlaying && _isMovable)
        {
            if (_index != currentItIndex)
            {
                Flee();
            }
            else
            {
                // 鬼になったときは逃げ先をリセット
                _currentFleeTarget = null;
                Agent.SetDestination(_target.position);
            }
            
            Animator.SetFloat("Speed", Agent.velocity.magnitude);
            Animator.SetFloat("MotionSpeed", 1);
        }
        else
        {
            Animator.SetFloat("Speed", 0);
            Animator.SetFloat("MotionSpeed", 0);
        }

        if (Agent.isOnOffMeshLink && !_isJumping)
        {
            StartCoroutine(ChangeSpeedOnLink());
        }

        _itEffect?.SetInt("Rate", _index == currentItIndex ? 30 : 0);
    }

    private IEnumerator WaitCoroutine(float time)
    {
        _isMovable = false;
        yield return new WaitForSeconds(time);
        _isMovable = true;
    }

    private IEnumerator ChangeSpeedOnLink()
    {
        _isJumping = true;
        Animator.SetBool("Jump", true);
        Animator.SetBool("Grounded", false);

        Agent.updatePosition = false;
        // プレイヤーの方向を向く
        transform.LookAt(_target);
        Agent.speed = 0;

        var startPos = Agent.transform.position;
        var endPos = Agent.currentOffMeshLinkData.endPos + Vector3.up * Agent.baseOffset;

        var duration = Vector3.Distance(Agent.transform.position, endPos) / (moveSpeed * 0.8f);

        float time = 0;

        var middle = new Vector3((startPos.x + endPos.x) / 2, Mathf.Max(startPos.y, endPos.y) + 0.5f, (startPos.z + endPos.z) / 2);
        this.transform.DOPath(new Vector3[] { startPos, middle, endPos }, duration).SetEase(Ease.Linear);

        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
        }
        Agent.CompleteOffMeshLink();

        Agent.Warp(endPos);
        Animator.SetBool("Jump", false);
        Animator.SetBool("Grounded", true);

        yield return new WaitForSeconds(0.3f);

        Agent.speed = moveSpeed;
        Agent.updatePosition = true;
        _isJumping = false;
    }

    private void Flee()
    {
        if (!_target) return;
        if (_fleeAnchors.Count == 0)
        {
            Debug.LogWarning("No flee anchors available.");
            return;
        }
        
        // 一定時間ごとに逃げる目標を再計算、またはまだ計算していない場合
        bool shouldRecalculate = _currentFleeTarget == null || 
                                (Time.time - _lastFleeCalculationTime > FLEE_RECALCULATION_INTERVAL) ||
                                (Agent.hasPath && Agent.remainingDistance < 1f);
        
        if (shouldRecalculate)
        {
            // アンカーポイントの中から、最もプレイヤーから遠いものを選択
            Transform farthestAnchor = null;
            var maxDistance = float.MinValue;

            foreach (var anchor in _fleeAnchors)
            {
                if (!anchor) continue;
                
                var distanceToTarget = Vector3.Distance(anchor.position, _target.position);
                if (distanceToTarget > maxDistance)
                {
                    // NavMesh上で有効な経路かどうかを確認
                    var path = new NavMeshPath();
                    Agent.CalculatePath(anchor.position, path);

                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        maxDistance = distanceToTarget;
                        farthestAnchor = anchor;
                    }
                }
            }

            if (farthestAnchor != null)
            {
                _currentFleeTarget = farthestAnchor;
                _lastFleeCalculationTime = Time.time;
                Agent.SetDestination(farthestAnchor.position);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || _index == _gameManager?.ItIndex) return;
        
        _gameManager?.ChangeIt(_index);
        this.Wait(1f);
    }

}
