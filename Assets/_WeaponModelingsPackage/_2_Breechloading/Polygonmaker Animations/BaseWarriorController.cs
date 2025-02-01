using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace FPS_Framework.ZuluWar
{
    public class BaseWarriorController : MonoBehaviour, IDamageable
    {
        protected NavMeshAgent agent;
        protected Animator animator;

        [SerializeField]
        protected Pool.UnitType unitType;

        [Space(10)]
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

                    OnStateChanged?.Invoke(value);
                }
            }
        }

        public event IDamageable.StateChanged OnStateChanged;

        public Vector3 MiddlePos
        {
            get
            {
                return this.transform.position + Vector3.up * 1.2f;
            }
        }

        public Vector3 Velocity
        {
            get
            {
                return agent.velocity;
            }
        }

        protected async UniTask MoveAndAttackAsync()
        {
            await UniTask.WaitWhile(() => FPSControllerEx.Instance == null);
            FPSControllerEx foundPlayer = FPSControllerEx.Instance;

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
                else
                {
                    _currentHealth = value;
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

        [ContextMenu("Set Height")]
        public void SetHeight()
        {
            RaycastHit hit;
            if (Physics.Raycast(this.transform.position, this.transform.up, out hit, Mathf.Infinity) == true)
            {
                if (hit.collider.gameObject.isStatic == true)
                {
                    this.transform.position = hit.point;
                }
            }
            else if (Physics.Raycast(this.transform.position, -this.transform.up, out hit, Mathf.Infinity) == true)
            {
                if (hit.collider.gameObject.isStatic == true)
                {
                    this.transform.position = hit.point;
                }
            }
        }

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

            GameManager.Instance.CountRemainedAndKilled(unitType);
        }

        protected void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            animator.speed = Random.Range(0.75f, 1.5f);
        }

        protected void OnEnable()
        {
            agent.enabled = true;

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

        public void ReturnPool()
        {
            CurrentHealth = 0f;

            if (this.gameObject.activeSelf == true)
            {
                this.gameObject.ReturnPool(unitType);
            }
        }

#if UNITY_EDITOR
        [Header("Editor Only")]
        [SerializeField]
        protected float yOffset;

        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false || FPSControllerEx.Instance == null)
            {
                return;
            }

            float distance = (FPSControllerEx.Instance.transform.position - this.transform.position).magnitude;
            GUIStyle style = new GUIStyle();
            style.onNormal.textColor = Color.red;

            Handles.Label(this.transform.position + Vector3.right * yOffset, "distance : " + distance.ToString("F0"));
        }
#endif
    }
}