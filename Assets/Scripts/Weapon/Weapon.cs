using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float Range;
    public GameObject MuzzleFlare;
    public float FlareDisplayTime;

    public bool AutoFire;
    public float TimeBtwShots;

    public int CurrentAmmo;
    public int ClipSize;
    public int RemainingAmmo;

    public int pickUpValue;
    public float damage;
}
