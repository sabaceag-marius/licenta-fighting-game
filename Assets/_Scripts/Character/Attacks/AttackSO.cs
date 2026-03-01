using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Combat/Attack Data")]
public class AttackDataSO : ScriptableObject
{
    public AttackType Type;

    public int TotalDurationFrames = 60;

    [HideInInspector]
    public List<AttackFrame> Frames = new List<AttackFrame>();

    // Used only for the attack editor
    public AnimationClip AnimationClip;
}