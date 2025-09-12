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


    [HideInInspector] private bool isDeath;


    [SerializeField] private float currentHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();

        strafeAmount = Random.Range(- 0.75f, 0.75f);

        pointHolder.SetParent(null); 
    }

    // Update is called once per frame
    void Update()
    {
        // Corpse is not following me, anymore.
        if (isDeath == true)
        {
            return;
        }

        // Enemy is Moving 
        float moveY = rB.linearVelocity.y;
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance < chaseRange)
        {
             transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));

            if (distance > toClose)
            {
                rB.linearVelocity = (transform.forward + (transform.right * strafeAmount)) * moveSpeed;
            }
            else
            {
                rB.linearVelocity = Vector3.zero;
            }
        }
        else
        {
            if (patrolsPoints.Length > 0)
            {
                if (Vector3.Distance(transform.position, patrolsPoints[currentPatrolPoint].position) < .25f)
                {
                    currentPatrolPoint++;
                    if(currentPatrolPoint >= patrolsPoints.Length)
                    {
                        currentPatrolPoint = 0;
                    }
                }

                transform.LookAt(new Vector3(patrolsPoints[currentPatrolPoint].position.x, transform.position.y, patrolsPoints[currentPatrolPoint].position.z));
                rB.linearVelocity = transform.forward * moveSpeed;
            }
            else
            {
                rB.linearVelocity = Vector3.zero;
            }
        }
        rB.linearVelocity = new Vector3(rB.linearVelocity.x, moveY, rB.linearVelocity.z);
    }

    public void TakeDamage(float damageToTake)
    {
         
        currentHealth -= damageToTake;

        if (currentHealth <= 0)
        {
            isDeath = true;
            Destroy(gameObject);
            rB.linearVelocity = Vector3.zero;
            GetComponent<Collider>().enabled = false; // if enemy is death, he isnt slide on the ground
            Debug.Log("i Hit You");
        } 
    }
}
