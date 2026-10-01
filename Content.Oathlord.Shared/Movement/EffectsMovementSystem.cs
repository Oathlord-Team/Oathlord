using Content.Shared.EntityEffects;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
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
        var curTime = _timing.CurTime;
        if (TerminatingOrDeleted(ent)
            || ent.Comp.NextUpdate > curTime
            || !_inputQuery.TryComp(ent, out var input)
            || input.HeldMoveButtons == MoveButtons.None // in case other forces move us, account only for our moving
            || !input.Sprinting)
            return;

        // did you know? not using the prototype variant will mispredict
        _effects.TryApplyEffect(ent, ent.Comp.Effect);

        ent.Comp.NextUpdate = curTime + ent.Comp.Delay;
        Dirty(ent);
    }
}
