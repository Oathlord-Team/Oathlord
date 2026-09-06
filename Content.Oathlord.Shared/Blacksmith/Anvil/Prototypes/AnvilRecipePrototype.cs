using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Oathlord.Shared.Blacksmith.Anvil.Prototypes;

/// <summary>
/// Prototype used for recipes of <see cref="MetalWorkableComponent"/> entities.
/// </summary>
[Prototype]
public sealed partial class AnvilRecipePrototype : IPrototype, IInheritingPrototype
{
    [IdDataField]
    public string ID { get; private set;  } = default!;

    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<AnvilRecipePrototype>))]
    public string[]? Parents { get; private set; }

    [AbstractDataField]
    public bool Abstract { get; private set; }

    /// <summary>
    /// Metals needed for this recipe
    /// </summary>
    [DataField(required: true)]
    [AlwaysPushInheritance]
    public List<EntProtoId> Metals = new();

    /// <summary>
    /// How much work it is required for this recipe.
    /// This should be a number between 0 and 100
    /// </summary>
    [DataField]
    public int WorkRequired = 50;

    /// <summary>
    /// What will result from this recipe
    /// </summary>
    [DataField]
    public EntProtoId Result;
}
