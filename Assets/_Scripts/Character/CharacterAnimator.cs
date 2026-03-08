using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterAnimator : MonoBehaviour
{
    private ICharacterManager characterManager;

    private Animator animator;
    //private AnimatorOverrideController _aoc;


    private int currentState = Animator.StringToHash("IdleState");

    // Tracking manual playback for frame-perfect attacks
    private bool isManualPlaybackActive = false;

    private int currentLogicFrame = 0;
    
    private int totalLogicFrames = 0;

    private int totalAnimationFrames = 0;

    #region CachedAnimationValues
    private Dictionary<Type, int> animationStates = new Dictionary<Type, int>
    {
        { typeof(IdleState), Animator.StringToHash(nameof(IdleState)) },
        { typeof(WalkState), Animator.StringToHash(nameof(WalkState)) },
        { typeof(RunState), Animator.StringToHash(nameof(RunState)) },
        { typeof(DashState), Animator.StringToHash(nameof(DashState)) }
    };

    //TODO: List<int> for combo attacks?
    private Dictionary<AttackType, int> attackAnimationStates = new Dictionary<AttackType, int>
    {
        {AttackType.GroundNeutral, Animator.StringToHash(nameof(AttackState)+"_"+AttackType.GroundNeutral.ToString())},
        {AttackType.GroundForward, Animator.StringToHash(nameof(AttackState)+"_"+AttackType.GroundForward.ToString())},
        {AttackType.AirNeutral, Animator.StringToHash(nameof(AttackState)+"_"+AttackType.AirNeutral.ToString())},
        {AttackType.AirForward, Animator.StringToHash(nameof(AttackState)+"_"+AttackType.AirForward.ToString())},
    };

    #endregion

    private void Awake()
    {
        characterManager = GetComponent<ICharacterManager>();
        animator = GetComponent<Animator>();

        //_aoc = new AnimatorOverrideController();
        //_aoc.runtimeAnimatorController = _animator.runtimeAnimatorController;
    }

    private void FixedUpdate()
    {
        if (!isManualPlaybackActive || totalLogicFrames == 0 || totalAnimationFrames == 0)
            return;

        float logicProgress = (float)currentLogicFrame / totalLogicFrames;

        // 2. Figure out which specific animation frame that translates to, and floor it so it "snaps"
        int currentAnimFrame = Mathf.FloorToInt(logicProgress * totalAnimationFrames);

        // 3. Convert that exact animation frame back into a normalized time for the Animator
        float normalizedTime = (float)currentAnimFrame / totalAnimationFrames;

        animator.Play(currentState, 0, normalizedTime);

        if (currentLogicFrame < totalLogicFrames)
        {
            currentLogicFrame++;
        }
    }

    private void OnEnable() => characterManager.OnAnimationChanged += HandleAnimationChanged;

    private void OnDisable() => characterManager.OnAnimationChanged -= HandleAnimationChanged;

    private void HandleAnimationChanged(Type stateType, Dictionary<string, object> parameters = null)
    {
        Debug.Log($"[Animator] changing state to {stateType}");

        isManualPlaybackActive = false;
        animator.speed = 1f;

        switch (stateType)
        {
            case Type _ when stateType == typeof(AttackState):


                if (!parameters.TryGetValue(AttackState.Param_AttackType, out object attackType) || attackType is not AttackType)
                    break;

                if (attackAnimationStates.ContainsKey((AttackType)attackType))
                {
                    currentState = attackAnimationStates[(AttackType)attackType];

                    if (parameters.TryGetValue(AttackState.Param_TotalDurationFrames, out object durationObj) &&
                        parameters.TryGetValue(AttackState.Param_TotalAnimationFrames, out object animFramesObj))
                    {
                        totalLogicFrames = (int)durationObj;
                        totalAnimationFrames = (int)animFramesObj;
                        currentLogicFrame = 0;

                        isManualPlaybackActive = true;
                        animator.speed = 0f;
                    }
                }
                else
                {
                    currentState = Animator.StringToHash("IdleState");
                }

                break;

            default:
                
                if (animationStates.ContainsKey(stateType))
                {
                    currentState = animationStates[stateType];
                }
                else
                {
                    currentState = Animator.StringToHash("IdleState");
                }
                    break;
        }

        animator.Play(currentState, 0, 0);
    }
}
