using Robust.Shared.GameStates;

namespace Content.Oathlord.Shared.CharacterAttributes;

/// <summary>
/// Component that defines an attribute
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class CharacterAttributeComponent : Component
{
    /// <summary>
    /// How many points are allocated at this attribute
    /// </summary>
    [DataField, AutoNetworkedField]
    public int CurrentPoints;
}
