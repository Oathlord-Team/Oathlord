using Content.Shared.EntityEffects;
using Robust.Shared.GameStates;

namespace Content.Oathlord.Shared.Blacksmith.Hammer;

/// <summary>
/// Component used on held entities that allow the user to work on anvils.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HammerComponent : Component
{
    /// <summary>
    /// Effects that will run against the anvil once a hit is done
    /// </summary>
    [DataField(required: true)]
    public EntityEffect[] HitEffects = default!;
}
