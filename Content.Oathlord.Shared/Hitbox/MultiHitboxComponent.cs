// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Hitbox;

/// <summary>
/// Component that allows a hitbox to have multiple sub-hitboxes.
/// Note: the sub-hitboxes won't spawn sub-hitboxes...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MultiHitboxComponent : Component
{
    /// <summary>
    /// An array of hitboxes to spawn alongside this entity.
    /// </summary>
    [DataField(required: true)]
    public MultiHitboxData[] Hitboxes;
}

/// <summary>
/// Record struct that holds simple data about a hitbox prototype and an offset
/// </summary>
/// <param name="HitboxProto">The hitbox</param>
/// <param name="Offset">The positional offset</param>
[DataRecord]
public partial record struct MultiHitboxData(EntProtoId<HitboxComponent> HitboxProto, Vector2 Offset);
