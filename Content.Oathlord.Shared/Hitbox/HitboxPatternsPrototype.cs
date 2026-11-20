// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Oathlord.Shared.Hitbox;

/// <summary>
/// Prototype that holds hitbox patterns.
/// </summary>
[Prototype]
public sealed partial class HitboxPatternsPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public Dictionary<AttackType, HitboxPattern[]> Patterns = new();
}

/// <summary>
/// A record that holds data about a hitbox pattern
/// </summary>
/// <param name="HitboxProto">The hitbox prototype</param>
/// <param name="Time">How long to wait until the next attack</param>
/// <param name="Grace">Added to <see cref="Time"/>, that's when the next attack can be commited</param>
/// <param name="Offset">The positional offset of the spawned hitbox</param>
[DataRecord]
public partial record struct HitboxPattern(
    EntProtoId<HitboxComponent> HitboxProto,
    TimeSpan Time,
    TimeSpan Grace,
    Vector2 Offset);

/// <summary>
/// Enum that defines an attack type.
/// </summary>
[Serializable, NetSerializable]
public enum AttackType : byte
{
    None,  // There's no attack currently
    Light, // Left-click
    Heavy, // Right-click
}
