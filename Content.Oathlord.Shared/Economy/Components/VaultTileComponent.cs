// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Oathlord.Shared.Economy.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Economy.Components;

/// <summary>
/// Component used on entities (tiles mainly) to detect <see cref="EconomyValueComponent"/> entities
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class VaultTileComponent : Component
{
    /// <summary>
    /// A hashset of entities that are currently interacting with this vault tile.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> Stored = new();
}
