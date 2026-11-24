using Content.Shared.EntityEffects;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Oathlord.Shared.StatusEffects;

public sealed partial class StatusEffectEffectsSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private ISharedPlayerManager _player = default!;
    [Dependency] private SharedEntityEffectsSystem _effects = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<StatusEffectEffectsComponent, StatusEffectComponent>();
        var now = _timing.CurTime;
        var me = _player.LocalEntity;
        while (eqe.MoveNext(out var uid, out var effects, out var status))
        {
            if (status.AppliedTo is not { } target
                || _net.IsClient && target != me // don't predict other clients' effects
                || now < effects.NextUpdate)
                continue;

            effects.NextUpdate = now + effects.Delay;
            Dirty(uid, effects);

            _effects.ApplyEffects(target, effects.Effects, user: target);
        }
    }

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<StatusEffectEffectsComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.Delay;
        Dirty(ent);
    }
}
