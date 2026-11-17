using Content.Oathlord.Shared.Blacksmith.Anvil;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;

namespace Content.Oathlord.Shared.Hands;

public sealed partial class OathlordHandsSystem : EntitySystem
{
    [Dependency] private SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HandsComponent, CanOperateAnvilAttemptEvent>(_hands.RefRelayEvent);
        SubscribeLocalEvent<HandsComponent, HammerHitDoneEvent>(_hands.RefRelayEvent);
    }
}
