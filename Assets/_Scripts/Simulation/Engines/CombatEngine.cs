
using System;
using Data.Character;
using Data.Combat;
using UnityEngine;

public static class CombatEngine
{
    public static void ProcessAttacks(ref Data.GameState state, Data.Combat.AttackData[][] attacks)
    {
        for (int i = 0; i < state.Characters.Length; i++)
        {
            ref Data.Character.CharacterData attackerCharacter = ref state.Characters[i];

            if (attackerCharacter.CurrentState != CharacterStateType.Attack)
                continue;

            Data.Combat.FrameData frameData = GetCurrentFrameData(i, attackerCharacter.AttackType, attackerCharacter.CurrentAttackFrame, attacks);

            // Frame doesn't exist or it doesn't have any hitboxes (wind-up / recovery frame)
            if (frameData.HitboxCount == 0)
                continue;

            for (int j = 0; j < state.Characters.Length; j++)
            {
                bool alreadyHit = (attackerCharacter.HitTargetsMask & (1 << j)) != 0;

                if (i == j || alreadyHit)
                    continue;

                ref Data.Character.CharacterData targetCharacter = ref state.Characters[j];
                
                LogicCollider hurtboxesBoundingBox;

                Data.Combat.HurtboxData[] hurtboxes = GetActiveHurtboxes(j, targetCharacter, attacks, out hurtboxesBoundingBox);

                Data.Combat.HitboxData? hitboxCheck = CheckForCollision(frameData, hurtboxes, hurtboxesBoundingBox, attackerCharacter, targetCharacter);
                
                if (hitboxCheck != null)
                {
                    attackerCharacter.HitTargetsMask |= (1 << j);

                    Data.Combat.HitboxData hitbox = hitboxCheck.Value;

                    // If any of the hurtboxes are in the invincible state, the target is not hit, but the hitstun still applies
                    // The invincible state should only be used when respawning and shields

                    //TODO: Change check when adding shielding
                    bool isInvincible = targetCharacter.InvincibilityFrames > 0;

                    int hitstopFrames = GetHitstopFramesCount(hitbox, targetCharacter);

                    attackerCharacter.HitstopFrames = hitstopFrames;
                    targetCharacter.HitstopFrames = hitstopFrames;

                    if (!isInvincible)
                    {
                        FixedFloat percentage = targetCharacter.Damage;
                        FixedFloat damage = hitbox.Damage;
                        FixedFloat weight = targetCharacter.Stats.Weight;
                        FixedFloat baseKnockback = hitbox.BaseKnockback;

                        attackerCharacter.Score += damage;
                        
                        // Apply the damage
                        targetCharacter.Damage += damage;

                        if (hitbox.FixedKnockback > 0)
                        {
                            percentage = 10;
                            damage = hitbox.FixedKnockback;
                        }

                        FixedFloat knockbackValue = (((percentage * 0.1f + percentage * damage * 0.2f) * 200f / (weight + 100f) * 1.4f) + 18 + baseKnockback); 

                        int hitstunFrames = (int)(knockbackValue * 0.25f);

                        FixedVector2 knockbackDirection = new FixedVector2(hitbox.LaunchDirection.x * attackerCharacter.FacingDirection, hitbox.LaunchDirection.y)
                            * knockbackValue * 0.0045f;

                        //TODO: add tuble only for high knockback
                        var hurtState = CharacterStateType.Tumble;

                        // We do this in order to always reset the target character's state
                        if (targetCharacter.CurrentState == hurtState)
                        {
                            targetCharacter.StateChanged = true;
                        }
                        else
                        {
                            targetCharacter.CurrentState = hurtState;
                        }
                        
                        targetCharacter.ExternalVelocity = knockbackDirection;
                        targetCharacter.HitstunFrames = hitstunFrames;

                        // Debug.Log($"Damage: {percentage}; Knockback: {knockbackValue * 0.0045}; Direction {knockbackDirection}");
                    }

                    break;
                }
            }
        }
    }

    private static int GetHitstopFramesCount(Data.Combat.HitboxData hitbox, CharacterData targetCharacter)
    {
        FixedFloat knockbackValue = targetCharacter.InvincibilityFrames > 0 ? 0 : targetCharacter.Damage;

        return FixedMath.Min(20, FixedMath.CeilToInt(hitbox.Damage / 3 + knockbackValue / 20));
    }

    private static Data.Combat.HitboxData? CheckForCollision(
        FrameData frameData, 
        Data.Combat.HurtboxData[] hurtboxes, 
        LogicCollider hurtboxesBoundingBox,
        CharacterData attacker,
        CharacterData target)
    {
        
        hurtboxesBoundingBox.Position = FixedMath.GetGlobalPosition(hurtboxesBoundingBox.Position, target.Position, target.FacingDirection); 

        for (int i = 0; i < frameData.HitboxCount; i++)
        {
            Data.Combat.HitboxData hitbox = frameData.Hitboxes[i];

            hitbox.Collider.Position = FixedMath.GetGlobalPosition(hitbox.Collider.Position, attacker.Position, -attacker.FacingDirection); 

            // Check the bounding box collisions

            if (!hitbox.Collider.GetBoundingBox().CheckAABBCollision(hurtboxesBoundingBox))
                continue;

            for (int j = 0; j < hurtboxes.Length; j++)
            {
                Data.Combat.HurtboxData hurtbox = hurtboxes[j];

                hurtbox.Collider.Position = FixedMath.GetGlobalPosition(hurtbox.Collider.Position, target.Position, target.FacingDirection); 

                if (hitbox.Collider.CheckCollision(hurtbox.Collider) && hurtbox.State != Data.Combat.HurtboxState.Intangible)
                    return hitbox;
            }
        }

        return null;
    }

    private static Data.Combat.HurtboxData[] GetActiveHurtboxes(
        int characterIndex,
        Data.Character.CharacterData character,
        Data.Combat.AttackData[][] attacks,
        out LogicCollider boundingBox)
    {
        if (character.CurrentState == CharacterStateType.Attack && attacks[characterIndex][(int)character.AttackType].OverrideHurtboxes)
        {
            Data.Combat.FrameData frameData = GetCurrentFrameData(characterIndex, character.AttackType, character.CurrentAttackFrame, attacks);

            boundingBox = frameData.HurtboxesBoundingBox;
            return frameData.Hurtboxes;
        }

        boundingBox = character.Hurtboxes[0].Collider.GetBoundingBox();
        return character.Hurtboxes;
    }

    private static FrameData GetCurrentFrameData(int characterIndex, Data.Combat.AttackType attackType, int attackFrame, AttackData[][] attacks)
    {
        AttackData attack = attacks[characterIndex][(int)attackType];

        return attackFrame > attack.FrameCount ? new FrameData() : attack.Frames[attackFrame];
    }
}