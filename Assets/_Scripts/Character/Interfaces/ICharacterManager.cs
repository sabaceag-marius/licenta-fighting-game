using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterManager
{
    public Vector2 Velocity { get; set; }
    
    public CharacterStats Stats { get; set; }
    
    public int FacingDirection { get; set; }
    
    public int RemainingAirJumps { get; set; }

    public bool IsGrounded { get; set; }
    
    // TODO: Instead of exposing FrameInput, add a method to check in the FrameBuffer for an action
    public FrameInput Input { get; }
    
    //TODO: REMOVE THIS WHEN ADDING ANIMATIONS
    void ChangeColor(Color color);

    void Flip();

    void HandlePlatformCollisions();
}
