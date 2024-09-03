using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    [SerializeField]
    protected GameObject playerNameUIPrefab;
    public int index = -1;

    private Transform target;
    private float moveSpeed = 5.2f;
    private Animator animator => GetComponent<Animator>();
    private CharacterController cCon => GetComponent<CharacterController>();
    private NavMeshAgent agent => this.GetComponent<NavMeshAgent>();

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
    }

    private void Flee()
    {
        Vector3 fleeDirection = (transform.position - target.position).normalized;
        Vector3 fleeTarget = transform.position + fleeDirection * 1;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeTarget, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // 別の逃げ場所を探すロジックをここに追加
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
