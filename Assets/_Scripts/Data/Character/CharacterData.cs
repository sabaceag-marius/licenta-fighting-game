
namespace Data.Character
{
    public struct CharacterData
    {
        #region General

        public FixedVector2 Position
        {
            get => DynamicBody.Position;

            set
            {
                DynamicBody.Position = value;
            }
        }

        public FixedVector2 Velocity
        {
            get => DynamicBody.Velocity;

            set
            {
                DynamicBody.Velocity = value;
            }
        }

        public FixedVector2 ExternalVelocity
        {
            get => DynamicBody.ExternalVelocity;

            set
            {
                DynamicBody.ExternalVelocity = value;
            }
        }

        public LogicDynamicBody DynamicBody;

        public RawInput RawInput;

        //TODO: Move this from here to save space
        public CharacterStats Stats;

        public int FacingDirection;

        public int RemainingStocks;

        public FixedVector2 SpawnPosition;

        public FixedFloat Score;

        public FixedFloat Damage;

        public int IgnorePlatformCollisionFrames;

        #endregion

        #region State Machine

        public CharacterStateType CurrentState;

        public bool StateChanged;

        public int StateFrame;

        #endregion

        #region Fall State
        
        public int RemainingAirJumps;

        public bool IsFastFalling;

        #endregion

        #region Air Dodge State

        public FixedVector2 AirDodgeDirection;

        public int AirDodgeCooldown;

        public int RemainingAirDodges;

        #endregion

        #region Attack State
        public Combat.AttackType AttackType;

        public int AttackFrameCount;

        public int AttackDurationCount;
        
        public int CurrentAttackFrame;

        public bool IsAerialAttack => 
            AttackType == Combat.AttackType.AirNeutral ||
            AttackType == Combat.AttackType.AirForward ||
            AttackType == Combat.AttackType.AirBackward ||
            AttackType == Combat.AttackType.AirDownward ||
            AttackType == Combat.AttackType.AirUpward;

        /// <summary>
        /// Hurtboxes of the character when not performing an attack that overrides its hurtboxes. 
        /// Note: For now there is only one hurtbox in this list.
        /// </summary>
        public Data.Combat.HurtboxData[] Hurtboxes;
        
        public int HitTargetsMask;

        public int InvincibilityFrames;

        public int HitstopFrames;
        
        #endregion

        #region Hit & Tumble State

        public int HitstunFrames;

        public int TechWindowFrames;

        public int TechPenaltyFrames;

        #endregion
    }
}