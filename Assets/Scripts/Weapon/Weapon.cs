using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float range;
    [SerializeField] private GameObject muzzelFlare;
    [SerializeField] private float flareTime;

    [Header("Fire Settings")]
    [SerializeField] private bool canAutoFire;
    [SerializeField] private float autoFireRate;
    [SerializeField] private float singleShotCooldown;

    [Header("Stats Reference")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private float damageValue;
}
