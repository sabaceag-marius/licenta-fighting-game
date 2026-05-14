using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Data.Character;

[RequireComponent(typeof(DynamicBody), typeof(Core.CharacterAnimator))]
// [RequireComponent(typeof(DynamicBody), typeof(InputController), typeof(Core.CharacterAnimator))]
public class Character : MonoBehaviour
{
    public int Index {get; set;}
    
    [Header("Debug settings")]
    public float Damage = 0;
    
    public List<AttackDataSO> Attacks;

    [SerializeField]
    private CharacterStats CharacterStats;

    private InputController inputController;

    private DynamicBody dynamicBody;

    private Core.CharacterAnimator characterAnimator;

    private BaseColliderFactory hurtboxFactory;

    private void Awake()
    {
        inputController = GetComponent<InputController>();
        dynamicBody = GetComponent<DynamicBody>();
        characterAnimator = GetComponent<Core.CharacterAnimator>();
        hurtboxFactory = GetComponents<BaseColliderFactory>().First(col => col.IsHurtbox);
    }

    public Data.Character.CharacterStats GetLogicCharacterStats(float fixedDeltaTime)
    {
        return new Data.Character.CharacterStats
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

            // --- Continuous Forces (Multiplied by fixedDeltaTime) ---
            Traction = CharacterStats.Traction * fixedDeltaTime / 100,
            DashAcceleration = CharacterStats.DashAcceleration * fixedDeltaTime / 100,
            Gravity = CharacterStats.Gravity * fixedDeltaTime / 100,
            AirAcceleration = CharacterStats.AirAcceleration * fixedDeltaTime / 100,
            AirFriction = CharacterStats.AirFriction * fixedDeltaTime / 100,

            // --- Integer Frames (Passed Directly) ---
            DashFrames = CharacterStats.DashFrames,
            AirJumpCount = CharacterStats.AirJumpCount,
            Weight = CharacterStats.Weight
        };
    }

    public LogicDynamicBody GetLogicBody() => dynamicBody.GetLogicBody();

    public LogicCollider GetHurtbox()
    {
        LogicCollider hurtbox = hurtboxFactory.GetLogicCollider();

        hurtbox.Position = hurtbox.Position - new FixedVector2(transform.position.x, transform.position.y);

        return hurtbox;
    }
    
    public RawInput GetRawInput() => inputController == null ? new RawInput() : inputController.GetRawInput();

    public void UpdateState(Data.Character.CharacterData data)
    {
        dynamicBody.transform.position = new Vector2(data.Position.x, data.Position.y);

        transform.localScale = new Vector3(
            data.FacingDirection,
            transform.localScale.y,
            transform.localScale.z
        );

        characterAnimator.UpdateAnimation(data);
    }
}