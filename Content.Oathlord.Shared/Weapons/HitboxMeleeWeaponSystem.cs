// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Oathlord.Shared.Hitbox;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Oathlord.Shared.Weapons;

public sealed partial class HitboxMeleeWeaponSystem : EntitySystem
{
    [Dependency] private HitboxSystem _hitbox = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    [SubscribeLocalEvent]
    private void OnMeleeHit(Entity<HitboxMeleeWeaponComponent> ent, ref MeleeHitEvent args)
    {
        // Leave it to us, bud
        args.Handled = true;

        var clickCoords = GetCoordinates(args.ClickLocation);
        var clickPos = _transform.ToMapCoordinates(clickCoords).Position;

        _hitbox.SpawnHitbox(
            user: args.User,
            weapon: args.Weapon,
            clickLocation: clickPos,
            hitbox: ent.Comp.Hitbox);
    }
}
