
namespace Simulation
{
    public static class GameRulesEngine
    {
        public static void CheckBlastZone(ref Data.CharacterData character, LogicBox blastzoneBoundingBox)
        {
            if (!character.DynamicBody.Collider.BoundingBox.CheckAABBCollision(blastzoneBoundingBox))
            {
                character.Position = character.SpawnPosition;
            }
        }
    }
}