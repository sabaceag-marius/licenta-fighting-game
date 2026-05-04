
namespace Simulation
{
    public interface ICharacterState
    {
        void Enter(ref Data.Character.CharacterData character, ProcessedInput input, Data.Combat.AttackData[] characterAttacks);
        void Exit(ref Data.Character.CharacterData character);
        void Execute(ref Data.Character.CharacterData character, ProcessedInput input, LogicCollider[] staticColliders, FixedFloat minimumSafeStepX, FixedFloat minimumSafeStepY);
        void ExecuteDuringHitstop(ref Data.Character.CharacterData character, ProcessedInput input);
    }
}