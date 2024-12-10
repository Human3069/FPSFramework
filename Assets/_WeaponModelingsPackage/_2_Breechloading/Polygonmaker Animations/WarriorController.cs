using Cysharp.Threading.Tasks;
using FPS_Framework;
using UnityEngine;
using UnityEngine.AI;

public class WarriorController : MonoBehaviour, IDamageable
{
    protected NavMeshAgent agent;
    protected Animator animator;

    [ReadOnly]
    [SerializeField]
    protected State _state;
    public State _State
    {
        get
        {
            return _state;
        }
        set
        {
            if (_state != value)
            {
                _state = value;

                if (value == State.Attack)
                {
                    MoveAndAttackAsync().Forget();
                }
            }
        }
    }

    protected async UniTask MoveAndAttackAsync()
    {
        FPSControllerEx foundPlayer = GameObject.FindObjectOfType<FPSControllerEx>();
        if (foundPlayer != null)
        {
            while (_State == State.Attack)
            {
                float distance = (foundPlayer.transform.position - this.transform.position).magnitude;
                if (distance > attackRange)
                {
                    while (distance > attackRange)
                    {
                        distance = (foundPlayer.transform.position - this.transform.position).magnitude;
                        Vector3 direction = (this.transform.position - foundPlayer.transform.position).normalized;
                        Vector3 targetPos = foundPlayer.transform.position + (direction * (attackRange / 2f));
                        SetDestination(targetPos);

                        await UniTaskEx.WaitForSeconds(this, 0, 0.5f);
                    }
                }

                int random = Random.Range(0, 5);
                this.transform.LookAt(foundPlayer.transform.position);
                animator.SetTrigger("attack_" + random);
                await UniTaskEx.WaitForSeconds(this, 0, attackSpeed);
            }
        }
    }

    [SerializeField]
    protected float maxHealth;
    [ReadOnly]
    [SerializeField]
    protected float _currentHealth;
    public float CurrentHealth
    {
        get
        {
            return _currentHealth;
        }
        set
        {
            if (_State != State.Dead)
            {
                if (_currentHealth > value)
                {
                    _currentHealth = Mathf.Clamp(value, 0f, maxHealth);

                    if (_currentHealth == 0f)
                    {
                        _State = State.Dead;
                        OnDead();
                    }
                    else
                    {
                        OnDamaged();
                    }
                }
                else
                {
                    _currentHealth = value;
                }
            }
        }
    }

    [Space(10)]
    [SerializeField]
    protected float attackDamage = 10f;
    [SerializeField]
    protected float attackRange = 1f;
    [SerializeField]
    protected float attackSpeed = 1f;

    public void OnDamaged()
    {
        int random = Random.Range(0, 2);
        animator.SetTrigger("hit_" + random);
    }

    public void OnDead()
    {
        agent.enabled = false;
        UniTaskEx.Cancel(this, 0);

        int random = Random.Range(0, 2);
        animator.Rebind();
        animator.SetTrigger("death_" + random);
    }

    protected void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        animator.speed = Random.Range(0.75f, 1.5f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetDestination(new Vector3(3f, 3f, 0f));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetDestination(new Vector3(-3f, -3f, 0f));
        }
    }

    protected void OnEnable()
    {
        CurrentHealth = maxHealth;
        _State = State.Attack;
    }

    protected void OnDisable()
    {
        UniTaskEx.Cancel(this, 0);
    }

    protected void SetDestination(Vector3 position)
    {
        agent.SetDestination(position);
        SetDestinationAsync().Forget();
    }

    protected async UniTaskVoid SetDestinationAsync()
    {
        animator.SetTrigger("run");
        while (agent.stoppingDistance < (this.transform.position - agent.destination).magnitude)
        {
            await UniTaskEx.NextFrame(this, 0);
        }
        animator.SetTrigger("idle");
        animator.speed = 1f;
    }
}
