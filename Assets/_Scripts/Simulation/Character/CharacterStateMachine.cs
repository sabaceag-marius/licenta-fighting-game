
using Data.Character;
using UnityEngine;

namespace Simulation
{
    public class CharacterStateMachine
    {
        private ICharacterState[] characterStates;
        
        public CharacterStateMachine()
        {
            int totalStates = System.Enum.GetValues(typeof(CharacterStateType)).Length;

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
            characterStates[(int)CharacterStateType.Hit ] = new HitState();
            characterStates[(int)CharacterStateType.Tumble ] = new TumbleState();
        }

        public void AdvanceFrame(
            ref CharacterData character, 
            ProcessedInput input, 
            Data.Combat.AttackData[] characterAttacks, 
            LogicCollider[] staticColliders, 
            FixedFloat minimumSafeStepX, 
            FixedFloat minimumSafeStepY)
        {
            // Save the state we started this frame in, in case we need to swap it

            CharacterStateType startingStateType = character.CurrentState;

            ICharacterState currentState = characterStates[(int)character.CurrentState];

            if (character.HitstopFrames > 0)
            {
                character.HitstopFrames--;
                currentState.ExecuteDuringHitstop(ref character, input);
                
                return;
            }

            currentState.Execute(ref character, input, staticColliders, minimumSafeStepX, minimumSafeStepY);

            character.StateFrame++;

            DecrementCountdowns(ref character);            

            // Swap between states
            if (character.StateChanged || character.CurrentState != startingStateType)
            {
                // Debug.Log($"Changed state from {startingStateType} to {character.CurrentState}");
                
                currentState.Exit(ref character);

                ICharacterState newState = characterStates[(int)character.CurrentState];
                newState.Enter(ref character, input, characterAttacks); // this sets the StateFrame to 0 and StateChange to false
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

            if (character.HitstunFrames > 0)
            {
                character.HitstunFrames--;
            }

            if (character.InvincibilityFrames > 0)
            {
                character.InvincibilityFrames--;
            }

            if (character.TechPenaltyFrames > 0)
            {
                character.TechPenaltyFrames--;
            }

            if (character.TechWindowFrames > 0)
            {
                character.TechWindowFrames--;
            }
        }
    }
}