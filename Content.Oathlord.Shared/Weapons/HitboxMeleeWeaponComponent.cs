// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Oathlord.Shared.Hitbox;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Oathlord.Shared.Weapons;

/// <summary>
/// Component used on mêlée weapons that work with hitboxes
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
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

    /// <summary>
    /// The time until the next pattern.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField, AutoNetworkedField]
    public TimeSpan NextPattern;

    /// <summary>
    /// What type is the next attack?
    /// </summary>
    [DataField, AutoNetworkedField]
    public AttackType NextAttack = AttackType.None;

    /// <summary>
    /// What type was the previous attack?
    /// Used to check against the new attack type, and the previous one.
    ///
    /// If it's not the same, then we reset the pattern to start the other attack type's pattern.
    /// </summary>
    [DataField, AutoNetworkedField]
    public AttackType PrevAttack = AttackType.None;
}
