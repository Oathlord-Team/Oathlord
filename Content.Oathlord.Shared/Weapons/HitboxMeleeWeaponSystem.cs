// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Oathlord.Shared.Hitbox;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Oathlord.Shared.Weapons;

public sealed partial class HitboxMeleeWeaponSystem : EntitySystem
{
    /*
        Did you know that you don't need update loops for everything?
        That's something I learned from SharedChargesSystem, and it makes sense.

        Although, I can't deny that you're gonna put your sanity at risk by doing that.
    */

    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private HitboxSystem _hitbox = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    [Dependency] private EntityQuery<HitboxMeleeWeaponComponent> _hitboxMeleeQuery = default!;

    [SubscribeLocalEvent]
    private void OnMeleeHit(Entity<HitboxMeleeWeaponComponent> ent, ref MeleeHitEvent args)
    {
        // Leave it to us, bud
        args.Handled = true;

        var user = args.User;
        var weapon = args.Weapon;

        var attackType = args.Direction == null ? AttackType.Light : AttackType.Heavy;
        var patterns = _hitbox.GetHitboxPatterns(ent.Comp.WeaponPattern, attackType);
        if (patterns.Length == 0)
        {
            Log.Warning($"Found attack pattern {attackType.ToString()} with 0 elements by weapon: {ToPrettyString(ent)}");
            return;
        }

        var clickCoords = GetCoordinates(args.ClickLocation);
        AdvancePattern(ent, user, weapon, clickCoords, patterns, attackType);
    }

    /// <summary>
    /// Starts a new pattern
    /// </summary>
    private void StartPattern(Entity<HitboxMeleeWeaponComponent> ent, EntityUid user, EntityUid weapon, EntityCoordinates clickLoc, HitboxPattern[] patterns, AttackType type)
    {
        ent.Comp.CurrentPattern++;
        ent.Comp.NextAttack = type;
        Dirty(ent);

        // If there's no pattern, or we're finished, reset to the original state
        if (GetPattern(ent.AsNullable(), patterns) is not { } pattern)
        {
            ResetAndStartPattern(ent, user, weapon, clickLoc, patterns, type);
            return;
        }

        SpawnHitbox(user, weapon, pattern, clickLoc);

        var now = _timing.CurTime;
        ent.Comp.NextPattern = pattern.Time + now;

        Dirty(ent);
    }

    /// <summary>
    /// Resets the melee hitbox weapon to its default state
    /// </summary>
    private void ResetPattern(Entity<HitboxMeleeWeaponComponent> ent)
    {
        ent.Comp.CurrentPattern = -1;
        ent.Comp.NextAttack = AttackType.None;
        ent.Comp.NextPattern = TimeSpan.Zero;
        Dirty(ent);
    }

    /// <summary>
    /// Advances the current pattern on the melee hitbox weapon
    /// </summary>
    private void AdvancePattern(Entity<HitboxMeleeWeaponComponent> ent, EntityUid user, EntityUid weapon, EntityCoordinates clickLoc, HitboxPattern[] patterns, AttackType type)
    {
        if (!HasActivePattern(ent.AsNullable()))
        {
            // There's no pattern active! Start a new one.
            StartPattern(ent, user, weapon, clickLoc, patterns, type);
            return;
        }

        // okay, there IS a pattern active, we should probably check if we can start the next attack first
        if (GetNextPattern(ent.AsNullable(), patterns) is not { } pattern)
        {
            // no attack available for current pattern.
            ResetAndStartPattern(ent, user, weapon, clickLoc, patterns, type);
            return;
        }

        var now = _timing.CurTime;

        // We can add the pattenr only within the next pattern minus the grace period,
        // so the player can select the next attack before it begins...
        if (now > ent.Comp.NextPattern - pattern.Grace)
        {
            if (ent.Comp.NextAttack != type)
            {
                ResetPattern(ent); // no, you can't abuse this (this will bite me later)
                return;
            }

            ent.Comp.NextAttack = type;
        }

        // We can attack between the next pattern and grace period
        if (now > ent.Comp.NextPattern && now <= ent.Comp.NextPattern + pattern.Grace)
        {
            SpawnHitbox(user, weapon, pattern, clickLoc);
            return;
        }

        ResetAndStartPattern(ent, user, weapon, clickLoc, patterns, type); // wah, missed attack, just reset
    }

    private void SpawnHitbox(
        EntityUid user,
        EntityUid weapon,
        HitboxPattern pattern,
        EntityCoordinates coordinates)
    {
        var clickPos = _transform.ToMapCoordinates(coordinates).Position;
        _hitbox.SpawnHitbox(user, weapon, clickPos, pattern.HitboxProto, pattern.Offset);
    }

    private void ResetAndStartPattern(Entity<HitboxMeleeWeaponComponent> ent,
        EntityUid user,
        EntityUid weapon,
        EntityCoordinates clickLoc,
        HitboxPattern[] patterns,
        AttackType type)
    {
        ResetPattern(ent);
        StartPattern(ent, user, weapon, clickLoc, patterns, type);
    }

    public bool HasActivePattern(Entity<HitboxMeleeWeaponComponent?> ent)
    {
        if (!_hitboxMeleeQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        return ent.Comp.CurrentPattern != -1;
    }

    public HitboxPattern? GetPattern(Entity<HitboxMeleeWeaponComponent?> ent, HitboxPattern[] patterns)
    {
        if (!_hitboxMeleeQuery.Resolve(ent.Owner, ref ent.Comp) || ent.Comp.CurrentPattern == -1)
            return null;

        return ent.Comp.CurrentPattern >= patterns.Length ? null : patterns[ent.Comp.CurrentPattern];
    }

    public HitboxPattern? GetNextPattern(Entity<HitboxMeleeWeaponComponent?> ent, HitboxPattern[] patterns)
    {
        if (!_hitboxMeleeQuery.Resolve(ent.Owner, ref ent.Comp) || ent.Comp.CurrentPattern == -1)
            return null;

        var next = ent.Comp.CurrentPattern + 1;
        return next >= patterns.Length ? null : patterns[next];
    }
}
