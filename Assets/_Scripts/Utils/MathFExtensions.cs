using UnityEngine;

public static class MathFExtensions
{
   public static void Accelerate(this ref float currentSpeed, float targetSpeed, float acceleration, float deltaTime)
   {
      currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * deltaTime);
   }

   public static void Decelerate(this ref float currentSpeed, float acceleration, float deltaTime)
   {
      currentSpeed.Accelerate(0, acceleration, deltaTime);
   }

   public static float Abs(this float value)
   {
      return Mathf.Abs(value);
   }
}