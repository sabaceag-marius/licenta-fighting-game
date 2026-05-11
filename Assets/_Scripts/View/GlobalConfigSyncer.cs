using UnityEngine;
using Data;

namespace View
{
    
public class GlobalConfigSyncer : MonoBehaviour
{
    [SerializeField]
    private GlobalCharacterStatsConfig StatsConfig;

    private void Awake()
    {
        SyncStats();
    }

    private void Update()
    {
#if UNITY_EDITOR
        SyncStats();
#endif
    }

    private void SyncStats()
    {
        if (StatsConfig == null) return;

        FixedFloat fixedDeltaTime = (FixedFloat)1f / 60;

        Simulation.Character.GlobalCharacterStats.IgnorePlatformCollisionFrames = StatsConfig.IgnorePlatformCollisionFrames;
        Simulation.Character.GlobalCharacterStats.JumpWindupFrames = StatsConfig.JumpWindupFrames;
        Simulation.Character.GlobalCharacterStats.AirJumpWindupFrames = StatsConfig.AirJumpWindupFrames;
        Simulation.Character.GlobalCharacterStats.LandLagFrames = StatsConfig.LandLagFrames;
        Simulation.Character.GlobalCharacterStats.TechWindowFrames = StatsConfig.TechWindowFrames;
        Simulation.Character.GlobalCharacterStats.TechCooldownFrames = StatsConfig.TechCooldownFrames;
        Simulation.Character.GlobalCharacterStats.InvincibilityFrames = StatsConfig.InvincibilityFrames;
        Simulation.Character.GlobalCharacterStats.AirDodgeFrames = StatsConfig.AirDodgeFrames;
        Simulation.Character.GlobalCharacterStats.AirDodgeCooldownFrames = StatsConfig.AirDodgeCooldownFrames;
        Simulation.Character.GlobalCharacterStats.AirDodgesCount = StatsConfig.AirDodgesCount;

        Simulation.Character.GlobalCharacterStats.AirDodgePower = StatsConfig.AirDodgePower / 100;
        Simulation.Character.GlobalCharacterStats.ExternalVelocityTraction = StatsConfig.ExternalVelocityTraction * fixedDeltaTime / 100;
        Simulation.Character.GlobalCharacterStats.AirDodgeTraction = StatsConfig.AirDodgeTraction * fixedDeltaTime / 100;
    }
}
}