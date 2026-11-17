using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Spellcasting.Prototypes;

/// <summary>
/// Prototype that is used for Spells, to define their type.
/// This is used before casting an item, to check if the spellcasting item can cast the spell.
/// E.g. if a Staff can only cast X type, then it will fail to cast Y spell type
/// </summary>
[Prototype]
public sealed partial class SpellTypePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The name of the spell type, that will be displayed in the spell's tooltip
    /// </summary>
    [DataField(required: true)]
    public string Name = string.Empty;

    /// <summary>
    /// A small description of the spell type
    /// </summary>
    [DataField(required: true)]
    public string Description = string.Empty;
}
