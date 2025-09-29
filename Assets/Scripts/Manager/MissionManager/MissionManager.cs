using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager instance;
    public Mission currentMission;

    private bool missionStarted = false;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {

        if (missionStarted)
        {
            currentMission?.UpdateMission();
        }
    }

    public void StartMission()
    {
        if (currentMission != null && !missionStarted)
        {
            currentMission.StartMission();
            missionStarted = true;
            Debug.Log($"Mission '{currentMission.missionName}' gestartet!");
        }
    }

    public bool MissionCompleted() => currentMission?.MissionCompleted() ?? false;
}