// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.StatusEffectNew;

namespace Content.Oathlord.Shared.StatusEffects;

public sealed partial class EffectsOnStatusEffectSystem : EntitySystem
{
    [Dependency] private SharedEntityEffectsSystem _effects = default!;

    [SubscribeLocalEvent]
    private void OnApplied(Entity<EffectsOnStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (ent.Comp.OnApplied is { } applied)
            _effects.ApplyEffects(args.Target, applied);
    }

    [SubscribeLocalEvent]
    private void OnRemoved(Entity<EffectsOnStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (ent.Comp.OnRemoved is { } removed)
            _effects.ApplyEffects(args.Target, removed);
    }
}
