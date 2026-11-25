using System.Linq;
using Content.IntegrationTests.Fixtures.Attributes;
using Content.IntegrationTests.Tests.Interaction;
using Content.Oathlord.Shared.Hitbox;
using Content.Oathlord.Shared.Weapons;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Oathlord;

public sealed partial class HitboxTest : InteractionTest
{
    [SidedDependency(Side.Server)] private HitboxMeleeWeaponSystem _hitboxMelee = default!;
    [SidedDependency(Side.Server)] private HitboxSystem _hitbox = default!;

    protected override string PlayerPrototype => "MobHuman";

    [Test]
    [RunOnSide(Side.Server)] // we don't need client for this
    [Description("Checks that all hitbox patterns, per attack type, work as intended. Goes through all registered pattern prototypes, and advances them to make sure everything works.")]
    public async Task HitboxPatternsTest()
    {
        var patterns = SProtoMan.EnumeratePrototypes<HitboxPatternsPrototype>().ToList();

        var hitboxMelee = SEntMan.AddComponent<HitboxMeleeWeaponComponent>(SPlayer);

        var melee = (SPlayer, hitboxMelee);
        var pos = Position(SPlayer);

        foreach (var hitboxPattern in patterns)
        {
            TestPatterns(hitboxPattern, AttackType.Light, melee, pos);
            TestPatterns(hitboxPattern, AttackType.Heavy, melee, pos);
        }
    }

    private void TestPatterns(ProtoId<HitboxPatternsPrototype> pattern, AttackType type, Entity<HitboxMeleeWeaponComponent> melee, EntityCoordinates pos)
    {
        // Set the pattern so we can an attack to do
        melee.Comp.WeaponPattern = pattern;

        var patternArray = _hitbox.GetHitboxPatterns(pattern, type); // this is cached so its fine and im lazy

        for (var i = 0; i < patternArray.Length; i++)
        {
            var grace = patternArray[i].Grace;
            var time = patternArray[i].Time;

            Assert.That(grace, Is.GreaterThan(TimeSpan.Zero), $"Hitbox grace time must be higher than 0");
            Assert.That(time, Is.GreaterThan(TimeSpan.Zero), "Hitbox time must be higher than 0");

            melee.Comp.NextPattern = SGameTiming.CurTime - grace;
            _hitboxMelee.AdvancePattern(melee, SPlayer, SPlayer, pos, patternArray, type);
        }

        Assert.That(melee.Comp.CurrentPattern, Is.EqualTo(patternArray.Length));
        _hitboxMelee.ResetPattern(melee.AsNullable()); // always reset, so next pattern starts correctly
    }
}
