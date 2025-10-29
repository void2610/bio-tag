using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;
using System.Collections.Generic;
using DG.Tweening;
using VContainer;
using Cysharp.Threading.Tasks;
using System.Threading;
using R3;
using System;
using VitalRouter;
using BioTag.Audio;

public class Npc : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;

    private readonly List<Transform> _fleeAnchors = new ();
    private Animator Animator => GetComponent<Animator>();
    private NavMeshAgent Agent => this.GetComponent<NavMeshAgent>();
    private int _index = -1;
    private bool _isMovable = true;
    private VisualEffect _itEffect;
    private Transform _target;
    private bool _isJumping = false;
    private Transform _currentFleeTarget = null;
    private IGameManagerService _gameManager;
    private float _onLandTime = 0f;
    
    private const float FLEE_RECALCULATION_INTERVAL = 0.25f;
    private readonly ReactiveProperty<bool> _shouldRecalculateFlee = new(true);
    private CancellationTokenSource _fleeRecalculationCts;
    private Vector3 _lastPlayerPosition;

    private async UniTaskVoid Wait(float time)
    {
        _isMovable = false;
        await UniTask.Delay((int)(time * 1000), cancellationToken: this.GetCancellationTokenOnDestroy());
        _isMovable = true;
    }

    public void Initialize(IGameManagerService gameManager, int index, Transform target, Transform fleeParent)
    {
        _gameManager = gameManager;
        _index = index;
        _target = target;

        foreach (var f in fleeParent.GetComponentsInChildren<Transform>())
            _fleeAnchors.Add(f);
        
        // NavMeshAgent設定
        Agent.speed = moveSpeed;
        Agent.autoTraverseOffMeshLink = false; // 手動でオフメッシュリンクを処理

        // VFXをセンサーマネージャーに登録
        var vfx = transform.Find("PlayerTrail").GetComponent<VisualEffect>();
        SensorManager.Instance.AddVFX(vfx);
        _itEffect = transform.Find("ItEffect").GetComponent<VisualEffect>();
        
        // 逃げる再計算タイマーの開始
        StartFleeRecalculationTimer().Forget();
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
            ChangeSpeedOnLinkAsync().Forget();
        }

        _itEffect?.SetInt("Rate", _index == currentItIndex ? 30 : 0);
    }

    private async UniTaskVoid ChangeSpeedOnLinkAsync()
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

        var middle = new Vector3((startPos.x + endPos.x) / 2, Mathf.Max(startPos.y, endPos.y) + 0.5f, (startPos.z + endPos.z) / 2);
        this.transform.DOPath(new Vector3[] { startPos, middle, endPos }, duration).SetEase(Ease.Linear);

        await UniTask.Delay((int)(duration * 1000), cancellationToken: this.GetCancellationTokenOnDestroy());
        
        Agent.CompleteOffMeshLink();

        Agent.Warp(endPos);
        Animator.SetBool("Jump", false);
        Animator.SetBool("Grounded", true);

        await UniTask.Delay(300, cancellationToken: this.GetCancellationTokenOnDestroy());

        Agent.speed = moveSpeed;
        Agent.updatePosition = true;
        _isJumping = false;
    }

    private async UniTaskVoid StartFleeRecalculationTimer()
    {
        _fleeRecalculationCts = new CancellationTokenSource();
        var token = CancellationTokenSource.CreateLinkedTokenSource(
            _fleeRecalculationCts.Token,
            this.GetCancellationTokenOnDestroy()
        ).Token;
        
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(FLEE_RECALCULATION_INTERVAL), cancellationToken: token);
            _shouldRecalculateFlee.Value = true;
        }
    }
    
    private void Flee()
    {
        if (!_target) return;
        if (_fleeAnchors.Count == 0) return;
        
        // 一定時間ごとに逃げる目標を再計算、またはまだ計算していない場合
        var shouldRecalculate = !_currentFleeTarget || 
                                _shouldRecalculateFlee.Value ||
                                (Agent.hasPath && Agent.remainingDistance < 1f);
        
        if (shouldRecalculate)
        {
            var myPosition = transform.position;
            var playerPosition = _target.position;
            
            // Step 1: プレイヤーからNPCへのベクトル（逃げる方向）
            var fleeDirection = (myPosition - playerPosition).normalized;
            
            Transform bestFleeTarget = null;
            var maxSafetyScore = float.MinValue;
            
            foreach (var anchor in _fleeAnchors)
            {
                if (!anchor) continue;
                
                // Step 2: NavMeshパスの確認
                var path = new NavMeshPath();
                if (!Agent.CalculatePath(anchor.position, path) || path.status != NavMeshPathStatus.PathComplete)
                    continue;
                
                // Step 3: アンカーの位置がプレイヤーの反対方向にあるかチェック
                var toAnchorDirection = (anchor.position - myPosition).normalized;
                var directionAlignment = Vector3.Dot(fleeDirection, toAnchorDirection);
                
                // Step 4: プレイヤーに向かう方向は完全に排除
                if (directionAlignment < 0.1f) // 逃げる方向とほぼ同じか、少しでも逆だったら除外
                    continue;
                
                // Step 5: 安全性スコア計算
                var distanceFromPlayer = Vector3.Distance(anchor.position, playerPosition);
                var distanceFromMe = Vector3.Distance(anchor.position, myPosition);
                
                // シンプルなスコア: 方向性 + プレイヤーからの距離
                var safetyScore = (directionAlignment * 10f) + (distanceFromPlayer * 0.1f);
                
                // 遠すぎる場所は減点（効率的な逃走のため）
                if (distanceFromMe > 30f)
                    safetyScore *= 0.5f;
                
                if (safetyScore > maxSafetyScore)
                {
                    maxSafetyScore = safetyScore;
                    bestFleeTarget = anchor;
                }
            }

            if (bestFleeTarget)
            {
                _currentFleeTarget = bestFleeTarget;
                _shouldRecalculateFlee.Value = false;
                Agent.speed = moveSpeed;
                Agent.SetDestination(bestFleeTarget.position);
            }
            else
            {
                // フォールバック: プレイヤーの反対方向に直接移動
                var fallbackTarget = myPosition + fleeDirection * 15f;
                Agent.SetDestination(fallbackTarget);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || _index == _gameManager?.ItIndex) return;
        
        _gameManager?.ChangeIt(_index);
        Wait(1f).Forget();
    }
    
    private void OnDestroy()
    {
        _fleeRecalculationCts?.Cancel();
        _fleeRecalculationCts?.Dispose();
        _shouldRecalculateFlee?.Dispose();
    }
    
    protected void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            var position = transform.position;
            Router.Default.PublishAsync(new PlayFootstepCommand(position));
        }
    }

    protected void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.2f && Time.time - _onLandTime > 0.1f)
        {
            _onLandTime = Time.time;
            var position = transform.position;
            Router.Default.PublishAsync(new PlayLandingCommand(position));
        }
    }
}
