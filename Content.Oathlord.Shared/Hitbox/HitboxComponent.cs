// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Oathlord.Shared.Hitbox;

/// <summary>
/// Component used to represent a hitbox, with damage properties
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class HitboxComponent : Component
{
    /// <summary>
    /// The damage this hitbox will inflict on the target
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// Who initiated this hitbox.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? User;

    /// <summary>
    /// Which weapon was used to initiate the hitbox.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Weapon;

    /// <summary>
    /// How many targets this hitbox has hit.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> HitTargets = new();
}
