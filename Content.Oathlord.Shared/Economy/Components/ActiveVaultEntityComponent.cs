// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Oathlord.Shared.Economy.Components;

/// <summary>
/// Active component put on entities that enter a vault tile, in order to prevent duplicate economy changes (if an entity collides with 2, or more vault tiles).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ActiveVaultEntityComponent : Component;
