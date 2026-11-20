// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Oathlord.Shared.Hitbox;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Weapons;

/// <summary>
/// Component used on mêlée weapons that work with hitboxes
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class HitboxMeleeWeaponComponent : Component
{
    /// <summary>
    /// What patterns can this weapon do.
    /// </summary>
    [DataField]
    public ProtoId<HitboxPatternsPrototype> WeaponPattern;

    /// <summary>
    /// The current pattern we're on.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int CurrentPattern = -1;

    [DataField, AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan NextPattern;

    [DataField, AutoNetworkedField]
    public AttackType NextAttack = AttackType.None;
}
