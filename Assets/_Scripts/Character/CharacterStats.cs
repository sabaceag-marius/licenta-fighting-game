using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class CharacterStats
{
    // [Header("MOVEMENT")]
    // [Tooltip("The top horizontal movement speed")]
    
    [Header("Ground movement")]
    
    [Tooltip("The speed of the character when walking")]
    public float WalkSpeed;

    [Tooltip("The rate at which the character stops moving horizontally when grounded")]
    public float Traction;
    
    [Tooltip("The initial speed of a character at the start of a dash")]
    public float InitialDashSpeed;
    
    [Tooltip("The rate at which the character's speed is increased while dashing or running")]
    public float DashAcceleration;

    [Tooltip("The minimum number of frames a character has to dash")]
    public int DashFrames;

    [Tooltip("The maximum rate at which the character can move while running")]
    public float RunningSpeed;

    // public int TurnAroundFrames = 40;

    [Tooltip("The force of the character's normal jump")]
    public float NormalJumpForce;

    [Tooltip("The force of the character's short jump")]
    public float ShortJumpForce;

    [Tooltip("The force of the character's air jump")]
    public float AirJumpForce;

    [Tooltip("The number of jumps the character can perform midair")]
    public int AirJumpCount = 1;

    [Tooltip("The number of frames it takes the character to start jumping after pressing the button")]
    public int JumpWindupFrames = 4;
    
    [Tooltip("The number of frames a character can't move after landing")]
    public int LandLagFrames = 6;
    
    [Header("Air Movement")]
    
    [Tooltip("Maximum rate at which the character can move down while midair")]
    public float FallSpeed;

    [Tooltip("Measure of how fast the character reaches their maximum falling speed")]
    public float Gravity;

    [Tooltip("The maximum rate at which the character can move horizontally while midair")]
    public float AirSpeed;
    
    [Tooltip("Measure of how fast the character can change their horizontal velocity in midair")]
    public float AirAcceleration;
    
    [Tooltip("The rate at which the character stops moving horizontally while midair")]
    public float AirFriction;

    [Tooltip("Maximum rate at which the character can move down while midair when fast falling")]
    public float FastFallSpeed;

    public float AirDodgePower = 20;
    
    public float AirDodgeTraction = 20;
    
    public int AirDodgeFrames = 40;
}
