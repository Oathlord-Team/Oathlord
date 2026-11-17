using Content.Shared.Body;
using Content.Shared.EntityEffects;
using Content.Shared.Whitelist;

namespace Content.Oathlord.Shared.EntityEffects;

/// <summary>
/// Effect that relays an effect to internal organs on the body.
/// </summary>
public sealed partial class RelayOrgans : EntityEffectBase<RelayOrgans>
{
    /// <summary>
    /// A whitelist that checks against the found organs
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// The effect to run on the organs
    /// </summary>
    [DataField(required: true)]
    public EntityEffect Effect = default!;
}

public sealed partial class RelayOrgansEffectSystem : EntityEffectSystem<BodyComponent, RelayOrgans>
{
    [Dependency] private BodySystem _body = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private SharedEntityEffectsSystem _effects = default!;

    [Dependency] private EntityQuery<InternalChildOrganComponent> _internalOrganQuery = default!;

    protected override void Effect(Entity<BodyComponent> ent, ref EntityEffectEvent<RelayOrgans> args)
    {
        var effect = args.Effect.Effect;
        var whitelist = args.Effect.Whitelist;
        foreach (var organ in _body.EnumerateOrgans(ent.AsNullable(), _internalOrganQuery))
        {
            if (_whitelist.IsWhitelistFail(whitelist, organ))
                continue;

            _effects.TryApplyEffect(organ, effect, args.Scale, args.User);
        }
    }
}
