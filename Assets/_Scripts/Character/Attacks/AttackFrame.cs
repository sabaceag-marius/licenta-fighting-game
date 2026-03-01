using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AttackFrame
{
    public int frameIndex;   // At which frame of the animation this data applies

    public List<HitboxData> hitboxes = new List<HitboxData>();
}