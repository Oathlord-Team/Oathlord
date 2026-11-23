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

    [SubscribeLocalEvent]
    private void OnUpdateIsPredicted(Entity<HitboxComponent> ent, ref UpdateIsPredictedEvent args)
    {
        args.IsPredicted = _player.LocalEntity == ent.Comp.User;
    }
}
