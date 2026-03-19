
using UnityEngine.UIElements;

public static class PhysicsExtensions
{
    #region Collision detection

    public static bool CheckAABBCollision(this LogicBox boxA, LogicBox boxB)
    {
        if (boxA.Right < boxB.Left || boxA.Left > boxB.Right ||
            boxA.Top < boxB.Bottom || boxA.Bottom > boxB.Top)
            return false;

        return true;
    }

    public static bool CheckCollision(this LogicCollider colliderA, LogicCollider colliderB)
    {
        if (colliderA.Type == ColliderType.Box && colliderB.Type == ColliderType.Box)
            return colliderA.CheckCollisionBoxBox(colliderB);

        if (colliderA.Type == ColliderType.Circle && colliderB.Type == ColliderType.Box)
            return colliderA.CheckCollisionCircleBox(colliderB);

        if (colliderA.Type == ColliderType.Capsule && colliderB.Type == ColliderType.Box)
            return colliderA.CheckCollisionCapsuleBox(colliderB);

        if (colliderA.Type == ColliderType.Circle && colliderB.Type == ColliderType.Circle)
            return colliderA.CheckCollisionCircleCircle(colliderB);

        if (colliderA.Type == ColliderType.Circle && colliderB.Type == ColliderType.Capsule)
            return colliderA.CheckCollisionCircleCapsule(colliderB);

        return colliderB.CheckCollision(colliderA);
    }

    public static bool CheckCollisionBoxBox(this LogicCollider boxColliderA, LogicCollider boxColliderB)
    {
        // AABB collisions

        if (boxColliderA.Right < boxColliderB.Left || boxColliderA.Left > boxColliderB.Right ||
        boxColliderA.Top < boxColliderB.Bottom || boxColliderA.Bottom > boxColliderB.Top)
            return false;

        return true;
    }

    public static bool CheckCollisionCircleBox(this LogicCollider circleCollider, LogicCollider boxCollider)
    {
        // The circle and box are colliding if the distance between the center of the circle and 
        // the closest point of the box is less than the circle's radius

        FixedFloat closestXOnBox = FixedMath.Clamp(circleCollider.Position.x, boxCollider.Left, boxCollider.Right);
        FixedFloat closestYOnBox = FixedMath.Clamp(circleCollider.Position.y, boxCollider.Bottom, boxCollider.Top);

        FixedVector2 closestPointOnBox = new FixedVector2(closestXOnBox, closestYOnBox);

        FixedVector2 differenceVector = circleCollider.Position - closestPointOnBox;

        FixedFloat distanceSquared = (differenceVector.x * differenceVector.x) + (differenceVector.y * differenceVector.y);
        FixedFloat radiusSquared = circleCollider.Radius * circleCollider.Radius;

        //TODO: handle = case
        return distanceSquared < radiusSquared;
    }

    public static bool CheckCollisionCapsuleBox(this LogicCollider capsuleCollider, LogicCollider boxCollider)
    {
        // A capsule is made out of multiple circles, so we can reduce this collision to a Circle - Box collision
        // if we can find the circle that is the closest vertically to the box

        FixedFloat topCircleCenterY = capsuleCollider.Position.y + capsuleCollider.HalfInnerLength;

        FixedFloat bottomCircleCenterY = capsuleCollider.Position.y - capsuleCollider.HalfInnerLength;

        FixedFloat closestYOnCapsule = FixedMath.Clamp(boxCollider.Position.y, bottomCircleCenterY, topCircleCenterY);

        FixedVector2 virtualCircleCenter = new FixedVector2(capsuleCollider.Position.x, closestYOnCapsule);

        return CheckCollisionCircleBox(new LogicCollider { Position = virtualCircleCenter, Radius = capsuleCollider.Radius }, boxCollider);
    }

    public static bool CheckCollisionCircleCircle(this LogicCollider circleControllerA, LogicCollider circleControllerB)
    {
        /*
         // Calculate the distance of the centers distanceX = center1.X – center2.X distanceY = center1.Y – center2.Y    // Calculate distance based on Pythagorean theorem d = sqrt((distanceX * distanceX) + (distanceY * distanceY))  // Check collision if (d <= (r1 + r2))  return true; // Collision
         */

        FixedVector2 differenceVector = circleControllerA.Position - circleControllerB.Position;

        FixedFloat distanceSquared = (differenceVector.x * differenceVector.x) + (differenceVector.y * differenceVector.y);
        FixedFloat radiusSumSquared = (circleControllerA.Radius + circleControllerB.Radius) * (circleControllerA.Radius + circleControllerB.Radius);

        return distanceSquared < radiusSumSquared;
    }

    public static bool CheckCollisionCircleCapsule(this LogicCollider circleCollider, LogicCollider capsuleCollider)
    {
        // A capsule is made out of multiple circles, so we can reduce this collision to a Circle - Circle collision
        // if we can find the circle that is the closest vertically to the circle

        FixedFloat topCircleCenterY = capsuleCollider.Position.y + capsuleCollider.HalfInnerLength;

        FixedFloat bottomCircleCenterY = capsuleCollider.Position.y - capsuleCollider.HalfInnerLength;

        FixedFloat closestYOnCapsule = FixedMath.Clamp(circleCollider.Position.y, bottomCircleCenterY, topCircleCenterY);

        FixedVector2 virtualCircleCenter = new FixedVector2(capsuleCollider.Position.x, closestYOnCapsule);

        return CheckCollisionCircleCircle(new LogicCollider { Position = virtualCircleCenter, Radius = capsuleCollider.Radius }, circleCollider);
    }

    #endregion

    #region Collision solver

    public static FixedVector2 SolveCollision(this LogicCollider colliderA, LogicCollider colliderB)
    {
        if (colliderA.Type == ColliderType.Box && colliderB.Type == ColliderType.Box)
            return colliderA.SolveCollisionBoxBox(colliderB);

        if (colliderA.Type == ColliderType.Circle && colliderB.Type == ColliderType.Box)
            return colliderA.SolveCollisionCircleBox(colliderB);

        if (colliderA.Type == ColliderType.Capsule && colliderB.Type == ColliderType.Box)
            return colliderA.SolveCollisionCapsuleBox(colliderB);

        // --- REVERSE CHECKS ---
        // If we swapped the order to find the function, we MUST invert the push vector!
        if (colliderA.Type == ColliderType.Box && colliderB.Type == ColliderType.Circle)
        {
            FixedVector2 push = colliderB.SolveCollisionCircleBox(colliderA);
            return new FixedVector2(-push.x, -push.y);
        }

        if (colliderA.Type == ColliderType.Box && colliderB.Type == ColliderType.Capsule)
        {
            FixedVector2 push = colliderB.SolveCollisionCapsuleBox(colliderA);
            return new FixedVector2(-push.x, -push.y);
        }

        return FixedVector2.zero;
    }

    public static FixedVector2 SolveCollisionBoxBox(this LogicCollider boxA, LogicCollider boxB)
    {
        FixedFloat distanceX = boxA.Position.x - boxB.Position.x;
        FixedFloat distanceY = boxA.Position.y - boxB.Position.y;

        // Assuming Extents is half-width/half-height. If you don't have Extents, 
        // it is: (boxA.Right - boxA.Left) / 2
        FixedFloat minDistanceX = boxA.Extents.x + boxB.Extents.x;
        FixedFloat minDistanceY = boxA.Extents.y + boxB.Extents.y;

        FixedFloat absDistanceX = FixedMath.Abs(distanceX);
        FixedFloat absDistanceY = FixedMath.Abs(distanceY);

        if (absDistanceX >= minDistanceX || absDistanceY >= minDistanceY)
            return FixedVector2.zero;

        FixedFloat penetrationX = minDistanceX - absDistanceX;
        FixedFloat penetrationY = minDistanceY - absDistanceY;

        // Push out on the shortest axis
        if (penetrationX < penetrationY)
            return new FixedVector2(distanceX > 0 ? penetrationX : -penetrationX, (FixedFloat)0);
        else
            return new FixedVector2((FixedFloat)0, distanceY > 0 ? penetrationY : -penetrationY);
    }

    public static FixedVector2 SolveCollisionCircleBox(this LogicCollider circle, LogicCollider box)
    {
        FixedFloat closestX = FixedMath.Clamp(circle.Position.x, box.Left, box.Right);
        FixedFloat closestY = FixedMath.Clamp(circle.Position.y, box.Bottom, box.Top);

        FixedVector2 closestPoint = new FixedVector2(closestX, closestY);
        FixedVector2 difference = circle.Position - closestPoint;

        FixedFloat distSq = (difference.x * difference.x) + (difference.y * difference.y);

        // EDGE CASE: Center of the circle is deeply inside the box!
        if (distSq == 0)
        {
            // Find distance to all 4 edges
            FixedFloat distLeft = circle.Position.x - box.Left;
            FixedFloat distRight = box.Right - circle.Position.x;
            FixedFloat distBottom = circle.Position.y - box.Bottom;
            FixedFloat distTop = box.Top - circle.Position.y;

            // Find the absolute smallest distance to an edge
            FixedFloat minX = distLeft < distRight ? distLeft : distRight;
            FixedFloat minY = distBottom < distTop ? distBottom : distTop;
            FixedFloat min = minX < minY ? minX : minY;

            // Push out towards that closest edge, plus the radius
            if (min == distLeft) return new FixedVector2(-(distLeft + circle.Radius), (FixedFloat)0);
            if (min == distRight) return new FixedVector2(distRight + circle.Radius, (FixedFloat)0);
            if (min == distBottom) return new FixedVector2((FixedFloat)0, -(distBottom + circle.Radius));
            return new FixedVector2((FixedFloat)0, distTop + circle.Radius); // distTop
        }
        // STANDARD CASE: Shallow penetration
        else if (distSq < circle.Radius * circle.Radius)
        {
            // Note: You need a FixedMath.Sqrt here to find the actual penetration depth
            FixedFloat distance = FixedMath.Sqrt(distSq);
            FixedFloat penetrationDepth = circle.Radius - distance;

            // Normalize the difference vector to get the push direction
            FixedVector2 pushDir = new FixedVector2(difference.x / distance, difference.y / distance);

            return new FixedVector2(pushDir.x * penetrationDepth, pushDir.y * penetrationDepth);
        }

        return FixedVector2.zero;
    }

    public static FixedVector2 SolveCollisionCapsuleBox(this LogicCollider capsule, LogicCollider box)
    {
        FixedFloat topY = capsule.Position.y + capsule.HalfInnerLength;
        FixedFloat bottomY = capsule.Position.y - capsule.HalfInnerLength;

        // Find the closest point on the capsule's inner bone to the box's Y position
        FixedFloat closestYOnCapsule = FixedMath.Clamp(box.Position.y, bottomY, topY);

        // Create the Virtual Circle
        LogicCollider virtualCircle = new LogicCollider
        {
            Type = ColliderType.Circle,
            Position = new FixedVector2(capsule.Position.x, closestYOnCapsule),
            Radius = capsule.Radius
        };

        // BOOM. Zero repeated math.
        return virtualCircle.SolveCollisionCircleBox(box);
    }

    #endregion
}