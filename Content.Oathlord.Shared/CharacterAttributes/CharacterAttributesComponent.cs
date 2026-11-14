using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Oathlord.Shared.CharacterAttributes;

/// <summary>
/// Component that holds the attributes of the player
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class CharacterAttributesComponent : Component
{
    public const string ContainerId = "character_attributes";

    [ViewVariables]
    public Container Container = default!;

    /// <summary>
    /// The points you can put towards an attribute of your choice.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Points;
}
