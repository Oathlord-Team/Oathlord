using Content.Shared.Movement.Components;

namespace Content.Shared.Movement.Systems;

public abstract partial class SharedMoverController
{
    [Dependency] private EntityQuery<MovementSpeedModifierComponent> _movModQuery = default!;

    /// <summary>
    /// Instead of having a set sound, get the sound based on our actual movement.
    /// If we don't have the movement speed component, calculation remains the same as before using <see cref="InputMoverComponent"/> values.
    /// </summary>
    private float GetMovementModSound(EntityUid uid, bool sprinting)
    {
        if (!_movModQuery.TryComp(uid, out var movMod))
            return sprinting ? InputMoverComponent.SprintingSoundModifier : InputMoverComponent.WalkingSoundModifier;

        // It's still too loud, that's why we reduce the sounds a bit
        return sprinting ? movMod.CurrentSprintSpeed - 8f : movMod.CurrentWalkSpeed - 12f;
    }
}
