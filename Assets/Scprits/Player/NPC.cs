using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPC : MonoBehaviour
{
    [SerializeField]
    protected GameObject playerNameUIPrefab;
    public int index = -1;

    private Transform target;
    private float moveSpeed = 6f;
    private bool isJumping = false;
    private Animator animator => GetComponent<Animator>();
    private CharacterController cCon => GetComponent<CharacterController>();
    private NavMeshAgent agent => this.GetComponent<NavMeshAgent>();
    private int jumpAreaType = 3;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    void Start()
    {
        var canvas = GameObject.Find("WorldSpaceCanvas");
        var ui = Instantiate(playerNameUIPrefab, canvas.transform);
        ui.GetComponent<PlayerNameUI>().SetTargetPlayer(this.gameObject, "NPC" + index);

        agent.speed = moveSpeed;
    }

    // Update is called once per frame
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
            animator.SetFloat("Speed", agent.velocity.magnitude); // アニメーションの速度を設定
            animator.SetFloat("MotionSpeed", 1);
        }
        else
        {
            animator.SetFloat("Speed", 0); // 停止時のアニメーション
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
        float originalSpeed = agent.speed;

        // エージェントの自動位置更新を無効化
        agent.updatePosition = false;

        // オフメッシュリンクのデータを取得
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;

        Vector3 startPos = agent.transform.position;
        Vector3 endPos = linkData.endPos + Vector3.up * agent.baseOffset;

        float distance = Vector3.Distance(startPos, endPos);
        float duration = distance / (originalSpeed * 0.8f); // 移動にかかる時間を計算

        float time = 0;

        while (time < duration)
        {
            // エージェントの位置を手動で移動
            agent.transform.position = Vector3.Lerp(startPos, endPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        // 最後にエージェントを目的地に正確に配置
        agent.transform.position = endPos;

        // オフメッシュリンクの移動を完了
        agent.CompleteOffMeshLink();

        // NavMeshAgentの位置を強制的に再同期
        agent.Warp(endPos);
        animator.SetBool("Jump", false);
        animator.SetBool("Grounded", true);

        yield return new WaitForSeconds(0.5f);

        // 元のスピードに戻し、エージェントの位置更新を再度有効化
        agent.speed = originalSpeed;
        agent.updatePosition = true;
        isJumping = false;
    }

    private void Flee()
    {
        // プレイヤーの反対方向に逃げるベクトルを計算
        Vector3 fleeDirection = (agent.transform.position - target.position).normalized;
        Vector3 fleeTarget = agent.transform.position + fleeDirection * 10f; // 逃げる距離を設定

        NavMeshPath pathWithLink = new NavMeshPath();
        agent.CalculatePath(fleeTarget, pathWithLink);

        if (pathWithLink.status == NavMeshPathStatus.PathComplete)
        {
            // 正しい経路が計算された場合、目的地に設定
            agent.SetDestination(fleeTarget);

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
