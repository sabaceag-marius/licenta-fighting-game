
namespace Simulation
{
    public static class GameRulesEngine
    {
        public static void CheckBlastZone(ref Data.Character.CharacterData character, LogicCollider blastzoneBoundingBox)
        {
            if (!character.DynamicBody.Collider.GetBoundingBox().CheckAABBCollision(blastzoneBoundingBox))
            {
                RespawnCharacter(ref character);
            }
        }

        private static bool RespawnCharacter(ref Data.Character.CharacterData character)
        {
            // if (character.RemainingStocks == 1)
            //     return false;

            // Reset stock and percentage

            character.RemainingStocks--;
            character.Damage = 0;

            // Set the respawn position
            character.Position = character.SpawnPosition;

            // Reset all velocity and timers

            character.Velocity = FixedVector2.zero;
            character.ExternalVelocity = FixedVector2.zero;

            character.AirDodgeCooldown = 0;
            character.IgnorePlatformCollisionFrames = 0;
            character.HitstunFrames = 0;

            character.RemainingAirDodges = Simulation.Character.GlobalCharacterStats.AirDodgesCount;
            character.RemainingAirJumps = character.Stats.AirJumpCount;

            character.CurrentState = Data.Character.CharacterStateType.Fall;
            character.StateChanged = true;
            character.IsFastFalling = false;
            
            // Give invincibility frames
            character.InvincibilityFrames = 120;
            return true;
        }
    }
}