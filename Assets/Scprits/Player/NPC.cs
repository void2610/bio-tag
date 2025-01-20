using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Npc : MonoBehaviour
{
    [SerializeField] private GameObject playerNameUIPrefab;
    [SerializeField] private float moveSpeed = 8f;
    public int index = -1;
    
    private readonly List<Transform> _fleeAnchors = new List<Transform>();
    private Animator Animator => GetComponent<Animator>();
    private CharacterController CCon => GetComponent<CharacterController>();
    private NavMeshAgent Agent => this.GetComponent<NavMeshAgent>();
    private bool _isMovable = true;
    private VisualEffect _itEffect;
    private Transform _target;
    private bool _isJumping = false;

    private void Wait(float time)
    {
        StartCoroutine(WaitCoroutine(time));
    }

    public void SetTarget(Transform target)
    {
        this._target = target;
    }

    private void Awake()
    {
        var anchors = GameObject.Find("NPCFleeAnchors");
        for (var i = 0; i < anchors.transform.childCount; i++)
        {
            _fleeAnchors.Add(anchors.transform.GetChild(i));
        }
    }

    private void Start()
    {
        var canvas = GameObject.Find("WorldSpaceCanvas");
        var ui = Instantiate(playerNameUIPrefab, canvas.transform);
        ui.GetComponent<PlayerNameUI>().SetTargetPlayer(this.gameObject, "NPC" + index);

        Agent.speed = moveSpeed;

        // VFXをセンサーマネージャーに登録
        var vfx = transform.Find("PlayerTrail").GetComponent<VisualEffect>();
        SensorManager.Instance.AddVFX(vfx);
        _itEffect = transform.Find("ItEffect").GetComponent<VisualEffect>();
    }

    private void Update()
    {
        Agent.isStopped = !_isMovable;
        if (_target && GameManagerBase.Instance.GameState == 1 && _isMovable)
        {
            if (index != GameManagerBase.Instance.itIndex)
            {
                Flee();
            }
            else
            {
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

        _itEffect.SetInt("Rate", index == GameManagerBase.Instance.itIndex ? 30 : 0);
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
        // アンカーポイントの中から、最もプレイヤーから遠いものを選択
        Transform farthestAnchor = null;
        var maxDistance = float.MinValue;

        foreach (var anchor in _fleeAnchors)
        {
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

        if (farthestAnchor)
        {
            // 計算されたアンカーポイントに向けてエージェントを移動させる
            var path = new NavMeshPath();
            Agent.CalculatePath(farthestAnchor.position, path);

            if (path.status == NavMeshPathStatus.PathComplete)
            {
                Agent.SetDestination(farthestAnchor.position);
            }

            // オフメッシュリンクに到達したらリンクを使う処理
            if (Agent.isOnOffMeshLink && !_isJumping)
            {
                StartCoroutine(ChangeSpeedOnLink());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || index == GameManagerBase.Instance.itIndex) return;
        
        GameManagerBase.Instance.ChangeIt(index);
        this.Wait(1f);
    }

    private void OnFootstep()
    {
    }
    private void OnLand()
    {
    }
}
