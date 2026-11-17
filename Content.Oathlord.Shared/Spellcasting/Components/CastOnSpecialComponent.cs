using Robust.Shared.GameStates;

namespace Content.Oathlord.Shared.Spellcasting.Components;

/// <summary>
/// Component that casts the active selected spell when an item does an item special interaction
/// The item must have <see cref="SpellcasterComponent"/> for this to work.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CastOnSpecialComponent : Component;
