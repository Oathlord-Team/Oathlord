// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Oathlord.Shared.CharacterAttributes.Attributes;

/// <summary>
/// Attribute component that gives you mêlée damage.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StrengthAttributeComponent : Component
{
    /// <summary>
    /// This is multiplied for the extra damage, and only applies if we're using our barehands.
    /// The bare hands get more damage increase than weapons.
    /// </summary>
    [DataField]
    public float DamageFalloffBare = 0.7f;

    /// <summary>
    /// This is multiplied for the extra damage, and only applies if we're holding a weapon.
    /// </summary>
    [DataField]
    public float DamageFalloffWeapon = 0.5f;
}
