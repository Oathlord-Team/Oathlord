// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
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
    /// Spawns a hitbox at the user's location
    /// </summary>
    /// <param name="user">The user who initiated the hitbox</param>
    /// <param name="weapon">The weapon used to initiate the hitbox</param>
    /// <param name="clickLocation">The click location of the attack, used to calculate angles</param>
    /// <param name="hitbox">The hitbox prototype to spawn</param>
    /// <param name="offset">The offset of the hitbox</param>
    public void SpawnHitbox(EntityUid user, EntityUid weapon, Vector2 clickLocation, [ForbidLiteral] EntProtoId hitbox, Vector2 offset)
    {
        var xform = Transform(user);

        var hitboxEnt = PredictedSpawnAtPosition(hitbox, xform.Coordinates);
        SetHitboxData(hitboxEnt, user, weapon);

        // Required for proper prediction, for high-ping combat.
        var physics = EnsureComp<PhysicsComponent>(hitboxEnt);
        _physics.UpdateIsPredicted(hitboxEnt, physics);

        var ev = new PlayerHitboxAttackEvent(user, hitboxEnt);
        RaiseLocalEvent(ref ev);

        var hitboxXform = Transform(hitboxEnt);

        var worldRot = _transform.GetWorldRotation(xform);
        var coords = _transform.GetWorldPosition(xform);

        // We need both the direction of the clicked location, and the angle of the direction, in order to calculate the correct rotation of the hitbox
        // We need the click locations because meleeweapon system does not have "light" and "heavy" attacks in events,
        // (only way we know what type of attack it is, is via the direction parameter)
        var directionToClick = clickLocation - coords;
        var clickAngle = directionToClick == Vector2.Zero ? worldRot : directionToClick.ToWorldAngle();

        var rotatedOffset = offset == Vector2.Zero ? Vector2.Zero : clickAngle.RotateVec(offset);
        var worldPos = rotatedOffset + coords;

        _transform.SetWorldRotation(hitboxXform, clickAngle);
        _transform.SetWorldPosition(hitboxXform, worldPos);

        _transform.SetWorldRotation(user, clickAngle); // User should be facing the attack...

        // It makes sense that the hitbox would NOT stay on air and follow the user, lol...?
        _transform.SetParent(hitboxEnt, user);
    }

    /// <summary>
    /// Returns an array of hitbox patterns, of a <see cref="HitboxPatternsPrototype"/> prototype, based on the attack type provided.
    /// </summary>
    /// <param name="patternProto">The pattern prototype</param>
    /// <param name="type">The attack type</param>
    public HitboxPattern[] GetHitboxPatterns([ForbidLiteral] ProtoId<HitboxPatternsPrototype> patternProto, AttackType type)
    {
        if (!_cachedPatterns.TryGetValue(patternProto, out var patterns)
            || !patterns.TryGetValue(type, out var pattern))
            return [];

        return pattern;
    }

    /// <summary>
    /// Attempts to do a hit on the target.
    /// </summary>
    /// <param name="hitbox">The hitbox</param>
    /// <param name="target">The target to hit</param>
    public void TryDoHit(EntityUid hitbox, EntityUid target)
    {
        if (!_hitboxQuery.TryComp(hitbox, out var hitComp))
            return;

        TryDoHit((hitbox, hitComp), target);
    }

    /// <inheritdoc cref="TryDoHit(Robust.Shared.GameObjects.EntityUid,Robust.Shared.GameObjects.EntityUid)"/>
    public void TryDoHit(Entity<HitboxComponent> ent, EntityUid target)
    {
        var user = ent.Comp.User;
        var weapon = ent.Comp.Weapon;
        var damageConfig = ent.Comp.DamageConfig;

        if (user == target)
            return;

        if (ent.Comp.HitTargets.Contains(target) && !ent.Comp.MultiHit)
            return;

        var bonus = new DamageSpecifier();
        if (weapon is { } used && user is { } usr)
        {
            var ev = new AttackedEvent(used, usr, Transform(ent).Coordinates);
            RaiseLocalEvent(target, ev);

            bonus += ev.BonusDamage;
        }

        var damageSpef = damageConfig.Damage + bonus;
        if (!_damage.TryChangeDamage(
                ent: target,
                damage: damageSpef,
                ignoreResistances: damageConfig.IgnoreResistances,
                interruptsDoAfters: damageConfig.InterruptDoAfters,
                origin: ent,
                ignoreGlobalModifiers: damageConfig.IgnoreGlobalModifiers))
            return;

        // Do user feedback
        // Fun fact: Did you know that using `[target]` instead is a sandbox violation
        _color.RaiseEffect(Color.Red, new List<EntityUid> { target }, Filter.Pvs(target, entityManager: EntityManager));

        // Melee system may need a HUGE refactor from wizden, now that I'm thinking about it
        if (_meleeQuery.TryComp(weapon, out var weaponMelee))
            _meleeSound.PlayHitSound(target, user, GetHighestDamageSound(damageSpef), null, weaponMelee);

        ent.Comp.HitTargets.Add(target);
        DirtyField(ent.AsNullable(), nameof(HitboxComponent.HitTargets));
    }
}
