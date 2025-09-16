using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private float health; 

    public float CurrentHealth { get; private set;}
    private EnemyBrain enemyBrain;
    //private Animator animator;

    private void Start()
    {
        CurrentHealth = health;
    }

    private void Awake()
    {
        enemyBrain = GetComponent<EnemyBrain>();   
       // animator
    }
    public void TakeDamage(float amount)
    {
        CurrentHealth -= amount;
        if(CurrentHealth <= 0f)
        {
            enemyBrain.enabled = false;
            Destroy(gameObject);
            //animator.SetTrigger(Dead)
            
        }
    }

}
