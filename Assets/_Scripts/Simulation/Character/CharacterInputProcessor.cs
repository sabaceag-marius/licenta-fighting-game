
using System;
using UnityEngine;

namespace Simulation
{
    public class CharacterInputProcessor
    {
        private const int minimumFlickValue = 45;

        //TODO: Add previous frame input
        public ProcessedInput ProcessInput(RawInput rawInput, RawInput previousRawInput)
        {
            ProcessedInput input = new ProcessedInput();

            input.Movement = new FixedVector2((FixedFloat)rawInput.LeftStickX / 100f,
                (FixedFloat)rawInput.LeftStickY / 100f);

            input.JumpHeld = rawInput.Jumped;

            input.JumpPressed = rawInput.Jumped && !previousRawInput.Jumped;

            input.AttackPressed = rawInput.Attacked && !previousRawInput.Attacked;

            input.DodgePressed = rawInput.Dodged;

            // Flick inputs

            int absCurrentX = Math.Abs(rawInput.LeftStickX);
            int absPrevX = Math.Abs(previousRawInput.LeftStickX);

            int absCurrentY = Math.Abs(rawInput.LeftStickY);
            int absPrevY = Math.Abs(previousRawInput.LeftStickY);

            if (absCurrentX - absPrevX >= minimumFlickValue
            && absCurrentX >= minimumFlickValue)
            {
                input.FlickDirection.x = Math.Sign(rawInput.LeftStickX);
            }

            if (absCurrentY - absPrevY >= minimumFlickValue
            && absCurrentY >= minimumFlickValue)
            {
                input.FlickDirection.y = Math.Sign(rawInput.LeftStickY);
            }

            input.Dashed = FixedMath.Abs(input.FlickDirection.x) > 0;

            input.FastFalled = input.FlickDirection.y == -1;

            return input;
        }
    }
}