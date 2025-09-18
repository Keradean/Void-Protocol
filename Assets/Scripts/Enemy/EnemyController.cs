using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Referens")]
    [SerializeField] private PlayerController player;

    [Header("Move")]
    [SerializeField] float moveSpeed;
    [SerializeField] private Rigidbody rB;
    [SerializeField] private float chaseRange;
    [SerializeField] private float toClose;
    [SerializeField] private float strafeAmount;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolsPoints;
    [SerializeField] private Transform pointHolder;
    [HideInInspector] private int currentPatrolPoint;

    [Header("Attack")]
    [SerializeField] private float attack; 
    [SerializeField] private float attackRange; 
    [SerializeField] private float attackCooldown; 
    private float lastAttackTime;

    [Header("EnemyHealth")]
    [SerializeField] private float currentHealth;
    [HideInInspector] private bool isDeath;

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        strafeAmount = Random.Range(-0.75f, 0.75f);
        pointHolder.SetParent(null);
    }

    void Update()
    {
        // Corpse is not following me, anymore.
        if (isDeath == true) return;

        // Enemy is Moving  
        float moveY = rB.linearVelocity.y;
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance < chaseRange)
        {
            transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));

           
            if (distance <= attackRange)
            {
                rB.linearVelocity = new Vector3(0, moveY, 0);

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    AttackPlayer();
                    lastAttackTime = Time.time;
                }
            }
            else if (distance > toClose)
            {
                rB.linearVelocity = (transform.forward + (transform.right * strafeAmount)) * moveSpeed;
            }
            else
            {
                rB.linearVelocity = new Vector3(0, moveY, 0);
            }
        }
        else
        {
            if (patrolsPoints.Length > 0)
            {
                if (Vector3.Distance(transform.position, patrolsPoints[currentPatrolPoint].position) < .25f)
                {
                    currentPatrolPoint++;
                    if (currentPatrolPoint >= patrolsPoints.Length)
                    {
                        currentPatrolPoint = 0;
                    }
                }
                transform.LookAt(new Vector3(patrolsPoints[currentPatrolPoint].position.x, transform.position.y, patrolsPoints[currentPatrolPoint].position.z));
                rB.linearVelocity = new Vector3(transform.forward.x * moveSpeed, moveY, transform.forward.z * moveSpeed);
            }
            else
            {
                rB.linearVelocity = new Vector3(0, moveY, 0);
            }
        }

        rB.linearVelocity = new Vector3(rB.linearVelocity.x, moveY, rB.linearVelocity.z);
    }

    private void AttackPlayer()
    {
        if (player != null)
        {
            IDamageable damageable = player.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(attack);
            }
        }
    }

    public void TakeDamage(float damageToTake)
    {
        currentHealth -= damageToTake;
        if (currentHealth <= 0)
        {
            isDeath = true;
            rB.linearVelocity = Vector3.zero;
            GetComponent<Collider>().enabled = false;
            Destroy(gameObject);
        }
    }
}
