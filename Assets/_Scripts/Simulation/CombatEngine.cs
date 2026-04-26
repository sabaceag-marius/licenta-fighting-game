
using System;
using Data;
using Data.Combat;
using UnityEngine;

public static class CombatEngine
{
    public static void ProcessAttacks(ref GameState state, Data.Combat.AttackData[][] attacks)
    {
        for (int i = 0; i < state.CharactersCount; i++)
        {
            ref Data.CharacterData character = ref state.Characters[i];

            if (character.CurrentState != Data.CharacterStateType.Attack)
                continue;

            Data.Combat.FrameData frameData = GetCurrentFrameData(i, character.AttackType, character.CurrentAttackFrame, attacks);

            // Frame doesn't exist or it doesn't have any hitboxes (wind-up / recovery frame)
            if (frameData.HitboxCount == 0)
                continue;

            for (int j = 0; j < state.CharactersCount; j++)
            {
                bool alreadyHit = (character.HitTargetsMask & (1 << j)) != 0;

                if (i == j || alreadyHit)
                    continue;

                ref Data.CharacterData targetCharacter = ref state.Characters[j];
                
                LogicBox hurtboxesBoundingBox;

                Data.Combat.HurtboxData[] hurtboxes = GetActiveHurtboxes(j, targetCharacter, attacks, out hurtboxesBoundingBox);

                Data.Combat.HitboxData? hitboxCheck = CheckForCollision(frameData, hurtboxes, hurtboxesBoundingBox, character, targetCharacter);
                
                if (hitboxCheck != null)
                {
                    character.HitTargetsMask |= (1 << j);

                    Data.Combat.HitboxData hitbox = hitboxCheck.Value;

                    Debug.Log($"Hit character! with hitbox {hitbox.Id}");

                    // If any of the hurtboxes are in the invincible state, the target is not hit, but the hitstun still applies
                    // The invincible state should only be used when respawning and shields

                    bool isInvincible = CheckIfInvincible(hurtboxes);

                    // TODO: Apply damage, knockback etc.
                    if (!isInvincible)
                    {
                        // Apply the damage
                        targetCharacter.DamagePercentage += hitbox.Damage;

                        FixedFloat percentage = targetCharacter.DamagePercentage;
                        FixedFloat damage = hitbox.Damage;
                        FixedFloat weight = targetCharacter.Stats.Weight;
                        FixedFloat baseKnockback = hitbox.BaseKnockback;

                        if (hitbox.FixedKnockback > 0)
                        {
                            percentage = 10;
                            damage = hitbox.FixedKnockback;
                        }

                        FixedFloat knockbackValue = (((percentage * 0.1f + percentage * damage * 0.2f) * 200f / (weight + 100f) * 1.4f) + 18 + baseKnockback); 

                        int hitstunFrames = (int)(knockbackValue * 0.4f);

                        FixedVector2 knockbackDirection = new FixedVector2(hitbox.LaunchDirection.x * character.FacingDirection, hitbox.LaunchDirection.y)
                            * knockbackValue * 0.0045f;

                        // To reset the Hit state
                        targetCharacter.StateChanged = true;

                        //TODO: add tuble for high knockback
                        targetCharacter.CurrentState = CharacterStateType.Hit;

                        targetCharacter.ExternalVelocity = knockbackDirection;
                        targetCharacter.HitstunFrames = hitstunFrames;

                        Debug.Log($"Damage: {percentage}; Knockback: {knockbackValue * 0.0045}; Direction {knockbackDirection}");
                    }

                    break;
                }
            }
        }
    }

    private static bool CheckIfInvincible(Data.Combat.HurtboxData[] hurtboxes)
    {
        for (int i = 0; i < hurtboxes.Length; i++)
        {
            if (hurtboxes[i].State == Data.Combat.HurtboxState.Invincible)
                return true;
        }

        return false;
    }

    private static Data.Combat.HitboxData? CheckForCollision(
        FrameData frameData, 
        Data.Combat.HurtboxData[] hurtboxes, 
        LogicBox hurtboxesBoundingBox,
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
        Data.CharacterData character,
        Data.Combat.AttackData[][] attacks,
        out LogicBox boundingBox)
    {
        if (character.CurrentState == Data.CharacterStateType.Attack && attacks[characterIndex][(int)character.AttackType].OverrideHurtboxes)
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