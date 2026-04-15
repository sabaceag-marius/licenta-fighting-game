
namespace Data
{
    public struct CharacterData
    {
        public LogicDynamicBody DynamicBody;

        public RawInput RawInput;

        public int FacingDirection;

        public CharacterStateType CurrentState;

        public bool StateChanged;

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

        #region Attack State
        public Combat.AttackType AttackType;

        public int AttackFrameCount;

        public int AttackDurationCount;
        
        public int CurrentAttackFrame;

        /// <summary>
        /// Hurtboxes of the character when not performing an attack that overrides its hurtboxes. 
        /// Note: For now there is only one hurtbox in this list.
        /// </summary>
        public Data.Combat.HurtboxData[] Hurtboxes;
        
        #endregion
    }
}