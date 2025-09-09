using System.Collections.Generic;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerStats stats;

    public float CurrentStamina { get; private set; }

    private void Start()
    {
        ResetStamina();
    }

    public bool CanRecoverStamina => stats.Stamina > 0 && stats.Stamina < stats.MaxStamina;

    public void UseStamina(int amount)
    {
        if (stats.Stamina >= amount)
        {
            stats.Stamina = Mathf.Max(stats.Stamina - amount, 0f);
            CurrentStamina = stats.Stamina;
        }
    }

    public void RecoverStamina(int amount)
    {
        stats.Stamina += amount;
        stats.Stamina = Mathf.Min(stats.Stamina, stats.MaxStamina);
        CurrentStamina = stats.Stamina;
    }

    public void ResetStamina()
    {
        stats.Stamina = stats.MaxStamina;
        CurrentStamina = stats.MaxStamina;
    }
}
