using UnityEngine;

public class ActionChase : FSMAction
{
    [Header("Config")]
    [SerializeField] private float chaseSpeed;

    private EnemyBrain enemyBrain;

    private void Awake()
    {
        enemyBrain = GetComponent<EnemyBrain>();
    }


    public override void Act()
    {
        ChasePlayer();
    }

    private void ChasePlayer()
    {
        if (enemyBrain.Player == null) return;
        Vector3 dirToPlayer = enemyBrain.Player.position - transform.position;
        dirToPlayer.y = 0;
        if (dirToPlayer.magnitude >= 1.3f)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + dirToPlayer, chaseSpeed * Time.deltaTime);
        }
    }
}
