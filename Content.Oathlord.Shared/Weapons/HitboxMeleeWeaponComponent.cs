using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Weapons;

/// <summary>
/// Component used on mêlée weapons that work with hitboxes
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HitboxMeleeWeaponComponent : Component
{
    [DataField]
    public EntProtoId Hitbox;
}
