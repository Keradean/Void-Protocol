using UnityEngine;

public class DecisionAttackPlayer : FSMDecision
{
    [Header("Condfig")]
    [SerializeField] private float attackRange;
    [SerializeField] private LayerMask playerMask;

    private EnemyBrain enemy;

    private void Awake()
    {
        enemy = GetComponent<EnemyBrain>();
    }
    public override bool Decide()
    {
        return PlayerInAttackRange();
    }

    private bool PlayerInAttackRange()
    {
        if (enemy.Player == null) return false;
        Collider[] playerColliders = Physics.OverlapSphere(enemy.transform.position, attackRange, playerMask);
        return playerColliders.Length > 0;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
