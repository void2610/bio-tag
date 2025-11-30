using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;
using System.Collections.Generic;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using VitalRouter;
using BioTag.Audio;
using BioTag.GameUI;

public class Npc : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;

    private Animator Animator => GetComponent<Animator>();
    private NavMeshAgent Agent => this.GetComponent<NavMeshAgent>();
    private int _index = -1;
    private bool _isMovable = true;
    private VisualEffect _itEffect;
    private Transform _target;
    private bool _isJumping;
    private Vector3? _currentFleeTarget; // 動的サンプリングではVector3を使用
    private IGameManagerService _gameManager;
    private float _onLandTime;

    // 逃走ロジック用
    private const float FLEE_RECALCULATION_INTERVAL = 0.25f;
    private bool _shouldRecalculateFlee = true;
    private CancellationTokenSource _fleeRecalculationCts;

    // スタック検出用
    private float _stuckCheckTimer;
    private Vector3 _lastPosition;
    private const float STUCK_THRESHOLD = 0.1f;
    private const float STUCK_CHECK_INTERVAL = 0.5f;

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

        // NavMeshAgent設定
        Agent.speed = moveSpeed;
        Agent.acceleration = 50f; // 高速加速（0.16秒で最大速度到達）
        Agent.autoBraking = false; // 目標地点手前で減速しない
        Agent.autoTraverseOffMeshLink = false; // 手動でオフメッシュリンクを処理

        // VFXをセンサーマネージャーに登録
        var vfx = transform.Find("PlayerTrail").GetComponent<VisualEffect>();
        SensorManager.Instance.AddVFX(vfx);
        _itEffect = transform.Find("ItEffect").GetComponent<VisualEffect>();

        // スタック検出の初期化
        _lastPosition = transform.position;

        // 逃げる再計算タイマーの開始
        StartFleeRecalculationTimer().Forget();
    }

    private void Update()
    {
        // VContainerからゲーム状態を取得
        var isGamePlaying = _gameManager?.GameState == 1;
        var currentItIndex = _gameManager?.CurrentItIndex ?? -1;

        Agent.isStopped = !_isMovable;
        if (_target && isGamePlaying && _isMovable)
        {
            if (_index != currentItIndex)
            {
                // スタック検出
                CheckIfStuck();
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
            _shouldRecalculateFlee = true;
        }
    }

    private void CheckIfStuck()
    {
        _stuckCheckTimer += Time.deltaTime;

        if (_stuckCheckTimer >= STUCK_CHECK_INTERVAL)
        {
            var distanceMoved = Vector3.Distance(transform.position, _lastPosition);

            // 移動距離が小さく、速度も低い場合はスタックと判定
            if (distanceMoved < STUCK_THRESHOLD && Agent.velocity.magnitude < 0.5f && Agent.hasPath)
            {
                // スタック検出！強制的に再計算
                _shouldRecalculateFlee = true;
                _currentFleeTarget = null;
            }

            _lastPosition = transform.position;
            _stuckCheckTimer = 0f;
        }
    }

    private void Flee()
    {
        if (!_target) return;

        var myPosition = transform.position;
        var playerPosition = _target.position;
        var distanceToPlayer = Vector3.Distance(myPosition, playerPosition);

        // 現在の速度を保存
        var currentSpeed = Agent.velocity.magnitude;
        var isMovingFast = currentSpeed > moveSpeed * 0.7f;

        // 再計算条件: 目標がない、目標に到達、プレイヤーが急接近、定期再計算
        var shouldRecalculate = !_currentFleeTarget.HasValue ||
                                (Agent.hasPath && Agent.remainingDistance < 2f) ||
                                (distanceToPlayer < 5f) ||
                                _shouldRecalculateFlee;

        if (shouldRecalculate)
        {
            // プレイヤーから逃げる基本方向
            var fleeDirection = (myPosition - playerPosition).normalized;

            // 動的NavMeshサンプリングで候補点を探索
            Vector3? bestTarget = FindBestFleeTarget(myPosition, playerPosition, fleeDirection);

            if (bestTarget.HasValue)
            {
                _currentFleeTarget = bestTarget;
                Agent.SetDestination(bestTarget.Value);
                _shouldRecalculateFlee = false;

                // 速度復元: 十分な速度で移動中の場合
                if (isMovingFast)
                {
                    RestoreVelocityAsync(currentSpeed).Forget();
                }
            }
            else
            {
                // 緊急回避: 有効な候補が見つからない場合
                Vector3? emergencyTarget = FindEmergencyEscapeTarget(myPosition, playerPosition, fleeDirection);
                if (emergencyTarget.HasValue)
                {
                    _currentFleeTarget = emergencyTarget;
                    Agent.SetDestination(emergencyTarget.Value);

                    // 速度復元
                    if (isMovingFast)
                    {
                        RestoreVelocityAsync(currentSpeed).Forget();
                    }
                }
            }
        }
        // パス喪失時の回復処理
        else if (_currentFleeTarget.HasValue && !Agent.hasPath)
        {
            // パスが失われた場合、現在の目標を再設定
            Agent.SetDestination(_currentFleeTarget.Value);
        }
    }

    private Vector3? FindBestFleeTarget(Vector3 myPosition, Vector3 playerPosition, Vector3 fleeDirection)
    {
        Vector3? bestTarget = null;
        float maxScore = float.MinValue;

        // 扇状検索: -60度～+60度を30度刻みでサンプリング（9方向 → 5方向）
        for (int angle = -60; angle <= 60; angle += 30)
        {
            var rotatedDirection = Quaternion.Euler(0, angle, 0) * fleeDirection;

            // 複数の距離でサンプリング（10m、15m）（3距離 → 2距離）
            float[] distances = { 10f, 15f };

            foreach (var distance in distances)
            {
                var candidatePos = myPosition + rotatedDirection * distance;

                // NavMesh上の有効な点を検索
                if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    // パス検証
                    var path = new NavMeshPath();
                    if (Agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        // スコア計算
                        float score = CalculateFleeScore(myPosition, playerPosition, hit.position, rotatedDirection, path);

                        if (score > maxScore)
                        {
                            maxScore = score;
                            bestTarget = hit.position;

                            // 早期終了: 十分良いスコアが見つかったら即座に返す
                            if (score > 10f)
                            {
                                return bestTarget;
                            }
                        }
                    }
                }
            }
        }

        return bestTarget;
    }

    private float CalculateFleeScore(Vector3 myPosition, Vector3 playerPosition, Vector3 candidatePosition, Vector3 fleeDirection, NavMeshPath path)
    {
        // 方向性評価: 逃げる方向との一致度
        var toCandidate = (candidatePosition - myPosition).normalized;
        var directionAlignment = Vector3.Dot(fleeDirection, toCandidate);

        // 距離評価
        var distFromPlayer = Vector3.Distance(candidatePosition, playerPosition);
        var distFromMe = Vector3.Distance(candidatePosition, myPosition);

        // パスの安全性評価: 経路とプレイヤーの最短距離
        float minDistToPlayerOnPath = float.MaxValue;
        foreach (var corner in path.corners)
        {
            var d = Vector3.Distance(corner, playerPosition);
            if (d < minDistToPlayerOnPath)
                minDistToPlayerOnPath = d;
        }

        // スコア計算: 方向性 + プレイヤーからの距離 + パス安全性 - 効率性
        var score = (directionAlignment * 5f) +
                    (distFromPlayer * 0.3f) +
                    (minDistToPlayerOnPath * 0.2f) -
                    (distFromMe * 0.1f);

        return score;
    }

    private Vector3? FindEmergencyEscapeTarget(Vector3 myPosition, Vector3 playerPosition, Vector3 fleeDirection)
    {
        // 緊急回避1: 壁沿い（逃げる方向の垂直方向）
        Vector3[] perpendicularDirections = {
            Vector3.Cross(fleeDirection, Vector3.up).normalized,
            Vector3.Cross(fleeDirection, Vector3.down).normalized
        };

        foreach (var direction in perpendicularDirections)
        {
            var candidatePos = myPosition + direction * 10f;

            if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                var path = new NavMeshPath();
                if (Agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    return hit.position;
                }
            }
        }

        // 緊急回避2: ランダム方向
        for (int i = 0; i < 8; i++)
        {
            var randomAngle = UnityEngine.Random.Range(0f, 360f);
            var randomDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
            var candidatePos = myPosition + randomDirection * 8f;

            if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, 8f, NavMesh.AllAreas))
            {
                var path = new NavMeshPath();
                if (Agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    return hit.position;
                }
            }
        }

        return null;
    }

    private async UniTaskVoid RestoreVelocityAsync(float targetSpeed)
    {
        // 1フレーム待つ（NavMeshAgentの初期計算を待つ）
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken: this.GetCancellationTokenOnDestroy());

        // desiredVelocityの方向を使って速度を設定
        if (Agent.desiredVelocity.magnitude > 0.1f)
        {
            var direction = Agent.desiredVelocity.normalized;
            Agent.velocity = direction * targetSpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var currentItIndex = _gameManager?.CurrentItIndex;

        // 自分が鬼の場合：相手（プレイヤー）を新しい鬼にする
        if (_index == currentItIndex)
        {
            var targetPlayer = other.GetComponent<PlayerBase>();
            if (targetPlayer != null)
            {
                Router.Default.PublishAsync(new PlayerTaggedCommand(targetPlayer.Index, other.transform));
                Wait(1f).Forget();
            }
        }
        // 自分が鬼でない場合：鬼にタッチされたので自分が新しい鬼になる
        else
        {
            Router.Default.PublishAsync(new PlayerTaggedCommand(_index, transform));
            Wait(1f).Forget();
        }
    }
    
    private void OnDestroy()
    {
        _fleeRecalculationCts?.Cancel();
        _fleeRecalculationCts?.Dispose();
    }
    
    protected void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            var position = transform.position;
            Router.Default.PublishAsync(new PlayFootstepCommand(position, FootstepType.Step));
        }
    }

    protected void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.2f && Time.time - _onLandTime > 0.1f)
        {
            _onLandTime = Time.time;
            var position = transform.position;
            Router.Default.PublishAsync(new PlayFootstepCommand(position, FootstepType.Landing));
        }
    }
}
