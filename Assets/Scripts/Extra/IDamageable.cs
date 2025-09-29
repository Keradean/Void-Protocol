/*
====================================================================
IDamageable
====================================================================
Project: Space Colony Game
Course: PIP
Script-Developer: Dennis De Col 
*
WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
Diese detaillierte Authorship-Dokumentation ist für die
akademische Bewertung erforderlich und darf nicht entfernt werden!
*
====================================================================
*/
using UnityEngine;

// ==================================================
// IDAMAGEABLE INTERFACE
// ==================================================
// This is an INTERFACE that defines a contract for objects that can take damage
//
// WHAT IS AN INTERFACE?
// An interface is like a blueprint or contract that says:
// "Any class that implements me MUST have these methods"
// Interfaces don't contain actual code - just method signatures (names and parameters)
//
// WHY USE INTERFACES?
// They allow different types of objects to be treated the same way
// For example: Players, Enemies, Destructible Objects, Vehicles can ALL take damage
// Even though they're different classes, they all implement IDamageable
// This means weapons/projectiles can damage ANY object that implements this interface
//
// NAMING CONVENTION:
// Interfaces in C# typically start with "I" (like IDamageable, IInteractable, ICollectable)
//
// HOW TO USE:
// Add "IDamageable" after the class name:
// public class Enemy : MonoBehaviour, IDamageable
// Then you MUST implement the TakeDamage method in that class
public interface IDamageable
{
    // ==================================================
    // TAKE DAMAGE METHOD SIGNATURE
    // ==================================================
    // This method MUST be implemented by any class that uses IDamageable
    // It defines how the object responds when it receives damage
    //
    // PARAMETERS:
    // float amount - The amount of damage to apply (e.g. 25.0 for 25 damage points)
    //
    // EXAMPLE USAGE:
    // If a bullet hits an enemy:
    // IDamageable target = enemy.GetComponent<IDamageable>();
    // if (target != null) target.TakeDamage(25f);
    void TakeDamage(float amount); // Method that handles receiving damage
}