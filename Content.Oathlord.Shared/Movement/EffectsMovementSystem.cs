using Content.Shared.EntityEffects;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Oathlord.Shared.Movement;

public sealed partial class EffectsMovementSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedEntityEffectsSystem _effects = default!;

    [Dependency] private EntityQuery<InputMoverComponent> _inputQuery = default!;

    [SubscribeLocalEvent]
    private void OnMove(Entity<EffectsMovementComponent> ent, ref MoveEvent args)
    {
        // help
        if (_timing.ApplyingState)
            return;

        if (!ent.Comp.CanApply)
            return;

        var curTime = _timing.CurTime;
        if (TerminatingOrDeleted(ent) || ent.Comp.NextUpdate > curTime)
            return;

        if (!_inputQuery.TryComp(ent, out var input)
            || !input.CanMove
            || input.HeldMoveButtons == MoveButtons.None // in case other forces move us, account only for our moving
            || !input.Sprinting)
            return;

        _effects.TryApplyEffect(ent, ent.Comp.Effect);

        ent.Comp.NextUpdate = curTime + ent.Comp.Delay;
        DirtyField(ent.AsNullable(), nameof(EffectsMovementComponent.NextUpdate));
    }

    [SubscribeLocalEvent]
    private void OnInserted(Entity<EffectsMovementComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (_timing.ApplyingState)
            return;

        // Don't apply effects in closets, and other entity storage entities etc
        if (args.Container.ID != SharedEntityStorageSystem.ContainerName)
            return;

        SetCanApply(ent, false);
    }

    [SubscribeLocalEvent]
    private void OnInserted(Entity<EffectsMovementComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (_timing.ApplyingState)
            return;

        if (args.Container.ID != SharedEntityStorageSystem.ContainerName)
            return;

        SetCanApply(ent, true);
    }

    private void SetCanApply(Entity<EffectsMovementComponent> ent, bool value)
    {
        ent.Comp.CanApply = value;
        DirtyField(ent.AsNullable(), nameof(EffectsMovementComponent.CanApply));
    }
}
