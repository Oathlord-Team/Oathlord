using Robust.Shared.GameStates;

namespace Content.Oathlord.Shared.Blacksmith.Anvil;

/// <summary>
/// Component used on materials to make them valid to be used on <see cref="AnvilComponent"/> entities.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MetalWorkableComponent : Component;
