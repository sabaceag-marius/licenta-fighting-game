using System;
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
    
    // OBSOLETETODO: Instead of exposing FrameInput, add a method to check in the FrameBuffer for an action
    public FrameInput Input { get; }
    
    //OBSOLETETODO: REMOVE THIS WHEN ADDING ANIMATIONS
    void ChangeColor(Color color);

    void Flip();

    void HandlePlatformCollisions();

    //OBSOLETETODO: Change to serializable class instead of scriptable object?
    //OBSOLETETODO: Move this and CharacterStats to another controller?
    AttackDataSO? GetAttack(AttackType attackType);

    T GetGameObjectComponent<T>();

    event Action<Type, Dictionary<string, object>> OnAnimationChanged;

    void TriggerAnimation(Type stateType, Dictionary<string, object> parameters = null);
}
