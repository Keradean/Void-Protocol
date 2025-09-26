using UnityEngine;

[CreateAssetMenu(fileName= "New Timer for Missions",  menuName = "Missions/MissonsTimer")]
public class MissionTimer : Mission
{
    public float time;
    private float currentTime;

    public override void StartMission()
    {
        currentTime = time;
    }

    public override void UpdateMission()
    {
        currentTime -= Time.deltaTime;

        if (currentTime < 0)
        {
            Debug.Log("Game Over Looser!!!");
        }

        string timeText = System.TimeSpan.FromSeconds(currentTime).ToString("mm\\:ss");
        Debug.Log(timeText);
    }

    public override bool MissionCompleted()
    {
        return currentTime > 0;
    }
}
