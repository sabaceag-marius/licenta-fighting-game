
using Data;
using UnityEngine;

namespace Simulation
{
    public class CharacterStateMachine
    {
        private ICharacterState[] characterStates;
        
        public CharacterStateMachine()
        {
            int totalStates = System.Enum.GetValues(typeof(Data.CharacterStateType)).Length;

            characterStates = new ICharacterState[totalStates];

            //Initialize states
            characterStates[(int)CharacterStateType.AirDodge] = new AirDodgeState();
            characterStates[(int)CharacterStateType.Dash] = new DashState();
            characterStates[(int)CharacterStateType.Fall] = new FallState();
            characterStates[(int)CharacterStateType.Idle] = new IdleState();
            characterStates[(int)CharacterStateType.Jump] = new JumpState();
            characterStates[(int)CharacterStateType.Land] = new LandState();
            characterStates[(int)CharacterStateType.Run] = new RunState();
            characterStates[(int)CharacterStateType.TurnAround] = new TurnAroundState();
            characterStates[(int)CharacterStateType.Walk] = new WalkState();
            characterStates[(int)CharacterStateType.Attack ] = new AttackState();
        }

        public void AdvanceFrame(ref CharacterData character, ProcessedInput input, Data.Combat.AttackData[] characterAttacks)
        {
            // Save the state we started this frame in, in case we need to swap it

            CharacterStateType startingStateType = character.CurrentState;

            ICharacterState currentState = characterStates[(int)character.CurrentState];
            currentState.Execute(ref character, input);

            character.StateFrame++;

            DecrementCountdowns(ref character);            

            // Swap between states
            if (character.StateChanged || character.CurrentState != startingStateType)
            {
                // Debug.Log($"Changed state from {startingStateType} to {character.CurrentState}");
                currentState.Exit(ref character);

                ICharacterState newState = characterStates[(int)character.CurrentState];
                newState.Enter(ref character, input, characterAttacks);
            }
        }

        private void DecrementCountdowns(ref CharacterData character)
        {
            if (character.IgnorePlatformCollisionFrames > 0)
            {
                character.IgnorePlatformCollisionFrames--;
            }

            if (character.AirDodgeCooldown > 0)
            {
                character.AirDodgeCooldown--;
            }
        }
    }
}