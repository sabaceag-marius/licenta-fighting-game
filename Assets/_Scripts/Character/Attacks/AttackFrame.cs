using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AttackFrame
{
    public int frameIndex;   // At which frame of the animation this data applies

    public List<HitboxData> Hitboxes = new List<HitboxData>();

    public List<HurtboxOverrideData> Hurtboxes = new List<HurtboxOverrideData>();
}