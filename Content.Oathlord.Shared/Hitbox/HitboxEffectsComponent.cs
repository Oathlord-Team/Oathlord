using Content.Shared.EntityEffects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Oathlord.Shared.Hitbox;

/// <summary>
/// Component that runs effects on the target
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HitboxEffectsComponent : Component
{
    /// <summary>
    /// Effects to run when the hitbox hits something
    /// </summary>
    [DataField(required: true)]
    public Dictionary<HitboxEffect, EntityEffect[]> HitEffects = new();
}

[Serializable, NetSerializable]
public enum HitboxEffect : byte
{
    User,
    Target,
    Hitbox,
}
