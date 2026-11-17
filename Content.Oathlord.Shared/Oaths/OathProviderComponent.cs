using Content.Oathlord.Common.Oaths;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Oaths;

/// <summary>
/// Component used on entities that can change the oath on another entity when interacted with via a verb.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class OathProviderComponent : Component
{
    /// <summary>
    /// The oath this entity provides
    /// </summary>
    [DataField(required: true)]
    public ProtoId<OathPrototype> Oath;
}
