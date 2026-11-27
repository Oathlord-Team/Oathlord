// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Oathlord.Shared.Economy.Components;

/// <summary>
/// Component used on entities that have a value on the economy.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EconomyValueComponent : Component
{
    /// <summary>
    /// The value of the item
    /// </summary>
    [DataField]
    public int Value;
}
