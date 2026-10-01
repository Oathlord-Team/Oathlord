using Content.Oathlord.Shared.Movement;
using Robust.Client.Player;

namespace Content.Oathlord.Client.Movement;

public sealed partial class ClientScrollingMovementSystem : ScrollingMovementSystem
{
    [Dependency] private IPlayerManager _player = default!;

    public event EventHandler<float>? OnUpdateMovement;

    [SubscribeLocalEvent]
    public void OnHandleState(Entity<ScrollingMovementComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (_player.LocalEntity == ent)
            OnUpdateMovement?.Invoke(this, ent.Comp.Current);
    }
}
