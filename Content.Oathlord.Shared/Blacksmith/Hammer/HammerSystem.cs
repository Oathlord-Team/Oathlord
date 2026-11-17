using Content.Oathlord.Shared.Blacksmith.Anvil;
using Content.Shared.EntityEffects;
using Content.Shared.Hands;

namespace Content.Oathlord.Shared.Blacksmith.Hammer;

public sealed partial class HammerSystem : EntitySystem
{
    [Dependency] private SharedEntityEffectsSystem _effects = default!;

    [SubscribeLocalEvent]
    public void OnBeforeAnvilOpen(Entity<HammerComponent> ent, ref HeldRelayedEvent<CanOperateAnvilAttemptEvent> args)
    {
        // we have a hammer on our hands, so we can use the anvil
        args.Args.Handled = true;
    }

    [SubscribeLocalEvent]
    public void OnHit(Entity<HammerComponent> ent, ref HeldRelayedEvent<HammerHitDoneEvent> args)
    {
        var ev = args.Args;
        var anvil = ev.Anvil;
        var user = ev.User;

        if (TerminatingOrDeleted(anvil))
            return;

        _effects.TryApplyEffects(anvil, ent.Comp.HitEffects, user: user);

        // Don't duplicate effects if we hold 2 hammers, only run for first hammer found
        args.RunOnce = true;
    }
}
