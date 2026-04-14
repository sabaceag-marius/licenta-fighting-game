using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Combat/Attack Data")]
public class AttackDataSO : ScriptableObject
{
    public Data.Combat.AttackType Type;

    [Tooltip("How many frames this attack will last")]
    [FormerlySerializedAs("TotalDurationFrames")]
    public int FrameCount = 60;

    [Obsolete("Set in CharacterAnimator instead")]
    [Tooltip("How many animation frames this attack contains")]
    public int TotalAnimationFrames = 60;

    [Tooltip("If true, the attack will use frame-by-frame custom hurtboxes instead of the default body capsule.")]
    public bool OverrideHurtboxes = false;

    [HideInInspector]
    public List<AttackFrame> Frames = new List<AttackFrame>();

    // Used only for the attack editor
    public AnimationClip AnimationClip;
    
    [Obsolete]
    [HideInInspector]
    public bool IsAerialAttack => (int)Type >= 7 && (int)Type <= 11;
}