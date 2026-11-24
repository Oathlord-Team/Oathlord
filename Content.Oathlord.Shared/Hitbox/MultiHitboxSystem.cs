namespace Content.Oathlord.Shared.Hitbox;

public sealed partial class MultiHitboxSystem : EntitySystem
{
    [Dependency] private HitboxSystem _hitbox = default!;

    [SubscribeLocalEvent]
    private void OnHitboxInitialized(Entity<MultiHitboxComponent> ent, ref HitboxInitializedEvent args)
    {
        var user = args.User;
        var weapon = args.Weapon;

        foreach (var multiHitbox in ent.Comp.Hitboxes)
        {
            // Do not run the initialize event when spawning children hitboxes.
            // It's gonna lead to a stack overflow... Wait, say that again
            _hitbox.SpawnHitbox(user, weapon, args.ClickLocation, multiHitbox.HitboxProto, multiHitbox.Offset, init: false);
        }
    }
}
