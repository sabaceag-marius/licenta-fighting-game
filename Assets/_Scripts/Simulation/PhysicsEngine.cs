
public static class PhysicsEngine
{
    // FixedFloat minStepX, FixedFloat minStepY set as static
    public static void SimulateCharacterPhysics(ref Data.CharacterData character, LogicCollider[] staticColliders)
    {

        // clear the variables related to collision hits

        ref LogicDynamicBody dynamicBody = ref character.DynamicBody;

        ApplyVelocity(ref dynamicBody);

        for(int i = 0; i < staticColliders.Length; i++)
        {
            LogicCollider staticCollider = staticColliders[i];

            if (staticCollider.Layer == ColliderLayer.Platform && !ShouldCheckPlatformCollisions(character, staticCollider))
                continue;

            if (!dynamicBody.Collider.BoundingBox.CheckAABBCollision(staticCollider.BoundingBox))
                continue;

            HandleDynamicBodyCollision(ref dynamicBody, staticCollider);
        }
    }

    public static void ApplyVelocity(ref LogicDynamicBody dynamicBody)
    {
        FixedVector2 position = dynamicBody.Position;

        position += dynamicBody.Velocity;

        if (dynamicBody.ExternalVelocity != FixedVector2.zero)
        {
            position += dynamicBody.ExternalVelocity;

            dynamicBody.ExternalVelocity = dynamicBody.ExternalVelocity.MoveTowards(FixedVector2.zero, 0.15f);
        }

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

                if (staticCollider.Layer == ColliderLayer.Platform && !ShouldCheckPlatformCollisions(state.Characters[i], staticCollider))
                    continue;

                if (!dynamicBody.Collider.BoundingBox.CheckAABBCollision(staticCollider.BoundingBox))
                    continue;

                HandleDynamicBodyCollision(ref dynamicBody, staticCollider);
            }
        }

        // TODO: Dynamic body - Dynamic body collision ?
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

    private static bool ShouldCheckPlatformCollisions(Data.CharacterData characterData, LogicCollider platformCollider)
    {
        // We should only consider the platform collider solid if all of these conditions are met:
        // 1. The character is not ignoring the platforms (holding down to fall through them)
        // 2. The character is moving downwards
        // 3. The character's bottom position was above the platform in the previous frame

        if (characterData.IgnorePlatformCollisionFrames > 0)
            return false;

        if (characterData.Velocity.y > 0)
            return false;

        FixedFloat characterBottomPreviousFrame = characterData.DynamicBody.Collider.BoundingBox.Bottom - characterData.Velocity.y;

        return characterBottomPreviousFrame >= platformCollider.Top;
    }
}