using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class NPC : MonoBehaviour
{
    [SerializeField]
    private GameObject playerNameUIPrefab;
    public int index = -1;

    private Transform target;
    private float moveSpeed = 6f;
    private bool isJumping = false;
    private List<Transform> fleeAnchors = new List<Transform>();
    private Animator animator => GetComponent<Animator>();
    private CharacterController cCon => GetComponent<CharacterController>();
    private NavMeshAgent agent => this.GetComponent<NavMeshAgent>();
    private int jumpAreaType = 3;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    void Awake()
    {
        var anchors = GameObject.Find("NPCFleeAnchors");
        for (int i = 0; i < anchors.transform.childCount; i++)
        {
            fleeAnchors.Add(anchors.transform.GetChild(i));
        }
    }

    void Start()
    {
        var canvas = GameObject.Find("WorldSpaceCanvas");
        var ui = Instantiate(playerNameUIPrefab, canvas.transform);
        ui.GetComponent<PlayerNameUI>().SetTargetPlayer(this.gameObject, "NPC" + index);

        agent.speed = moveSpeed;
    }

    void Update()
    {
        if (target != null && GameManagerBase.instance.gameState == 1)
        {
            if (index != GameManagerBase.instance.itIndex)
            {
                Flee();
            }
            else
            {
                agent.SetDestination(target.position);
            }
            animator.SetFloat("Speed", agent.velocity.magnitude);
            animator.SetFloat("MotionSpeed", 1);
        }
        else
        {
            animator.SetFloat("Speed", 0);
            animator.SetFloat("MotionSpeed", 0);
        }

        if (agent.isOnOffMeshLink && !isJumping)
        {
            StartCoroutine(ChangeSpeedOnLink());
        }
    }

    private IEnumerator ChangeSpeedOnLink()
    {
        isJumping = true;
        animator.SetBool("Jump", true);
        animator.SetBool("Grounded", false);

        agent.updatePosition = false;

        Vector3 startPos = agent.transform.position;
        Vector3 endPos = agent.currentOffMeshLinkData.endPos + Vector3.up * agent.baseOffset;

        float duration = Vector3.Distance(startPos, endPos) / (moveSpeed * 0.8f);

        float time = 0;

        while (time < duration)
        {
            // エージェントの位置を手動で移動
            agent.transform.position = Vector3.Lerp(startPos, endPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        agent.transform.position = endPos;
        agent.CompleteOffMeshLink();


        agent.Warp(endPos);
        animator.SetBool("Jump", false);
        animator.SetBool("Grounded", true);

        // ジャンプ後の待機時間
        agent.speed = 0;
        yield return new WaitForSeconds(0.3f);

        agent.speed = moveSpeed;
        agent.updatePosition = true;
        isJumping = false;
    }

    private void Flee()
    {
        // アンカーポイントの中から、最もプレイヤーから遠いものを選択
        Transform farthestAnchor = null;
        float maxDistance = float.MinValue;

        foreach (var anchor in fleeAnchors)
        {
            float distanceToTarget = Vector3.Distance(anchor.position, target.position);
            if (distanceToTarget > maxDistance)
            {
                // NavMesh上で有効な経路かどうかを確認
                NavMeshPath path = new NavMeshPath();
                agent.CalculatePath(anchor.position, path);

                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    maxDistance = distanceToTarget;
                    farthestAnchor = anchor;
                }
            }
        }

        if (farthestAnchor != null)
        {
            // 計算されたアンカーポイントに向けてエージェントを移動させる
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(farthestAnchor.position, path);

            if (path.status == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(farthestAnchor.position);
            }

            // オフメッシュリンクに到達したらリンクを使う処理
            if (agent.isOnOffMeshLink && !isJumping)
            {
                StartCoroutine(ChangeSpeedOnLink());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NPCGameManager.instance.ChangeIt(index);
        }
    }

    private void OnFootstep()
    {
    }
    private void OnLand()
    {
    }
}
