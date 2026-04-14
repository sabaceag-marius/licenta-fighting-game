using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DynamicBody), typeof(InputController), typeof(Core.CharacterAnimator))]
public class Character : MonoBehaviour
{
    [SerializeField]
    private CharacterStats CharacterStats;

    private InputController inputController;
    private DynamicBody dynamicBody;
    private Core.CharacterAnimator characterAnimator;

    [SerializeField]
    private List<AttackDataSO> AttackData;

    private void Awake()
    {
        inputController = GetComponent<InputController>();
        dynamicBody = GetComponent<DynamicBody>();
        characterAnimator = GetComponent<Core.CharacterAnimator>();
    }

    public Data.CharacterStats GetLogicCharacterStats(float fixedDeltaTime)
    {
        return new Data.CharacterStats
        {
            // --- Target Speeds & Instant Impulses (Passed Directly) ---
            WalkSpeed = CharacterStats.WalkSpeed / 100,
            InitialDashSpeed = CharacterStats.InitialDashSpeed / 100,
            RunningSpeed = CharacterStats.RunningSpeed / 100,
            NormalJumpForce = CharacterStats.NormalJumpForce / 100,
            ShortJumpForce = CharacterStats.ShortJumpForce / 100,
            AirJumpForce = CharacterStats.AirJumpForce / 100,
            FallSpeed = CharacterStats.FallSpeed / 100,
            AirSpeed = CharacterStats.AirSpeed / 100,
            FastFallSpeed = CharacterStats.FastFallSpeed / 100,
            AirDodgePower = CharacterStats.AirDodgePower / 100,

            // --- Continuous Forces (Multiplied by fixedDeltaTime) ---
            Traction = CharacterStats.Traction * fixedDeltaTime / 100,
            DashAcceleration = CharacterStats.DashAcceleration * fixedDeltaTime / 100,
            Gravity = CharacterStats.Gravity * fixedDeltaTime / 100,
            AirAcceleration = CharacterStats.AirAcceleration * fixedDeltaTime / 100,
            AirFriction = CharacterStats.AirFriction * fixedDeltaTime / 100,
            AirDodgeTraction = CharacterStats.AirDodgeTraction * fixedDeltaTime / 100,

            // --- Integer Frames (Passed Directly) ---
            DashFrames = CharacterStats.DashFrames,
            JumpWindupFrames = CharacterStats.JumpWindupFrames,
            LandLagFrames = CharacterStats.LandLagFrames,
            AirJumpCount = CharacterStats.AirJumpCount,
            AirDodgeFrames = CharacterStats.AirDodgeFrames
        };
    }

    public LogicDynamicBody GetLogicBody() => dynamicBody.GetLogicBody();

    public RawInput GetRawInput() => inputController.GetRawInput();

    public void SnapToState(Data.CharacterData data)
    {
        dynamicBody.transform.position = new Vector2((float)data.Position.x, (float)data.Position.y);

        transform.localScale = new Vector3(
            data.FacingDirection,
            transform.localScale.y,
            transform.localScale.z
        );

        characterAnimator.UpdateAnimation(data.CurrentState, data.StateFrame);
    }
}