using UnityEngine;

public abstract class Mission : ScriptableObject
{
    public string missionName;
    [TextArea] public string missiondescription; 
    public abstract void StartMission();
    public abstract bool MissionCompleted();

    public virtual void UpdateMission()
    {
       
    }

}
