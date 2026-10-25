using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Oathlord.Shared.CharacterAttributes;

/// <summary>
/// Handles all interactions with character attributes
/// </summary>
public sealed partial class CharacterAttributesSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedContainerSystem _container = default!;

    [Dependency] private EntityQuery<CharacterAttributesComponent> _characterAttrQuery = default!;
    [Dependency] private EntityQuery<CharacterAttributeComponent> _attrQuery = default!;

    /// <summary>
    /// All attributes that are loaded in the game
    /// </summary>
    public List<EntProtoId> AllAttributes = new();

    public override void Initialize()
    {
        base.Initialize();

        LoadAttributes();
        InitializeRelay();
    }

    [SubscribeLocalEvent]
    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<EntityPrototype>())
            return;

        LoadAttributes();
    }

    [SubscribeLocalEvent]
    private void OnCompInit(Entity<CharacterAttributesComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Container = _container.EnsureContainer<Container>(ent, CharacterAttributesComponent.ContainerId);

        // Everyone gets all attributes
        var charEnt = ent.AsNullable();
        foreach (var attribute in AllAttributes)
        {
            AddAttribute(charEnt, attribute);
        }
    }

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<CharacterAttributesComponent> ent, ref ComponentShutdown args)
    {
        if (_timing.ApplyingState && ent.Comp.NetSyncEnabled)
            return;

        _container.ShutdownContainer(ent.Comp.Container);
    }

    [EventSubscription]
    private void OnApplyAttributes(ApplyCharacterAttributesEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } player)
            return;

        var entityAttributes = new Dictionary<EntityUid, int>();
        foreach (var (attribute, points) in msg.ChangedPoints)
        {
            var entityAttr = GetEntity(attribute);
            entityAttributes.Add(entityAttr, points);
        }

        AdjustAttributes(player, entityAttributes);
    }

    private void LoadAttributes()
    {
        AllAttributes.Clear();
        var name = Factory.CompName<CharacterAttributeComponent>();
        foreach (var proto in ProtoMan.EnumeratePrototypes<EntityPrototype>())
        {
            if (!proto.HasComp(name) || proto.Abstract)
                continue;

            AllAttributes.Add(proto.ID);
        }
    }
}
