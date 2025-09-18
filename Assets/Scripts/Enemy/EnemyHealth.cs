using UnityEngine;
using UnityEngine.Pool;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private float health;

    public float CurrentHealth { get; private set;}
    private EnemyBrain enemyBrain;
    //private Animator animator;
    
    //Spawner
    private IObjectPool<EnemyHealth> enemyPool;
    public void SetPool(IObjectPool<EnemyHealth> pool)
    {
        enemyPool = pool;
    }

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
            EnemyDead();
  
        }
    }

    private void EnemyDead()
    {
        if(enemyPool != null)
        {
            enemyBrain.enabled = false;
            enemyPool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
        //Animation
        
    }


}
