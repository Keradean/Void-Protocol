using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Vector3[] points;

    public Vector3[] Points => points;  
    public Vector3 EntityPosition { get; set; }

    private bool gameStarted;
    private void Start()
    {
        EntityPosition = transform.position;
    }

    public Vector3 GetPosition(int pointIndex)
    {
        return EntityPosition + points[pointIndex];
    }
    private void OnDrawGizmos()
    {
        if (Points == null || Points.Length == 0) return;
        Gizmos.color = Color.red;
        foreach (var point in Points)
        {
            Gizmos.DrawSphere(EntityPosition + point, 0.2f);
        }
    }
}
