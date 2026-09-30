using Content.Shared.Movement.Systems;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Oathlord.Shared.Movement;

/// <summary>
/// System that handles adjusting your walk speed when scrolling your mouse.
///
/// No public API provided, this should not be used by anything other than this system
/// </summary>
public sealed partial class ScrollingMovementSpeedSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;

    [Dependency] private EntityQuery<ScrollingMovementComponent> _scrollMoveQuery = default!;

    /// <summary>
    /// How many seconds the user has to wait before adjusting their speed again
    /// Exists mainly to prevent spam and also, we don't want users to go to max/min speed immediately which is bad UX
    /// </summary>
    private TimeSpan _delay = TimeSpan.FromSeconds(0.1f);

    [EventSubscription]
    private void OnScrollingWalkSpeed(RequestScrollingWalkSpeed msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } player
            || !_scrollMoveQuery.TryComp(player, out var scrollMove) // we do not resolve, not all entities have this component
            || TerminatingOrDeleted(player))
            return;

        if (scrollMove.NextDelay > _timing.CurTime)
            return;

        // Delta values are either 1/-1, so we need a demical number
        AdjustScrollingSpeed((player, scrollMove), msg.DeltaY * 0.1f);
    }

    [SubscribeLocalEvent]
    private void OnRefreshSpeed(Entity<ScrollingMovementComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.Current, 1.0f);
    }

    /// <summary>
    /// Adjusts the scrolling walk speed of an entity, and refreshes the movement modifiers after that
    /// </summary>
    private void AdjustScrollingSpeed(Entity<ScrollingMovementComponent> ent, float value)
    {
        var minMax = ent.Comp.WalkSpeedRange;
        ent.Comp.Current = Math.Clamp(ent.Comp.Current + value, minMax.Min, minMax.Max);
        ent.Comp.NextDelay = _timing.CurTime + _delay;
        Dirty(ent);

        _movement.RefreshMovementModifiers(ent.Owner);
    }
}

/// <summary>
/// Raised as a predictive event when the user scrolls with <see cref="OathlordKeyFunctions.MovementMod"/> keybind.
/// </summary>
/// <param name="deltaY">This is Y value of the mousewheel delta. Positive (1) means user scrolls up, negative (-1) means down</param>
[NetSerializable, Serializable]
public sealed class RequestScrollingWalkSpeed(float deltaY) : EntityEventArgs
{
    public float DeltaY = deltaY;
};
