using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Oathlord.Shared.Mana;

/// <summary>
/// Component that gives an entity the ability to "have" mana,
/// but in order to use it in things such as magic, they'll need <see cref="CanUse"/> variable set to true.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true, fieldDeltas: true)]
public sealed partial class ManaUserComponent : Component
{
    /// <summary>
    /// Whether this entity can use mana
    /// This value, usually, can change throughout the course of the round
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CanUse = true;

    /// <summary>
    /// The maximum amount of mana this entity can have
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 MaxMana = 100.0f;

    /// <summary>
    /// The current amount of mana this entity has
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 CurrentMana;
}
