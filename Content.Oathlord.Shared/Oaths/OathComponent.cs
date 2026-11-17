using Content.Oathlord.Common.Oaths;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Oaths;

/// <summary>
/// Component used on the brain to define what Oath the player is aligned to.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class OathComponent : Component
{
    /// <summary>
    /// The oath this entity belongs to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<OathPrototype> Oath = "Oathless";
}
