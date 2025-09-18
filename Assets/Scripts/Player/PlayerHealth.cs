using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private PlayerStats stats;

    [Header("References")]
    [SerializeField] private UIManager uiManager;

    private void Awake()
    {
        if (uiManager == null)
        {
            uiManager = GetComponent<UIManager>();

        }
    }

    public void TakeDamage(float amount)
    {
        if (stats == null) return; 

        stats.Health -= amount;
        if (stats.Health <= 0f)
        {
            stats.Health = 0f;
   
            PlayerDead();
        }
    }

    public void RestoreHealth(float amount)
    {
        if (stats == null) return;

        stats.Health += amount;
        if (stats.Health > stats.MaxHealth)
        {
            stats.Health = stats.MaxHealth;
        }
    }

    public bool CanRestoreHealth()
    {

        if (stats == null) return false;
        return stats.Health > 0 && stats.Health < stats.MaxHealth;
    }

    private void PlayerDead()
    {
        if (uiManager == null) return; 
        uiManager.ShowDeathScreen();
        Debug.Log("I am Dead");
    }
}