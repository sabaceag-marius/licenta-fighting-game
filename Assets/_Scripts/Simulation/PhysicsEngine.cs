
public static class PhysicsEngine
{

    public static void SimulateCharacterPhysics(ref Data.CharacterData character, LogicCollider[] staticColliders, FixedFloat minimumSafeStepX, FixedFloat minimumSafeStepY)
    {
        // clear the variables related to collision hits
        ref LogicDynamicBody body = ref character.DynamicBody;

        body.HitCeiling = false;
        body.HitFloor = false;
        body.HitWall = false;
        body.IsGrounded = false;

        FixedVector2 totalVelocity = body.Velocity + body.ExternalVelocity;

        FixedFloat absDeltaX = FixedMath.Abs(totalVelocity.x);
        FixedFloat absDeltaY = FixedMath.Abs(totalVelocity.y);

        // Calculate required iterations for each axis based on the safe limits
        int iterationsX = FixedMath.CeilToInt(absDeltaX / minimumSafeStepX);
        int iterationsY = FixedMath.CeilToInt(absDeltaY / minimumSafeStepY);

        int iterations = FixedMath.Max(1, FixedMath.Max(iterationsX, iterationsY));
        
        for (int step = 0; step < iterations; step++)
        {
            // CRITICAL FIX: Recalculate the step velocity every loop!
            // If the character hits a wall and their Velocity.x is zeroed out (or bounced),
            // currentVelocity updates, and the remaining sub-steps will safely slide or bounce.
            FixedVector2 currentVelocity = body.Velocity + body.ExternalVelocity;
            FixedVector2 stepVelocity = currentVelocity / iterations;

            // We no longer break! Let the loop finish all iterations.
            SimulateCharacterPhysicsStep(ref character, staticColliders, stepVelocity);
        }
        // Decay external velocity ONCE per frame (after all sub-steps are done)

        if (body.ExternalVelocity != FixedVector2.zero)
        {
            body.ExternalVelocity = body.ExternalVelocity.MoveTowards(FixedVector2.zero, new FixedFloat(0.15f));
        }
    }

    private static bool SimulateCharacterPhysicsStep(ref Data.CharacterData character, LogicCollider[] staticColliders, FixedVector2 velocity)
    {
        bool collisionOccured = false;

        ref LogicDynamicBody body = ref character.DynamicBody;

        body.Position += velocity;
        body.Collider.BoundingBox = body.Collider.GetBoundingBox();

        for (int i = 0; i < staticColliders.Length; i++)
        {
            ref LogicCollider staticCollider = ref staticColliders[i];    

            if (CheckDynamicBodyCollision(ref body, staticCollider, velocity, character.IgnorePlatformCollisionFrames))
            {
                collisionOccured = true;
            }
        }

        return collisionOccured;
    }

    private static bool CheckDynamicBodyCollision(ref LogicDynamicBody body, LogicCollider staticCollider, FixedVector2 velocity, FixedFloat ignorePlatformCollisionsFrames = default)
    {
        if (staticCollider.Layer == ColliderLayer.Platform && !ShouldCheckPlatformCollisions(body, staticCollider, velocity, ignorePlatformCollisionsFrames))
            return false;

        if (!body.Collider.GetBoundingBox().CheckAABBCollision(staticCollider.BoundingBox))
            return false;

        return HandleDynamicBodyCollision(ref body, staticCollider);
    }

    private static bool ShouldCheckPlatformCollisions(LogicDynamicBody body, LogicCollider platformCollider, FixedVector2 velocity, FixedFloat ignorePlatformCollisionsFrames)
    {
        // We should only consider the platform collider solid if all of these conditions are met:
        // 1. The character is not ignoring the platforms (holding down to fall through them)
        // 2. The character is moving downwards
        // 3. The character's bottom position was above the platform before it just moved 

        if (ignorePlatformCollisionsFrames > 0)
            return false;

        if (velocity.y > 0)
            return false;

        FixedFloat bodyBottomPreviously = body.Collider.BoundingBox.Bottom - velocity.y;

        return bodyBottomPreviously >= platformCollider.Top;
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

    private static bool HandleDynamicBodyCollision(ref LogicDynamicBody dynamicBody, LogicCollider collider)
    {
        LogicCollider dynamicBodyCollider = dynamicBody.Collider;

        if (!dynamicBodyCollider.CheckCollision(collider))
            return false;

        FixedVector2 pushVector = dynamicBodyCollider.SolveCollision(collider);

        dynamicBody.Position += pushVector;

        // If we were pushed UP at all, we landed on something
        if (pushVector.y >= 0.01f)
        {
            dynamicBody.HitFloor = true;
            dynamicBody.ExternalVelocityAtImpact = dynamicBody.ExternalVelocity; // Record impact

            dynamicBody.Velocity.y = 0f;
            dynamicBody.IsGrounded = true;

            if (dynamicBody.ExternalVelocity.y < 0f) 
                dynamicBody.ExternalVelocity.y = 0f;
        }
        // If we were pushed DOWN, we hit a ceiling
        else if (-pushVector.y > 0.01f) // && dynamicBody.Velocity.y > 0f
        {
            dynamicBody.Velocity.y = 0f;

            dynamicBody.HitCeiling = true;
            dynamicBody.ExternalVelocityAtImpact = dynamicBody.ExternalVelocity; // Record impact

            if (dynamicBody.ExternalVelocity.y > 0f) dynamicBody.ExternalVelocity.y = 0f;
        }

        // If we were pushed horizontally, stop horizontal momentum into the wall
        if (FixedMath.Abs(pushVector.x) > 0.01f)
        {
            dynamicBody.HitWall = true;
            dynamicBody.ExternalVelocityAtImpact = dynamicBody.ExternalVelocity;
            
            dynamicBody.ExternalVelocity.x = 0f;

            dynamicBody.Velocity.x = 0f;
        }

        // are there cases when to not return true here?
        return true;
    }

    private static bool ShouldCheckPlatformCollisions(Data.CharacterData characterData, LogicCollider platformCollider)
    {
        // We should only consider the platform collider solid if all of these conditions are met:
        // 1. The character is not ignoring the platforms (holding down to fall through them)
        // 2. The character is moving downwards
        // 3. The character's bottom position was above the platform in the previous frame

        if (characterData.IgnorePlatformCollisionFrames > 0)
            return false;

        if (characterData.Velocity.y + characterData.ExternalVelocity.y > 0)
            return false;

        FixedFloat characterBottomPreviousFrame = characterData.DynamicBody.Collider.BoundingBox.Bottom - characterData.Velocity.y;

        return characterBottomPreviousFrame >= platformCollider.Top;
    }
}