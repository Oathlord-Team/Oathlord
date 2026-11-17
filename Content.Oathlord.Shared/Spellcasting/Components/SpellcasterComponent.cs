using Content.Oathlord.Shared.Spellcasting.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Spellcasting.Components;

/// <summary>
/// Component for items (usually) that can cast spells
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpellcasterComponent : Component
{
    /// <summary>
    /// What types of spells this entity can cast
    /// </summary>
    [DataField]
    public List<ProtoId<SpellTypePrototype>> Types = new();
}
