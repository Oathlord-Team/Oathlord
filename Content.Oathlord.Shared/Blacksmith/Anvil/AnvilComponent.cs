using Content.Oathlord.Shared.Blacksmith.Anvil.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Oathlord.Shared.Blacksmith.Anvil;

/// <summary>
/// Component used on entities (usually structures) to give them the ability to work on <see cref="MetalWorkableComponent"/> entities.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true, fieldDeltas: true)]
public sealed partial class AnvilComponent : Component
{
    /// <summary>
    /// How many workables we can have in this anvil at a time
    /// </summary>
    [DataField]
    public int AllowedWorkables = 2;

    /// <summary>
    /// The recipe that was selected to be worked on
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<AnvilRecipePrototype>? SelectedRecipe;

    /// <summary>
    /// The numbers that this anvil support, for hitting workables.
    /// </summary>
    [DataField(required: true)]
    public List<int> Numbers = new();

    /// <summary>
    /// The amount that has been worked on the metals, towards the <see cref="SelectedRecipe"/>.
    /// Check <see cref="AnvilRecipePrototype"/> for more info.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int WorkDone;
}

[Serializable, NetSerializable]
public sealed class AnvilRecipeSelectedMessage(ProtoId<AnvilRecipePrototype> recipe) : BoundUserInterfaceMessage
{
    /// <summary>
    /// The recipe that was selected by the user
    /// </summary>
    public ProtoId<AnvilRecipePrototype> Recipe = recipe;
}

[Serializable, NetSerializable]
public sealed class AnvilHitMessage(int number) : BoundUserInterfaceMessage
{
    /// <summary>
    /// The hit number that was clicked by the user
    /// </summary>
    public int Number = number;
}

[Serializable, NetSerializable]
public enum AnvilUiKey : byte
{
    Key,
}
