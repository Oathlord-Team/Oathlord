// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Hitbox;

public sealed partial class HitboxSystem
{
    /// <summary>
    /// Sets data on the hitbox
    /// </summary>
    /// <param name="ent">The hitbox entity</param>
    /// <param name="user">The user who initiated this hitbox</param>
    /// <param name="weapon">The weapon used to initiate the hitbox</param>
    public void SetHitboxData(Entity<HitboxComponent?> ent, EntityUid user, EntityUid weapon)
    {
        if (!_hitboxQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.User = user;
        ent.Comp.Weapon = weapon;
        DirtyFields(ent, null, nameof(HitboxComponent.User), nameof(HitboxComponent.Weapon));
    }

    /// <summary>
    /// Returns the offset of the hitbox
    /// </summary>
    public Vector2 GetOffset(Entity<HitboxComponent?> ent)
    {
        if (!_hitboxQuery.Resolve(ent.Owner, ref ent.Comp))
            return Vector2.Zero;

        return ent.Comp.Offset;
    }

    /// <summary>
    /// Spawns a hitbox at the user's location
    /// </summary>
    /// <param name="user">The user who initiated the hitbox</param>
    /// <param name="weapon">The weapon used to initiate the hitbox</param>
    /// <param name="clickLocation">The click location of the attack, used to calculate angles</param>
    /// <param name="hitbox">The hitbox prototype to spawn</param>
    public void SpawnHitbox(EntityUid user, EntityUid weapon, Vector2 clickLocation, [ForbidLiteral] EntProtoId hitbox)
    {
        var xform = Transform(user);

        var hitboxEnt = PredictedSpawnAtPosition(hitbox, xform.Coordinates);
        SetHitboxData(hitboxEnt, user, weapon);

        var hitboxOffset = GetOffset(hitboxEnt);
        var hitboxXform = Transform(hitboxEnt);

        var worldRot = _transform.GetWorldRotation(xform);
        var coords = _transform.GetWorldPosition(xform);

        // We need both the direction of the clicked location, and the angle of the direction, in order to calculate the correct rotation of the hitbox
        // We need the click locations because meleeweapon system does not have "light" and "heavy" attacks in events,
        // (only way we know what type of attack it is, is via the direction parameter)
        var directionToClick = clickLocation - coords;
        var clickAngle = directionToClick == Vector2.Zero ? worldRot : directionToClick.ToWorldAngle();

        var rotatedOffset = hitboxOffset == Vector2.Zero ? Vector2.Zero : clickAngle.RotateVec(hitboxOffset);
        var worldPos = rotatedOffset + coords;

        _transform.SetWorldRotation(hitboxXform, clickAngle);
        _transform.SetWorldPosition(hitboxXform, worldPos);

        // It makes sense that the hitbox would NOT stay on air and follow the user, lol...?
        _transform.SetParent(hitboxEnt, user);
    }
}
