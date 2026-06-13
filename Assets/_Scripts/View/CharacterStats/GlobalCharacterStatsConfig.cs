using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    [CreateAssetMenu(fileName = "GlobalCharacterStatsConfig", menuName = "Game Config/Global Character Stats")]
    public class GlobalCharacterStatsConfig : ScriptableObject
    {
        [Header("Universal Frame Data")]

        public int IgnorePlatformCollisionFrames = 6;

        public int JumpWindupFrames = 4;

        public int AirJumpWindupFrames = 2;

        public int LandLagFrames = 4;

        public int TechWindowFrames = 20;

        public int TechCooldownFrames = 40;

        public int InvincibilityFrames = 120;

        public int AirDodgeFrames = 40;

        public int AirDodgeCooldownFrames = 50;

        public int AirDodgesCount = 2;
        

        [Header("Universal Physics")]

        public float ExternalVelocityTraction = 0.15f;

        public float AirDodgePower = 20;

        public float AirDodgeTraction = 50;
    }
}
