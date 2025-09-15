using UnityEngine;

public class DetectPlayer : FSMDecision 
{
    [Header("Condfig")]
    [SerializeField] private float range; 
    [SerializeField] private LayerMask playerMask; 

    private EnemyBrain enemy;

    private void Awake()
    {
        enemy = GetComponent<EnemyBrain>();
    }
    public override bool Decide()
    {
        return DecisionPlayerDetected(); 
    }

    private bool DecisionPlayerDetected()
    {
        Collider[] playerColliders = Physics.OverlapSphere(enemy.transform.position, range, playerMask);
        if (playerColliders.Length > 0)
        {
            enemy.Player = playerColliders[0].transform;
            return true;
        }

        enemy.Player = null;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
       Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
