using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    public class CharacterAnimator : MonoBehaviour
    {
        private Animator animator;

        private Dictionary<Data.CharacterStateType, int> animationNames = new();

        private Dictionary<Data.CharacterStateType, int> animationLengths = new();

        private void Awake()
        {
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
                Debug.Log($"{stateType},{Mathf.RoundToInt(clip.length * 60f)}");

                animationLengths.Add(stateType, Mathf.RoundToInt(clip.length * 60f));
            }
        }

        public void UpdateAnimation(Data.CharacterStateType stateType, int stateFrame) //TODO: Add attack type when needed
        {
            // Check for attack state

            int animationName = animationNames[stateType];

            if (!animationLengths.TryGetValue(stateType, out int animationLength))
            {
                //Debug.LogWarning($"There was no animation found for the state {stateType}");

                animationName = animationNames[Data.CharacterStateType.Idle];
                animationLength = animationLengths[Data.CharacterStateType.Idle];
            }

            int currentFrame = stateFrame % animationLength;

            float normalizedTime = (float)currentFrame / (float)animationLength;

            animator.Play(animationName, 0, normalizedTime);
            animator.speed = 0;
        }
    }
}
