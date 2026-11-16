using Robust.Shared.GameStates;

namespace Content.Oathlord.Shared.CharacterAttributes.Attributes;

/// <summary>
/// Attribute component that increases your spell active slots per points.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IntelligenceAttributeComponent : Component
{
    /// <summary>
    /// How many points are required to increase the active spell slots.
    /// Only 1 is added per increase.
    /// </summary>
    [DataField]
    public int ActiveIncrease = 3;

    /// <summary>
    /// How many points are required to increase the learned spell slots.
    /// </summary>
    [DataField]
    public int LearnedIncrease = 2;

    /// <summary>
    /// How many learned spell slots to add per <see cref="LearnedIncrease"/> in points.
    /// </summary>
    [DataField]
    public int LearnedAmount = 3;

    /// <summary>
    /// How many active spell slots to add per <see cref="LearnedIncrease"/> in points.
    /// </summary>
    [DataField]
    public int ActiveAmount = 1;
}
