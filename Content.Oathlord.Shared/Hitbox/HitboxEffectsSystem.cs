using Content.Shared.EntityEffects;

namespace Content.Oathlord.Shared.Hitbox;

public sealed partial class HitboxEffectsSystem : EntitySystem
{
    [Dependency] private SharedEntityEffectsSystem _effects = default!;

    [SubscribeLocalEvent]
    private void OnHitboxHit(Entity<HitboxEffectsComponent> ent, ref HitboxHitEvent args)
    {
        var user = args.User;
        var target = args.Target;

        foreach (var (type, effects) in ent.Comp.HitEffects)
        {
            RunEffects(
                type: type,
                effects: effects,
                user: user,
                hitbox: ent,
                target: target);
        }
    }

    private void RunEffects(HitboxEffect type, EntityEffect[] effects, EntityUid? user, EntityUid hitbox, EntityUid target)
    {
        switch (type)
        {
            case HitboxEffect.Hitbox:
            {
                _effects.TryApplyEffects(hitbox, effects);
                break;
            }
            case HitboxEffect.Target:
            {
                _effects.TryApplyEffects(target, effects);
                break;
            }
            case HitboxEffect.User:
            {
                if (user is not { } usr)
                    break;

                _effects.TryApplyEffects(usr, effects);
                break;
            }
        }
    }
}
