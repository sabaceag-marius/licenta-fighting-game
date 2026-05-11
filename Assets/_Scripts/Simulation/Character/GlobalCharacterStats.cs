
namespace Simulation.Character
{
    public static class GlobalCharacterStats
    {
        public static int IgnorePlatformCollisionFrames = 6;

        public static FixedFloat ExternalVelocityTraction = 0.15f;

        public static FixedFloat AirDodgePower = 20;

        public static FixedFloat AirDodgeTraction = 50;

        public static int AirDodgeFrames = 40;

        public static int AirDodgeCooldownFrames = 50;

        public static int AirDodgesCount = 2;

        public static int JumpWindupFrames = 4;

        public static int AirJumpWindupFrames = 2;

        public static int LandLagFrames = 4;

        public static int TechWindowFrames = 20;

        public static int TechCooldownFrames = 40;

        public static int InvincibilityFrames = 120;

        public static GlobalCharacterStatsSerializable GetData()
        {
            return new GlobalCharacterStatsSerializable
            {
                IgnorePlatformCollisionFrames = IgnorePlatformCollisionFrames,
                ExternalVelocityTraction = ExternalVelocityTraction,
                AirDodgePower = AirDodgePower,
                AirDodgeTraction = AirDodgeTraction,
                AirDodgeFrames = AirDodgeFrames,
                AirDodgeCooldownFrames = AirDodgeCooldownFrames,
                AirDodgesCount = AirDodgesCount,
                JumpWindupFrames = JumpWindupFrames,
                AirJumpWindupFrames = AirJumpWindupFrames,
                LandLagFrames = LandLagFrames,
                TechWindowFrames = TechWindowFrames,
                TechCooldownFrames = TechCooldownFrames,
                InvincibilityFrames = InvincibilityFrames
            };
        }

        public static void ApplyData(GlobalCharacterStatsSerializable data)
        {
            IgnorePlatformCollisionFrames = data.IgnorePlatformCollisionFrames;
            ExternalVelocityTraction = data.ExternalVelocityTraction;
            AirDodgePower = data.AirDodgePower;
            AirDodgeTraction = data.AirDodgeTraction;
            AirDodgeFrames = data.AirDodgeFrames;
            AirDodgeCooldownFrames = data.AirDodgeCooldownFrames;
            AirDodgesCount = data.AirDodgesCount;
            JumpWindupFrames = data.JumpWindupFrames;
            AirJumpWindupFrames = data.AirJumpWindupFrames;
            LandLagFrames = data.LandLagFrames;
            TechWindowFrames = data.TechWindowFrames;
            TechCooldownFrames = data.TechCooldownFrames;
            InvincibilityFrames = data.InvincibilityFrames;
        }
    }

    [System.Serializable]
     public class GlobalCharacterStatsSerializable
    {
        public  int IgnorePlatformCollisionFrames = 6;

        public  FixedFloat ExternalVelocityTraction = 0.15f;

        public  FixedFloat AirDodgePower = 20;

        public  FixedFloat AirDodgeTraction = 50;

        public  int AirDodgeFrames = 40;

        public  int AirDodgeCooldownFrames = 50;

        public  int AirDodgesCount = 2;

        public  int JumpWindupFrames = 4;

        public  int AirJumpWindupFrames = 2;

        public  int LandLagFrames = 4;

        public  int TechWindowFrames = 20;

        public  int TechCooldownFrames = 40;

        public  int InvincibilityFrames = 120;
    }
}