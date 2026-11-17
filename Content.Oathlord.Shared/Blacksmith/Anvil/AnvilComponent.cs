using Content.Oathlord.Shared.Blacksmith.Anvil.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Oathlord.Shared.Blacksmith.Anvil;

/// <summary>
/// Component used on entities (usually structures) to give them the ability to work on <see cref="MetalWorkableComponent"/> entities.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(AnvilSystem))]
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
    /// The hit numbers that this anvil support, for hitting workables.
    /// </summary>
    [DataField(required: true)]
    public List<int> Numbers = new();

    /// <summary>
    /// The amount that has been worked on the metals, towards the <see cref="SelectedRecipe"/>.
    /// Check <see cref="AnvilRecipePrototype"/> for more info.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int WorkDone;

    /// <summary>
    /// The index of the <see cref="AnvilRecipePrototype.Pattern"/> the player must meet.
    /// It increases with every successful hit, but resets if user fails the pattern.
    ///
    /// -1 means no pattern was set, or a recipe has not been yet selected
    /// </summary>
    [DataField, AutoNetworkedField]
    public int PatternIndex = -1;
}

/// <summary>
/// Sent when the user selects a recipe in the anvil UI
/// </summary>
/// <param name="recipe">The recipe that was selected</param>
[Serializable, NetSerializable]
public sealed class AnvilRecipeSelectedMessage(ProtoId<AnvilRecipePrototype> recipe) : BoundUserInterfaceMessage
{
    /// <summary>
    /// The recipe that was selected by the user
    /// </summary>
    public ProtoId<AnvilRecipePrototype> Recipe = recipe;
}

/// <summary>
/// Sent when the user clicks a hit number button the anvil UI
/// </summary>
/// <param name="number">The hit number</param>
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

/// <summary>
/// Raised before doing operations on the anvil, on the user, to check if they can operate on it.
/// </summary>
[ByRefEvent]
public record struct CanOperateAnvilAttemptEvent(bool Handled = false);

/// <summary>
/// Raised on the user when a hit has been done.
/// </summary>
[ByRefEvent]
public record struct HammerHitDoneEvent(EntityUid Anvil, EntityUid User);
