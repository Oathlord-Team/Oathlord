// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.GameStates;

namespace Content.Oathlord.Shared.StatusEffects;

/// <summary>
/// Status effect component that applies effects when the status effect is applied and/or removed.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EffectsOnStatusEffectComponent : Component
{
    /// <summary>
    /// Effects to apply when the status effect is applied
    /// </summary>
    [DataField]
    public EntityEffect[]? OnApplied;

    /// <summary>
    /// Effects to run when the status effect is removed
    /// </summary>
    [DataField]
    public EntityEffect[]? OnRemoved;
}
