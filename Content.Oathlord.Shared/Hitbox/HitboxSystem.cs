// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Effects;
using Content.Shared.FixedPoint;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Hitbox;

public sealed partial class HitboxSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedColorFlashEffectSystem _color = default!;
    [Dependency] private MeleeSoundSystem _meleeSound = default!;

    [Dependency] private EntityQuery<HitboxComponent> _hitboxQuery = default!;
    [Dependency] private EntityQuery<MeleeWeaponComponent> _meleeQuery = default!;

    /// <summary>
    /// A dictionary containing <see cref="HitboxPatternsPrototype"/> as keys, and their patterns as nested dictionary value, for quick-lookups
    /// </summary>
    private Dictionary<ProtoId<HitboxPatternsPrototype>, Dictionary<AttackType, HitboxPattern[]>> _cachedPatterns = new();

    public override void Initialize()
    {
        base.Initialize();

        LoadPatterns();
    }

    [SubscribeLocalEvent]
    private void OnProtoReload(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<HitboxPatternsPrototype>())
            return;

        LoadPatterns();
    }

    [SubscribeLocalEvent]
    private void StartCollide(Entity<HitboxComponent> ent, ref StartCollideEvent args)
    {
        var user = ent.Comp.User;
        var target = args.OtherEntity;
        var weapon = ent.Comp.Weapon;
        var damageConfig = ent.Comp.DamageConfig;

        if (target == user)
            return;

        // Hitboxes can re-trigger collision, like when moving, so disallow this from damaging targets again.
        if (ent.Comp.HitTargets.Contains(target) && !ent.Comp.MultiHit)
            return;

        // We call the same events from melee system so that we don't lose basic functionality
        // todo: support melee hit bonus damage...?
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

    #region Helpers

    /// <summary>
    /// Returns the highest damage group in a damage specifier.
    /// </summary>
    /// <param name="modifiedDamage">The damage specified to check against</param>
    /// <remarks>Copy-pasted and fixed, from Wizden's implementation</remarks>
    private ProtoId<DamageGroupPrototype>? GetHighestDamageSound(DamageSpecifier modifiedDamage)
    {
        var groups = modifiedDamage.GetDamagePerGroup(ProtoMan);

        // Use group if it's exclusive, otherwise fall back to type.
        if (groups.Count == 1)
        {
            return groups.Keys.First();
        }

        var highestDamage = FixedPoint2.Zero;
        string? highestDamageType = null;

        foreach (var (type, damage) in modifiedDamage.DamageDict)
        {
            if (damage <= highestDamage)
                continue;

            highestDamage = damage;
            highestDamageType = type;
        }

        return highestDamageType;
    }

    private void LoadPatterns()
    {
        _cachedPatterns.Clear();
        foreach (var proto in ProtoMan.EnumeratePrototypes<HitboxPatternsPrototype>())
        {
            _cachedPatterns.Add(proto, proto.Patterns);
        }
    }

    #endregion
}
