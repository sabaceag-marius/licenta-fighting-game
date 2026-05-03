using System.Collections.Generic;
using System.Linq;
using Data;
using Unity.VisualScripting;
using UnityEngine;

namespace Core
{
    public class CharacterAnimator : MonoBehaviour
    {
        [Header("General Settings")]
        [SerializeField]
        private bool IsAnimated = true;

        [Header("Hitstop shake settings")]
        [SerializeField]
        private float BaseShakeIntensity = 0.15f;
        private float DefenderMultiplier = 1.5f;

        private Vector3 originalSpritePosition;

        private Animator animator;

        private Transform spriteTransform;

        private Dictionary<Data.CharacterStateType, int> animationNames = new();

        private Dictionary<Data.CharacterStateType, int> animationLengths = new();

        private Dictionary<Data.Combat.AttackType, int> attackAnimationNames = new();

        private Dictionary<Data.Combat.AttackType, int> attackAnimationLengths = new();

        private void Awake()
        {
            if (!IsAnimated)
                return;

            animator = GetComponentInChildren<Animator>();

            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

            Debug.Log(clips);

            foreach(Data.CharacterStateType stateType in System.Enum.GetValues(typeof(Data.CharacterStateType)))
            {
                animationNames.Add(stateType, Animator.StringToHash(stateType.ToString()));

                AnimationClip clip = clips.FirstOrDefault(c => c.name == stateType.ToString());
                
                if (clip == null)
                { 
                    Debug.LogWarning($"There was no clip found for the state {stateType}");
                    
                    continue; 
                }

                animationLengths.Add(stateType, Mathf.RoundToInt(clip.length * 60f));
            }

            // Animations for attacks

            foreach(Data.Combat.AttackType attackType in System.Enum.GetValues(typeof(Data.Combat.AttackType)))
            {
                string animationName = $"Attack_{attackType.ToString()}";

                attackAnimationNames.Add(attackType, Animator.StringToHash(animationName));

                AnimationClip clip = clips.FirstOrDefault(c => c.name == animationName);
                
                if (clip == null)
                { 
                    Debug.LogWarning($"There was no clip found for the attack {attackType}");
                    
                    continue; 
                }

                attackAnimationLengths.Add(attackType, Mathf.RoundToInt(clip.length * 60f));
            }

            spriteTransform = animator.gameObject.transform;

            originalSpritePosition = spriteTransform.localPosition;
        }

        public void UpdateAnimation(Data.CharacterData characterData)
        {
            if (!IsAnimated)
                return;

            Data.CharacterStateType stateType = characterData.CurrentState;
            int stateFrame = characterData.StateFrame;

            int animationName;
            int animationLength;

            if (stateType != Data.CharacterStateType.Attack)
            {
                animationName = animationNames[stateType];

                if (!animationLengths.TryGetValue(stateType, out animationLength))
                {
                    //Debug.LogWarning($"There was no animation found for the state {stateType}");

                    animationName = animationNames[Data.CharacterStateType.Idle];
                    animationLength = animationLengths[Data.CharacterStateType.Idle];
                }
            }
            else
            {
                Data.Combat.AttackType attackType = characterData.AttackType;

                animationName = attackAnimationNames[attackType];

                if (!attackAnimationLengths.TryGetValue(attackType, out animationLength))
                {
                    //Debug.LogWarning($"There was no animation found for the state {stateType}");

                    animationName = animationNames[Data.CharacterStateType.Idle];
                    animationLength = animationLengths[Data.CharacterStateType.Idle];
                }
            }

            int currentFrame = stateFrame % animationLength;

            float normalizedTime = (float)currentFrame / (float)animationLength;

            animator.Play(animationName, 0, normalizedTime);
            animator.speed = 0;

            if (characterData.HitstopFrames > 0)
            {
                HandleHitstopShake(characterData);
            }
            else
            {
                spriteTransform.localPosition = originalSpritePosition;
            }
        }

        private void HandleHitstopShake(CharacterData characterData)
        {
            float currentIntensity = BaseShakeIntensity;

            if (characterData.CurrentState == Data.CharacterStateType.Hit ||
                characterData.CurrentState == Data.CharacterStateType.Tumble)
            {
                currentIntensity *= DefenderMultiplier;
            }

            float offsetX = Random.Range(-currentIntensity, currentIntensity);
            float offsetY = 0f;

            if (!characterData.DynamicBody.IsGrounded)
            {
                offsetY = Random.Range(-currentIntensity, currentIntensity);
            }

            spriteTransform.localPosition = originalSpritePosition + new Vector3(offsetX, offsetY, 0f);
        }
    }
}
