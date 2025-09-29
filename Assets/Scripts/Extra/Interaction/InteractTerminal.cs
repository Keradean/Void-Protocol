using UnityEngine;

public class InteractTerminal : Interactable
{
    [Header("Mission Start Settings")]
    [SerializeField] private bool startMissionOnInteract = true;

    [Header("Spawner Settings")]
    [SerializeField] private Spawner enemySpawner;

    private bool hasBeenActivated = false;

    public override void Interaction()
    {
    
        if (hasBeenActivated) return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;

        // [AI-ASSISTED] Terminal activation audio feedback
        SoundManager.Instance?.PlayTerminalActivation(transform.position);

 
        if (startMissionOnInteract && MissionManager.instance != null)
        {
            MissionManager.instance.currentMission?.StartMission();
            Debug.Log("Mission gestartet!");
        }

        if (enemySpawner != null)
        {
            enemySpawner.enabled = true;
            Debug.Log("Enemy Spawner aktiviert!");
        }
        else
        {
            Debug.LogWarning("Kein Enemy Spawner zugewiesen!");
        }

        hasBeenActivated = true;

        base.Interaction();
    }
}