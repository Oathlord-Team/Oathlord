// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Oathlord.Shared.Hitbox;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
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
    [Dependency] private StatusEffectsSystem _status = default!;

    [Dependency] private EntityQuery<HitboxMeleeWeaponComponent> _hitboxMeleeQuery = default!;

    /// <summary>
    /// A status effect that restricts you from rotating during combat, in order for the hitbox to not move, since it's parented to the user.
    /// Hitbox must follow the user, but it can not go to other big positions since it will re-trigger hits and stuff like that.
    ///
    /// Oh, and also, restricts movement.
    /// </summary>
    public static readonly EntProtoId HitboxStatusEffect = "HitboxCombatStatusEffect";

    [SubscribeLocalEvent]
    private void OnMeleeHit(Entity<HitboxMeleeWeaponComponent> ent, ref MeleeHitEvent args)
    {
        // Always handle because oathlord's combat is different from upstream combat
        args.Handled = true;

        // if there's an active attack, do not waste resources on calculating logic, just return if we can't attack at that moment, so no spam
        if (ent.Comp.NextPattern != TimeSpan.Zero && _timing.CurTime <= ent.Comp.NextPattern)
            return;

        var clickCoords = GetCoordinates(args.ClickLocation);
        DoHitboxAttack(ent, args.User, args.Weapon, clickCoords, args.Direction);
    }

    /// <summary>
    /// Does a hitbox attack.
    /// </summary>
    private void DoHitboxAttack(
        Entity<HitboxMeleeWeaponComponent> ent,
        EntityUid user,
        EntityUid weapon,
        EntityCoordinates clickLoc,
        Vector2? direction)
    {
        // based on upstream combat, direction being null means it was a click-attack, otherwise it's a wideswing
        var attackType = direction == null ? AttackType.Light : AttackType.Heavy;

        var patterns = _hitbox.GetHitboxPatterns(ent.Comp.WeaponPattern, attackType);
        if (patterns.Length == 0)
        {
            Log.Warning($"Found attack pattern {attackType.ToString()} with 0 elements by weapon: {ToPrettyString(ent)}");
            return;
        }

        AdvancePattern(ent, user, weapon, clickLoc, patterns, attackType);
    }

    /// <summary>
    /// Starts a new hitbox attack pattern.
    /// </summary>
    private void StartPattern(Entity<HitboxMeleeWeaponComponent> ent, EntityUid user, EntityUid weapon, EntityCoordinates clickLoc, HitboxPattern[] patterns, AttackType type)
    {
        Log.Info("Staring new pattern...");

        ent.Comp.CurrentPattern++;
        DirtyField(ent, ent.Comp, nameof(HitboxMeleeWeaponComponent.CurrentPattern));

        // If there's no pattern, or we're finished, reset to the original state
        if (GetPattern(ent.AsNullable(), patterns) is not { } pattern)
        {
            ResetAndStartPattern(ent, user, weapon, clickLoc, patterns, type);
            return;
        }

        SpawnHitbox(ent, user, weapon, clickLoc, pattern, type);
    }

    /// <summary>
    /// Advances the current pattern on the mêlée hitbox weapon
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
        if (GetPattern(ent.AsNullable(), patterns) is not { } pattern)
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
            if (ent.Comp.PrevAttack != type)
            {
                Log.Info("Commited different attacks, cancelling!");
                ResetPattern(ent);
                return;
            }
        }

        // We can attack between the next pattern and grace period
        if (now > ent.Comp.NextPattern && now <= ent.Comp.NextPattern + pattern.Grace)
        {
            Log.Info("You attack at the right moment!");
            SpawnHitbox(ent, user, weapon, clickLoc, pattern, type);
            return;
        }

        ResetAndStartPattern(ent, user, weapon, clickLoc, patterns, type); // Missed the attack?
    }

    /// <summary>
    /// Spawns a hitbox on the user, and handles setting up the next attack.
    /// </summary>
    private void SpawnHitbox(Entity<HitboxMeleeWeaponComponent> ent, EntityUid user, EntityUid weapon, EntityCoordinates clickLoc, HitboxPattern pattern, AttackType type)
    {
        var clickPos = _transform.ToMapCoordinates(clickLoc).Position;
        _hitbox.SpawnHitbox(user, weapon, clickPos, pattern.HitboxProto, pattern.Offset);

        var now = _timing.CurTime;
        ent.Comp.NextPattern = pattern.Time + now;

        ent.Comp.PrevAttack = ent.Comp.NextAttack == AttackType.None ? type : ent.Comp.NextAttack;
        ent.Comp.NextAttack = type;

        ent.Comp.CurrentPattern++;
        Dirty(ent);

        // The status effect is as long as the pattern attack.
        _status.TryAddStatusEffectDuration(user, HitboxStatusEffect, pattern.Time);

        Log.Info($"Pattern done, and advanced to {ent.Comp.CurrentPattern}!");
    }

    /// <summary>
    /// Resets the mêlée hitbox weapon to its default state.
    /// </summary>
    private void ResetPattern(Entity<HitboxMeleeWeaponComponent> ent)
    {
        ent.Comp.CurrentPattern = -1;
        ent.Comp.NextAttack = AttackType.None;
        ent.Comp.PrevAttack = AttackType.None;
        ent.Comp.NextPattern = TimeSpan.Zero;
        Dirty(ent);

        Log.Info("Current pattern was reset!");
    }

    /// <summary>
    /// Resets the component fields on the weapon, and starts a new pattern all-together
    /// </summary>
    private void ResetAndStartPattern(
        Entity<HitboxMeleeWeaponComponent> ent,
        EntityUid user,
        EntityUid weapon,
        EntityCoordinates clickLoc,
        HitboxPattern[] patterns,
        AttackType type)
    {
        ResetPattern(ent);
        StartPattern(ent, user, weapon, clickLoc, patterns, type);
    }

    /// <summary>
    /// Returns whether we have an active hitbox attack pattern on the weapon.
    /// </summary>
    public bool HasActivePattern(Entity<HitboxMeleeWeaponComponent?> ent)
    {
        if (!_hitboxMeleeQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        return ent.Comp.CurrentPattern != -1;
    }

    /// <summary>
    /// Returns the active hitbox attack pattern on the weapon
    /// </summary>
    /// <param name="ent">The weapon entity</param>
    /// <param name="patterns">An array of patterns, for quick lookup</param>
    /// <returns>The active hitbox pattern, or null if the pattern index is out of bounds</returns>
    public HitboxPattern? GetPattern(Entity<HitboxMeleeWeaponComponent?> ent, HitboxPattern[] patterns)
    {
        if (!_hitboxMeleeQuery.Resolve(ent.Owner, ref ent.Comp) || ent.Comp.CurrentPattern == -1)
            return null;

        var current = ent.Comp.CurrentPattern;
        return current >= patterns.Length ? null : patterns[current];
    }
}
