
public static class PhysicsEngine
{
    public static void ApplyVelocity(ref LogicDynamicBody dynamicBody)
    {
        FixedVector2 position = dynamicBody.Position;

        position += dynamicBody.Velocity;

        dynamicBody.Position = position;

        dynamicBody.Collider.BoundingBox = dynamicBody.Collider.GetBoundingBox();

        dynamicBody.IsGrounded = false;
    }

    public static void HandleGameStateCollisions(ref GameState state)
    {
        // Dynamic body - static collider collision

        for (int i = 0; i < state.CharactersCount; i++)
        {
            ref LogicDynamicBody dynamicBody = ref state.Characters[i].DynamicBody;

            for(int j = 0; j < state.StaticColliderCount; j++)
            {
                ref LogicCollider staticCollider = ref state.StaticColliders[j];

                //TODO: Check for layermasks

                if (!dynamicBody.Collider.BoundingBox.CheckAABBCollision(staticCollider.BoundingBox))
                    continue;

                HandleDynamicBodyCollision(ref dynamicBody, staticCollider);
            }
        }

        // TODO: Dynamic body - Dynamic body collision
    }

    private static void HandleDynamicBodyCollision(ref LogicDynamicBody dynamicBody, LogicCollider collider)
    {
        LogicCollider dynamicBodyCollider = dynamicBody.Collider;

        if (!dynamicBodyCollider.CheckCollision(collider))
            return;

        FixedVector2 pushVector = dynamicBodyCollider.SolveCollision(collider);

        dynamicBody.Position += pushVector;

        // If we were pushed UP at all, we landed on something
        if (pushVector.y >= 0.01f)
        {
            dynamicBody.Velocity.y = 0f;
            dynamicBody.IsGrounded = true;
        }
        // If we were pushed DOWN, we hit a ceiling
        else if (-pushVector.y > 0.01f && dynamicBody.Velocity.y > 0f)
        {
            dynamicBody.Velocity.y = 0f;
        }

        // If we were pushed horizontally, stop horizontal momentum into the wall
        if (FixedMath.Abs(pushVector.x) > 0.01f)
        {
            dynamicBody.Velocity.x = 0f;
        }
    }
}