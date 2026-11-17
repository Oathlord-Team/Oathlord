using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Economy.Prototypes;

/// <summary>
/// This is a prototype for Oathlord's main currency to be used in economy.
/// </summary>
[Prototype]
public sealed partial class EconomyCurrencyPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The name of the currency
    /// </summary>
    [DataField(required: true)]
    public string Name = string.Empty;

    /// <summary>
    /// How much value this currency has, in the lowest form possible. E.g. gold has 1000 value
    /// </summary>
    [DataField(required: true)]
    public int Value;

    /// <summary>
    /// The "physical" version of this currency
    /// </summary>
    [DataField(required: true)]
    public EntProtoId EntityProto;
}
