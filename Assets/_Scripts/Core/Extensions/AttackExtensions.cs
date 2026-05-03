
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AttackExtensions
{
    public static Data.Combat.AttackData GetAttackData(this AttackDataSO attackSO)
    {
        Data.Combat.AttackData attackData = new Data.Combat.AttackData
        {
            Type = attackSO.Type,
            FrameCount = attackSO.FrameCount,
            TotalDurationFrames = attackSO.TotalDurationFrames,
            OverrideHurtboxes = attackSO.OverrideHurtboxes
        };

        attackData.Frames = new Data.Combat.FrameData[attackSO.FrameCount];

        Dictionary<int, AttackFrame> attackKeyframes = new Dictionary<int, AttackFrame>();
        
        foreach (AttackFrame frame in attackSO.Frames)
        {
            attackKeyframes[frame.frameIndex] = frame;
        }

        for (int i = 0; i < attackSO.FrameCount; i++)
        {
            Data.Combat.FrameData currentFrameData = CreateEmptyFrame();

            if (attackKeyframes.TryGetValue(i, out AttackFrame keyframe))
            {
                currentFrameData = keyframe.GetFrameData();
            }

            attackData.Frames[i] = currentFrameData;
        }

        return attackData;
    }

    private static Data.Combat.FrameData GetFrameData(this AttackFrame attackFrame)
    {
        Data.Combat.FrameData frameData = new Data.Combat.FrameData();    

        // Hitboxes 

        frameData.HitboxCount = attackFrame.Hitboxes.Count;
        frameData.Hitboxes = new Data.Combat.HitboxData[frameData.HitboxCount];

        HitboxData[] sortedHitboxes = attackFrame.Hitboxes.OrderBy(i => i.Id).ToArray();

        for (int i = 0; i < frameData.HitboxCount; i++)
        {
            FixedFloat damageMultiplier = 1;

            HitboxData hitbox = sortedHitboxes[i];

            FixedVector2 launchDirection = FixedMath.GetDirectionVector(hitbox.Angle);

            frameData.Hitboxes[i] = new Data.Combat.HitboxData
            {
                Collider = new LogicCollider
                {
                    Type = ColliderType.Circle,
                    Position = hitbox.Center,
                    Radius = hitbox.Radius
                },

                Id = hitbox.Id,

                LaunchDirection = launchDirection,
                BaseKnockback = hitbox.BaseKnockback * damageMultiplier,
                FixedKnockback = hitbox.FixedKnockback * damageMultiplier,
                Damage = hitbox.Damage,
            };
        }

        // Hurtboxes

        frameData.HurtboxCount = attackFrame.Hurtboxes.Count;
        frameData.Hurtboxes = new Data.Combat.HurtboxData[frameData.HurtboxCount];

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;


        for (int i = 0; i < frameData.HurtboxCount; i++)
        {
            HurtboxOverrideData hurtbox = attackFrame.Hurtboxes[i];

            float angleRad = hurtbox.Angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(-Mathf.Sin(angleRad), Mathf.Cos(angleRad));

            float radius = hurtbox.Size.x / 2;
            float halfLength = (hurtbox.Size.y - hurtbox.Size.x) / 2f;

            frameData.Hurtboxes[i] = new Data.Combat.HurtboxData
            {
                State = (Data.Combat.HurtboxState)hurtbox.State,
                Collider = new LogicCollider
                {
                    Type = ColliderType.Capsule,
                    Position = hurtbox.Center,
                    Radius = radius,
                    HalfInnerLength = halfLength,
                    Direction = direction
                }
            };

            // Expand the float Bounding Box

            float extentX = (Mathf.Abs(direction.x) * halfLength) + radius;
            float extentY = (Mathf.Abs(direction.y) * halfLength) + radius;

            minX = Mathf.Min(minX, hurtbox.Center.x - extentX);
            maxX = Mathf.Max(maxX, hurtbox.Center.x + extentX);
            minY = Mathf.Min(minY, hurtbox.Center.y - extentY);
            maxY = Mathf.Max(maxY, hurtbox.Center.y + extentY);
        }

        // Hurtboxes Bounding Box

        if (frameData.HurtboxCount > 0)
        {
            Vector2 bbCenter = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
            Vector2 bbExtents = new Vector2((maxX - minX) / 2f, (maxY - minY) / 2f);

            frameData.HurtboxesBoundingBox = new LogicCollider
            {
                Position = bbCenter,
                Extents = bbExtents,
                Type = ColliderType.Box
            };
        }

        return frameData;
    }

    private static Data.Combat.FrameData CreateEmptyFrame()
    {
        return new Data.Combat.FrameData
        {
            HitboxCount = 0,
            Hitboxes = new Data.Combat.HitboxData[0],
            HurtboxCount = 0,
            Hurtboxes = new Data.Combat.HurtboxData[0]
        };
    }
}