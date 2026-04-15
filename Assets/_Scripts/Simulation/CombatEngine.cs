
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
                if (i == j || character.HitTargets[j])
                    continue;

                ref Data.CharacterData targetCharacter = ref state.Characters[j];
                
                LogicBox hurtboxesBoundingBox;

                Data.Combat.HurtboxData[] hurtboxes = GetActiveHurtboxes(j, targetCharacter, attacks, out hurtboxesBoundingBox);

                Data.Combat.HitboxData? hitbox = CheckForCollision(frameData, hurtboxes, hurtboxesBoundingBox, character, targetCharacter);
                
                if (hitbox != null)
                {
                    character.HitTargets[j] = true;

                    Debug.Log($"Hit character! with hitbox {hitbox.Value.Id}");

                    // TODO: Apply damage, knockback etc.
                    break;
                }
            }
        }
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

                if (hitbox.Collider.CheckCollision(hurtbox.Collider) && hurtbox.State != Data.Combat.HurtboxState.Invincible)
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