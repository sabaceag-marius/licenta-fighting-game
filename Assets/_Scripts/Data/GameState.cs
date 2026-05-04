
namespace Data
{
    public struct GameState
    {
        public ushort FrameNumber;

        public LogicCollider[] StaticColliders;

        public Data.Character.CharacterData[] Characters;
    }
}