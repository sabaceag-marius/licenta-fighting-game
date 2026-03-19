using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

public static class PhysicsEngine
{
    public static void ApplyVelocity(ref LogicDynamicBody dynamicBody)
    {
        if (dynamicBody.IsGrounded)
        {
            dynamicBody.Velocity.y -= 0.01f.ToFixedFloat();
        }
        else // Clamp to MaximumFallSpeed
        {
            dynamicBody.Velocity.y -= dynamicBody.Gravity;
        }

        FixedVector2 position = dynamicBody.Position;

        position += dynamicBody.Velocity;

        dynamicBody.Position = position;

        dynamicBody.Collider.BoundingBox = dynamicBody.Collider.GetBoundingBox();

        //Debug.Log($"DB: {dynamicBody.Position}, {dynamicBody.Velocity}, {dynamicBody.IsGrounded}");
    }

    public static void HandleGameStateCollisions(ref GameState state)
    {
        // Dynamic body - static collider collision

        for (int i = 0; i < state.DynamicBodiesCount; i++)
        {
            ref LogicDynamicBody dynamicBody = ref state.DynamicBodies[i];

            for(int j = 0; j < state.StaticColliderCount; j++)
            {
                ref LogicCollider staticCollider = ref state.StaticColliders[j];

                // Check for layermasks

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
        if (pushVector.y > (FixedFloat)0.01f)
        {
            dynamicBody.Velocity.y = 0;
            dynamicBody.IsGrounded = true;
        }
        // If we were pushed DOWN, we hit a ceiling
        else if (-pushVector.y > (FixedFloat)0.01f && dynamicBody.Velocity.y > 0)
        {
            dynamicBody.Velocity.y = 0;
        }

        // If we were pushed horizontally, stop horizontal momentum into the wall
        if (FixedMath.Abs(pushVector.x) > (FixedFloat)0.01f)
        {
            dynamicBody.Velocity.x = 0;
        }

        //if (dynamicBodyCollider.Type == ColliderType.Box && collider.Type == ColliderType.Box)
        //{
        //    ResolveDynamicBodyCollisionBoxBox(ref dynamicBody, ref collider);
        //}
        //else if (dynamicBodyCollider.Type == ColliderType.Capsule && collider.Type == ColliderType.Box)
        //{
        //    ResolveDynamicBodyCollisionCapsuleBox(ref dynamicBody, ref collider);
        //}
    }

    private static void ResolveDynamicBodyCollisionBoxBox(ref LogicDynamicBody dynamicBody, ref LogicCollider collider)
    {
        // No need to check if the actual collision happens here, as it is equivalent to the bounding box check

        ref LogicCollider dynamicBodyCollider = ref dynamicBody.Collider;

        FixedVector2 dynamicBodyPosition = dynamicBody.Position;

        FixedFloat distanceX = dynamicBodyCollider.Position.x - collider.Position.x;
        FixedFloat distanceY = dynamicBodyCollider.Position.y - collider.Position.y;

        FixedFloat absDistanceX = FixedMath.Abs(distanceX);
        FixedFloat absDistanceY = FixedMath.Abs(distanceY);

        FixedFloat minDistanceX = dynamicBodyCollider.Extents.x + collider.Extents.x;
        FixedFloat minDistanceY = dynamicBodyCollider.Extents.y + collider.Extents.y;

        // If no overlap, do nothing
        if (absDistanceX >= minDistanceX || absDistanceY >= minDistanceY) return;

        FixedFloat penetrationX = minDistanceX - absDistanceX;
        FixedFloat penetrationY = minDistanceY - absDistanceY;

        // Wall collision
        if (penetrationX < penetrationY)
        {
            if (distanceX > 0)
            {
                dynamicBodyPosition.x += penetrationX;
            }
            else
            {
                dynamicBodyPosition.x -= penetrationX;
            }

            dynamicBody.Velocity.x = 0;
        }
        else
        {
            if (distanceY > 0)
            {
                dynamicBodyPosition.y += penetrationY;
                dynamicBody.IsGrounded = true;
            }
            else
            {
                dynamicBodyPosition.y -= penetrationY;
            }

            dynamicBody.Velocity.y = 0;
        }

        dynamicBody.Position = dynamicBodyPosition;
    }

    private static void ResolveDynamicBodyCollisionCapsuleBox(ref LogicDynamicBody dynamicBody, ref LogicCollider collider)
    {
        // Check for actual collision

        // The capsule is a box, with 2 equal circles attached at the top and bottom of the box

        LogicCollider bodyCollider = dynamicBody.Collider;

        FixedFloat topCircleCenterY = bodyCollider.Position.y + bodyCollider.HalfInnerLength;

        FixedFloat bottomCircleCenterY = bodyCollider.Position.y - bodyCollider.HalfInnerLength;

        FixedFloat closestYOnCapsule = FixedMath.Clamp(collider.Position.y, bottomCircleCenterY, topCircleCenterY);

        // We the point on the vertical axis of the capsule that is the closest to the logic collider

        FixedVector2 virtualCircleCenter = new FixedVector2(bodyCollider.Position.x, closestYOnCapsule);

        // The box collides the capsule if the closest point of the box to the capsule collides this circle

        FixedFloat closestXOnBox = FixedMath.Clamp(virtualCircleCenter.x, collider.Left, collider.Right);
        
        FixedFloat closestYOnBox = FixedMath.Clamp(virtualCircleCenter.y, collider.Bottom, collider.Top);

        // Calculate the distance between the Virtual Circle and the Box

        FixedVector2 closestPointOnBox = new FixedVector2(closestXOnBox, closestYOnBox);

        FixedVector2 distanceVector = virtualCircleCenter - closestPointOnBox;

        FixedFloat distanceSquared = (distanceVector.x * distanceVector.x) + (distanceVector.y * distanceVector.y);
        FixedFloat radiusSquared = bodyCollider.Radius * bodyCollider.Radius;

        // If the distance is less than the radius, we hit the box

        if (distanceSquared > radiusSquared)
            return;

        // Penetration resolution

        FixedVector2 pushVector = FixedVector2.zero;

        if (distanceSquared == 0)
        {
            throw new System.Exception("Probably moving too fast");
        }

        FixedFloat distance = FixedMath.Sqrt(distanceSquared);

        FixedFloat pushDepth = bodyCollider.Radius - distance;

        // Normalize the difference vector and multiply by depth

        pushVector = (distanceVector / distance) * pushDepth;

        // Move the position

        dynamicBody.Position += pushVector;

        // If we were pushed UP at all, we landed on something
        if (pushVector.y > 0.01f.ToFixedFloat())
        {
            dynamicBody.Velocity.y = 0;
            dynamicBody.IsGrounded = true;
        }
        // If we were pushed DOWN, we hit a ceiling
        else if (pushVector.y < -0.01f.ToFixedFloat() && dynamicBody.Velocity.y > 0)
        {
            dynamicBody.Velocity.y = 0;
        }

        // If we were pushed horizontally, stop horizontal momentum into the wall
        if (FixedMath.Abs(pushVector.x) > 0.01f.ToFixedFloat())
        {
            dynamicBody.Velocity.x = 0;
        }
    }

    
}