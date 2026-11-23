using Content.Oathlord.Common.CCVar;
using Content.Oathlord.Shared.Hitbox;
using Content.Server.Movement.Components;
using Content.Server.Movement.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Oathlord.Server.Hitbox;

/// <summary>
/// System that handles lag-compensation of hitboxes.
///
/// It basically runs an update loop on all hitboxes with <see cref="LagCompHitboxComponent"/>,
/// and runs a lookup against an enlarged fixture, based on the hitbox's fixture shape, in order to see if any
/// targets intersect with the original fixture, based on their lag compensated position.
///
/// This ensures high-ping gameplay is not hell!
/// </summary>
public sealed partial class ServerHitboxSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private HitboxSystem _hitbox = default!;
    [Dependency] private LagCompensationSystem _lag = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;

    [Dependency] private EntityQuery<ActorComponent> _actorQuery = default!;

    /// <summary>
    /// We obv. only care for entities that we can get the lag coordinates from...
    /// </summary>
    private HashSet<Entity<LagCompensationComponent>> _lagEntities = new();

    private float _hitboxLagRange;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, OathlordCVars.HitboxLagRange, x => _hitboxLagRange = x, true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<LagCompHitboxComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var transform))
        {
            if (comp.Session is not { } session)
                continue;

            var aabb = _lookup.GetWorldAABB(uid, transform);
            var search = aabb.Enlarged(_hitboxLagRange);

            _lagEntities.Clear();
            _lookup.GetEntitiesIntersecting(transform.MapID, search, _lagEntities);
            foreach (var target in _lagEntities)
            {
                if (target.Owner == comp.Attacker)
                    continue;

                var current = _transform.GetWorldPosition(target);
                var lagged = _transform.ToMapCoordinates(_lag.GetCoordinates(target, session)).Position;

                // Compare the target's moved aabb (from his current position in the server, and the rewinded point),
                // so we can have accurate intersections of the moved target's Box2
                var targetAabb = _lookup.GetWorldAABB(target).Translated(lagged - current);
                if (!aabb.Intersects(targetAabb))
                    continue;

                _hitbox.TryDoHit(uid, target);
            }
        }
    }

    [SubscribeLocalEvent]
    private void OnHitboxAttack(ref PlayerHitboxAttackEvent args)
    {
        // NPCs and other non-player controlled entities should be out of this.
        if (!_actorQuery.TryComp(args.User, out var actor))
            return;

        var lag = EnsureComp<LagCompHitboxComponent>(args.Hitbox);
        lag.Attacker = args.User;
        lag.Session = actor.PlayerSession;
    }
}
