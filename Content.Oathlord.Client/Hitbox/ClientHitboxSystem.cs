// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Oathlord.Shared.Hitbox;
using Robust.Client.Physics;
using Robust.Client.Player;

namespace Content.Oathlord.Client.Hitbox;

/// <summary>
/// System that handles proper hitbox prediction.
/// </summary>
public sealed partial class ClientHitboxSystem : EntitySystem
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private PhysicsSystem _physics = default!;

    [SubscribeLocalEvent]
    private void OnUpdateIsPredicted(Entity<HitboxComponent> ent, ref UpdateIsPredictedEvent args)
    {
        args.IsPredicted = _player.LocalEntity == ent.Comp.User;
    }

    [SubscribeNetworkEvent]
    private void OnPredictedHitbox(PredictedHitboxAttackEvent args)
    {
        var hitbox = GetEntity(args.Hitbox);
        if (!hitbox.IsValid())
            return;

        _physics.UpdateIsPredicted(hitbox);
    }
}
