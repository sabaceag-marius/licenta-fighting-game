
namespace Simulation
{
    public interface ICharacterState
    {
        void Enter(ref Data.CharacterData character, ProcessedInput input, Data.Combat.AttackData[] characterAttacks);
        void Exit(ref Data.CharacterData character);
        void Execute(ref Data.CharacterData character, ProcessedInput input, LogicCollider[] staticColliders, FixedFloat minimumSafeStepX, FixedFloat minimumSafeStepY);
        void ExecuteDuringHitstop(ref Data.CharacterData character, ProcessedInput input);
    }
}