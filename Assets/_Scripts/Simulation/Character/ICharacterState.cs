
namespace Simulation
{
    public interface ICharacterState
    {
        void Enter(ref Data.CharacterData player, ProcessedInput input, Data.Combat.AttackData[] characterAttacks);
        void Exit(ref Data.CharacterData player);
        void Execute(ref Data.CharacterData player, ProcessedInput input);
    }
}