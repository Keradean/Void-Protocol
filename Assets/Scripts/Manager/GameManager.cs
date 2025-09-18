using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    internal object PlayerHealth;
    [SerializeField] private Player player;

    public Player Player => player; // Public property to access the player instance


    /// <summary>
    /// Test Only
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            player.ResetStats(); // Call the ResetStats method on the player instance
        }
    }
}
