using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Combat/Attack Data")]
public class AttackDataSO : ScriptableObject
{
    public AttackType Type;

    [Tooltip("How many frames this attack will last")]
    public int TotalDurationFrames = 60;

    [Tooltip("How many animation frames this attack contains")]
    public int TotalAnimationFrames = 60;

    [HideInInspector]
    public List<AttackFrame> Frames = new List<AttackFrame>();

    // Used only for the attack editor
    public AnimationClip AnimationClip;

    public bool IsAerialAttack => (int)Type >= 7 && (int)Type <= 11; 
}