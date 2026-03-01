using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HitboxData
{
    public int Id;           // Unique ID for this hitbox (e.g., for different damage types)

    public Vector2 Center;   // Position relative to the character pivot
    
    public float Radius;     // Size of the circle
}