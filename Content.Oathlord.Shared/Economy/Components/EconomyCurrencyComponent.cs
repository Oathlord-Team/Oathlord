using Content.Oathlord.Shared.Economy.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Economy.Components;

/// <summary>
/// Component used on anything that acts as currency, like coins (na, nar, nara).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EconomyCurrencyComponent : Component
{
    /// <summary>
    /// The currency prototype this entity represents
    /// </summary>
    [DataField(required: true)]
    public ProtoId<EconomyCurrencyPrototype> Currency;
}
