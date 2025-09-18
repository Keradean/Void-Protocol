using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; } // Singleton instance of type T

    protected virtual void Awake()
    {
        Instance = this as T; // Set the singleton instance to this instance
    }
}
