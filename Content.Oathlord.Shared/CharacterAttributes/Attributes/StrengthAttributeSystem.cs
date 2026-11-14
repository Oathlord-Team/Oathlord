using Content.Shared.Weapons.Melee.Events;

namespace Content.Oathlord.Shared.CharacterAttributes.Attributes;

public sealed partial class StrengthAttributeSystem : EntitySystem
{
    [Dependency] private CharacterAttributesSystem _attributes = default!;

    [SubscribeLocalEvent]
    private void OnGetDamage(Entity<StrengthAttributeComponent> ent, ref CharAttributeRelayedEvent<GetMeleeDamageEvent> args)
    {
        var arguments = args.Args;
        var points = _attributes.GetAttributePoints(ent.Owner) + 1;

        // We want weapons to have lower damage than bare hands, but still gain increased damage.
        var multiplier = arguments.User == arguments.Weapon
            ? points - ( ent.Comp.DamageFalloffBare * points )
            : points - ( ent.Comp.DamageFalloffWeapon * points );

        var extraDmg = arguments.Damage * multiplier;
        arguments.Damage += extraDmg;
        args.Args = arguments;
    }
}
