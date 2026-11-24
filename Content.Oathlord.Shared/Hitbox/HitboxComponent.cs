// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

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
    [DataField(required: true)]
    public DamageConfig DamageConfig;

    /// <summary>
    /// The color of the hit effect.
    /// </summary>
    [DataField]
    public Color HitColor = Color.Red;

    /// <summary>
    /// If true, the collision can re-trigger hits, ignoring <see cref="HitTargets"/>
    /// </summary>
    [DataField]
    public bool MultiHit;

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

/// <summary>
/// A record struct holding data that gets passed to damage methods.
/// </summary>
/// <param name="Damage">The damage specified to inflict on the target</param>
/// <param name="IgnoreResistances">Whether the damage should ignore resistances</param>
/// <param name="IgnoreGlobalModifiers">Whether the damage should ignore global modifiers</param>
/// <param name="InterruptDoAfters">Whether the damage should interrupt all do-afters</param>
[DataRecord]
public partial record struct DamageConfig(
    DamageSpecifier Damage,
    bool IgnoreResistances = false,
    bool IgnoreGlobalModifiers = false,
    bool InterruptDoAfters = true);

/// <summary>
/// Event raised when a hitbox has been spawned.
/// </summary>
[ByRefEvent]
public record struct PlayerHitboxAttackEvent(EntityUid User, EntityUid Hitbox);

/// <summary>
/// Raised on the hitbox when initialized, so other components can respond to it.
/// </summary>
/// <param name="User">The user of the hitbox</param>
/// <param name="Weapon">The weapon of the hitbox</param>
/// <param name="ClickLocation">The click location</param>
[ByRefEvent]
public record struct HitboxInitializedEvent(EntityUid User, EntityUid Weapon, Vector2 ClickLocation);

/// <summary>
/// Raised on the hitbox when it has hit a target
/// </summary>
/// <param name="Target">The target that was hit</param>
[ByRefEvent]
public record struct HitboxHitEvent(EntityUid? User, EntityUid Target);

/// <summary>
/// Raised from server->client after a hitbox spawns.
/// </summary>
[Serializable, NetSerializable]
public sealed class PredictedHitboxAttackEvent : EntityEventArgs
{
    public NetEntity Hitbox;
}
