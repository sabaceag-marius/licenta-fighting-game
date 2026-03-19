
public class GameSimulation
{
    public GameSimulation()
    {
        
    }

    public void AdvanceFrame(ref GameState gameState, FrameInput input) // Add input later
    {
        // Logic for the players based on the state machine and input

        // Apply velocity to dynamic bodies

        for (int i = 0; i < gameState.DynamicBodiesCount; i++)
        {
            ref LogicDynamicBody dynamicBody = ref gameState.DynamicBodies[i];

            dynamicBody.Velocity = input.Movement.ToFixedVector2() * dynamicBody.MovementSpeed;

            PhysicsEngine.ApplyVelocity(ref dynamicBody);

            dynamicBody.IsGrounded = false;
        }

        // Check for all collisions for the colliders from the gameState in the PhysicsEngine

        // TODO: Separate this from the hitbox hurtbox collision when implementing combat

        // Check collisions between the dynamic bodies colliders and the static colliders
        PhysicsEngine.HandleGameStateCollisions(ref gameState);
    }
}
