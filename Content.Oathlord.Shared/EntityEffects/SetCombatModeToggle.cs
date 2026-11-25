// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CombatMode;
using Content.Shared.EntityEffects;

namespace Content.Oathlord.Shared.EntityEffects;

/// <summary>
/// Effect that sets the toggle on combat mode, disallowing or allowing you from turning off combat mode.
/// </summary>
public sealed partial class SetCombatModeToggle : EntityEffectBase<SetCombatModeToggle>
{
    /// <summary>
    /// The value to set the toggle to
    /// </summary>
    [DataField]
    public bool Toggle;
}

public sealed partial class SetCombatModeToggleEffectSystem : EntityEffectSystem<CombatModeComponent, SetCombatModeToggle>
{
    [Dependency] private SharedCombatModeSystem _combat = default!;

    protected override void Effect(Entity<CombatModeComponent> ent, ref EntityEffectEvent<SetCombatModeToggle> args)
    {
        _combat.SetToggle(ent.AsNullable(), args.Effect.Toggle);
    }
}
