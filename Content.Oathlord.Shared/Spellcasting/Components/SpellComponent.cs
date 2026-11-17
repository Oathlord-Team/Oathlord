using Content.Oathlord.Shared.Spellcasting.Prototypes;
using Content.Oathlord.Shared.Spellcasting.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Spellcasting.Components;

/// <summary>
/// Component used on actions to mark them as a spell,
/// which then can be casted by entities with <see cref="SpellcasterComponent"/>
///
/// Spells are stored on the body in entities with <see cref="SpellsComponent"/>
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SpellcastingSystem))]
[AutoGenerateComponentState]
public sealed partial class SpellComponent : Component
{
    /// <summary>
    /// Whether this spell is active, or inactive on the user's spell container
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Active;

    /// <summary>
    /// What type of spell is this?
    /// If it's not set, any entity with <see cref="SpellcasterComponent"/> will be able to cast this spell
    /// </summary>
    [DataField]
    public List<ProtoId<SpellTypePrototype>> Types = new();
};
