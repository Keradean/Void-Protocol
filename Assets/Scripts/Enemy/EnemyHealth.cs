using UnityEngine;
using UnityEngine.Pool;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private float health;

    public float CurrentHealth { get; private set; }
    private EnemyBrain enemyBrain;
    private EnemyEXP enemyExp;
    //private Animator animator;

    // Spawner
    private IObjectPool<EnemyHealth> enemyPool;
    public void SetPool(IObjectPool<EnemyHealth> pool)
    {
        enemyPool = pool;
    }

    private void Awake()
    {
        enemyBrain = GetComponent<EnemyBrain>();
        enemyExp = GetComponent<EnemyEXP>();
        // animator
    }

    public void OnSpawn()
    {

        CurrentHealth = health;

        enemyBrain.enabled = true;
    }

    public void TakeDamage(float amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth <= 0f)
        {
            EnemyDead();
        }
    }

    private void EnemyDead() // Von Julian [AI-ASSISTED] Audio integration for enemy death feedback
    {
        if (enemyPool != null)
        {
            
            SoundManager.Instance?.PlaySpiderDefeat(transform.position);

            enemyBrain.enabled = false;
            enemyPool.Release(this);
            GameManager.Instance.AddPlayerExp(enemyExp.ExpDrop);
        }
    }

    /*   private void EnemyDead()
       {
           if (enemyPool != null)
           {
               enemyBrain.enabled = false;
               enemyPool.Release(this);
               GameManager.Instance.AddPlayerExp(enemyExp.ExpDrop);
           }

           // Animation
       }*/

}