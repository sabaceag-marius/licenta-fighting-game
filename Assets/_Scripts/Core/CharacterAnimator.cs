using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    public class CharacterAnimator : MonoBehaviour
    {
        [SerializeField]
        private bool IsAnimated = true;

        private Animator animator;

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
        }

        public void UpdateAnimation(Data.CharacterData characterData) //TODO: Add attack type when needed
        {
            if (!IsAnimated)
                return;

            Data.CharacterStateType stateType = characterData.CurrentState;
            int stateFrame = characterData.StateFrame;

            int animationName = 0;
            int animationLength = 0;

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
        }
    }
}
