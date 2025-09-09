using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable // Interface for objects that can take damage
{
    void TakeDamage(float amount); // Method to apply damage to the object

}
