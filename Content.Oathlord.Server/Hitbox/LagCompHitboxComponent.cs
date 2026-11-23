using Robust.Shared.Player;

namespace Content.Oathlord.Server.Hitbox;

/// <summary>
/// Component that is automatically applied to the hitbox when the user spawns one by initiating an attack.
///
/// It helps against lag.
/// </summary>
[RegisterComponent]
public sealed partial class LagCompHitboxComponent : Component
{
    /// <summary>
    /// The session of the user.
    /// </summary>
    [ViewVariables]
    public ICommonSession? Session;

    /// <summary>
    /// The entity of the attacker
    /// </summary>
    [DataField]
    public EntityUid Attacker;
}
