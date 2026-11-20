// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Systems;
using Robust.Shared.Physics.Events;

namespace Content.Oathlord.Shared.Hitbox;

public sealed partial class HitboxSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    [Dependency] private EntityQuery<HitboxComponent> _hitboxQuery = default!;

    [SubscribeLocalEvent]
    private void StartCollide(Entity<HitboxComponent> ent, ref StartCollideEvent args)
    {
        var user = ent.Comp.User;
        var target = args.OtherEntity;

        if (target == user)
            return;

        // Hitboxes can re-trigger collision, like when moving, so disallow this from damaging targets again.
        if (ent.Comp.HitTargets.Contains(target))
            return;

        _damage.ChangeDamage(target, ent.Comp.Damage);

        ent.Comp.HitTargets.Add(target);
        DirtyField(ent.AsNullable(), nameof(HitboxComponent.HitTargets));
    }
}
