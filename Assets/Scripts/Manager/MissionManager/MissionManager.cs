using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager instance;

    public Mission CurrentMission;

    private void Awake()
    {
         instance = this;
    }

    private void Start()
    {
        CurrentMission?.StartMission();
    }

    private void Update()
    {
        CurrentMission?.UpdateMission(); 
    }
    private void StartMission() => CurrentMission.StartMission();

    public bool MissionCompleted() => CurrentMission.MissionCompleted(); 
}
