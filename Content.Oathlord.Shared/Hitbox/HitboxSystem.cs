// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Effects;
using Content.Shared.FixedPoint;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Hitbox;

public sealed partial class HitboxSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedColorFlashEffectSystem _color = default!;
    [Dependency] private MeleeSoundSystem _meleeSound = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;

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
        TryDoHit(ent, args.OtherEntity);
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
