
using UnityEngine;

namespace Data
{
    public struct CharacterData
    {
        public LogicDynamicBody DynamicBody;

        public RawInput RawInput;

        public int FacingDirection;

        public CharacterStateType CurrentState;

        public int StateFrame;

        public CharacterStats Stats;

        public FixedVector2 Velocity
        {
            get => DynamicBody.Velocity;

            set
            {
                DynamicBody.Velocity = value;
            }
        }

        public FixedVector2 Position
        {
            get => DynamicBody.Position;

            set
            {
                DynamicBody.Position = value;
            }
        }

        // Specific state stuff

        public int RemainingAirJumps;

        public bool IsFastFalling;

        public FixedVector2 AirDodgeDirection;
    }
}