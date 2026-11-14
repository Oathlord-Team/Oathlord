using Content.Oathlord.Common.Input;
using Content.Oathlord.Shared.Spellcasting.Components;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Oathlord.Shared.Spellcasting.Systems;

/// <summary>
/// Handles anything related to spellcasting, and provides a public api
///
/// Spells are wrappers around actions (for flexibility purposes), but are bound by their own rules.
///
/// Some of the important ones being:
/// - You can have only 1 spell active at a time to use
/// - Spells usually require an entity with <see cref="SpellcasterComponent"/> in order to be casted (usually an item on the user's hand)
/// - You have a limited amount of spells you can switch to
/// - You have a limited amount of spells you can learn
///
/// The purpose of spells is to eliminate the management of tons of actions on the user's hotbar,
/// and instead make them be usable only within certain contexts.
/// </summary>
public abstract partial class SpellcastingSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SpellcasterSystem _spellcaster = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedActionsSystem _actions = default!;

    [Dependency] private EntityQuery<SpellsComponent> _spellsQuery = default!;
    [Dependency] private EntityQuery<SpellComponent> _spellQuery = default!;

    public override void Initialize()
    {
        base.Initialize();

        CommandBinds.Builder
            .Bind(OathlordKeyFunctions.MoveSpellDown, InputCmdHandler.FromDelegate(HandleMoveSpellDown, handle: false, outsidePrediction: false))
            .Bind(OathlordKeyFunctions.MoveSpellUp, InputCmdHandler.FromDelegate(HandleMoveSpellUp, handle: false, outsidePrediction: false))
            .Register<SpellcastingSystem>();

        InitializeSpells();
    }

    #region Command Binds

    private void HandleMoveSpellDown(ICommonSession? session)
    {
        MoveSelectedSpell(session, SpellMove.Down);
    }

    private void HandleMoveSpellUp(ICommonSession? session)
    {
        MoveSelectedSpell(session, SpellMove.Up);
    }

    #endregion

    #region Event Handlers

    [SubscribeLocalEvent]
    public void OnCompInit(Entity<SpellsComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Container = _container.EnsureContainer<Container>(ent, SpellsComponent.ContainerId);
    }

    [SubscribeLocalEvent]
    public void OnShutdown(Entity<SpellsComponent> ent, ref ComponentShutdown args)
    {
        if (_timing.ApplyingState && ent.Comp.NetSyncEnabled)
            return;

        _container.ShutdownContainer(ent.Comp.Container);
    }

    [SubscribeLocalEvent]
    public void OnContInserted(Entity<SpellsComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != SpellsComponent.ContainerId)
            return;

        UpdateUi(ent, refresh: true);
    }

    [SubscribeLocalEvent]
    public void OnContRemoved(Entity<SpellsComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != SpellsComponent.ContainerId)
            return;

        UpdateUi(ent, refresh: true);
    }

    [EventSubscription]
    public void OnTransfer(SpellTransferEvent msg, EntitySessionEventArgs args)
    {
        var attached = args.SenderSession.AttachedEntity;
        if (attached is not { } entity)
            return;

        var spell = GetEntity(msg.Spell);
        var transferTo = msg.Type;

        if (TryTransferSpell(entity, spell, transferTo))
            return;

        msg.Cancelled = true;
        _popup.PopupCursor($"You can not transfer this spell to the {transferTo.ToString().ToLower()} category", entity, PopupType.MediumCaution);
    }

    #endregion

    #region Public API

    /// <summary>
    /// Tries to transfer a spell from one category to another.
    /// </summary>
    /// <param name="ent">The entity</param>
    /// <param name="spell">The spell to transfer</param>
    /// <param name="spellTransferType">In which category to transfer to; either learned spells or active spells</param>
    /// <returns>False if the transferring was canceled, true otherwise</returns>
    public bool TryTransferSpell(Entity<SpellsComponent?> ent, EntityUid spell, SpellTransfer spellTransferType)
    {
        if (!_spellsQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        if (!_spellQuery.HasComp(spell) || !ent.Comp.Container.Contains(spell))
            return false;

        var activeSpells = GetSpells(ent, activeOnly: true);
        var learnedSpells = GetSpells(ent, activeOnly: false);

        switch (spellTransferType)
        {
            case SpellTransfer.Active:
            {
                if (activeSpells.Count >= ent.Comp.CurrentSlots)
                    return false;

                break;
            }
            case SpellTransfer.Learned:
            {
                if (learnedSpells.Count >= ent.Comp.MaxLearned)
                    return false;

                // so things don't get fucked up, always reset the index to the first spell
                ent.Comp.SelectedSpell = 0;
                Dirty(ent);
                break;
            }
        }

        TransferSpell(ent, spell, spellTransferType);
        return true;
    }

    /// <summary>
    /// Transfers a spell from one category to another
    /// </summary>
    /// <param name="ent">The entity</param>
    /// <param name="spell">The spell to transfer</param>
    /// <param name="spellTransferType">In which category to transfer to; either learned spells or active spells</param>
    public void TransferSpell(Entity<SpellsComponent?> ent, EntityUid spell, SpellTransfer spellTransferType)
    {
        if (!_spellsQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        SetActive(spell, active: spellTransferType == SpellTransfer.Active);
        UpdateUi((ent.Owner, ent.Comp));
    }

    /// <summary>
    /// Casts the active selected spell of the entity
    /// </summary>
    /// <param name="ent">The entity</param>
    /// <param name="used">What was used to cast this spell</param>
    /// <param name="target">The target of this spell, if null there was no target</param>
    /// <param name="coords">The click coordinates of the special interaction</param>
    public void CastSpell(Entity<SpellsComponent?> ent, EntityUid used, EntityUid? target, EntityCoordinates coords)
    {
        if (!_spellsQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        if (GetActiveSpell(ent) is not { } spellEntity
            || !_spellcaster.CanCast(used, spellEntity)
            || !IsValid(spellEntity))
        {
            _popup.PopupEntity("You fail to cast the spell!", ent, PopupType.MediumCaution);
            return;
        }

        // this method is pure hell due to actions being hardcoded, but is simple, small and works. I cbf to fix action ancientcode
        _actions.PerformSpellAction(ent, spellEntity, target, coords);
    }

    /// <summary>
    /// Gets the current selected active spell of the user
    /// </summary>
    public EntityUid? GetActiveSpell(Entity<SpellsComponent?> ent)
    {
        if (!_spellsQuery.Resolve(ent.Owner, ref ent.Comp))
            return null;

        var activeSpells = GetSpells(ent, activeOnly: true);
        var selectedSpell = ent.Comp.SelectedSpell;

        if (selectedSpell < 0 || selectedSpell >= activeSpells.Count)
            return null;

        var spell = activeSpells[selectedSpell];
        if (TerminatingOrDeleted(spell))
            return null;

        return spell;
    }

    /// <summary>
    /// Adds a spell to the entity's learned spells category
    /// </summary>
    /// <param name="ent">The entity to add the spell to</param>
    /// <param name="spell">The spell prototype to add</param>
    /// <param name="force">Whether to add the spell, without checking whether we already have it</param>
    /// <returns>The spell entity that was made, null if insertion failed</returns>
    public EntityUid? AddSpell(Entity<SpellsComponent?> ent, [ForbidLiteral] EntProtoId spell, bool force = false)
    {
        if (!_spellsQuery.Resolve(ent.Owner, ref ent.Comp))
            return null;

        if (!AllSpells.Contains(spell))
        {
            // this should not ever happen
            Log.Error($"No spell prototype found for: {spell}");
            return null;
        }

        // todo: spells with charges should add to the charges of an existing spell...!!
        if (!force && GetSpell(ent, spell) != null)
            return null;

        var spellSpawn = PredictedSpawnAtPosition(spell, Transform(ent).Coordinates);
        InsertSpell(ent, spellSpawn);

        // if insertion failed, the spawned spell must be deleted
        if (!ent.Comp.Container.Contains(spellSpawn))
        {
            Log.Warning($"Failed to add {spell} to the user {ent.Owner}");
            PredictedQueueDel(spellSpawn);
            return null;
        }

        return spellSpawn;
    }

    /// <summary>
    /// Inserts a spell into the learned spells of the user
    /// </summary>
    public void InsertSpell(Entity<SpellsComponent?> ent, EntityUid spell)
    {
        if (!_spellsQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        if (_actions.GetAction(spell) is not { } action)
        {
            // spells should always have action component
            Log.Error($"Tried to add {spell} but it does not have an action component.");
            return;
        }

        action.Comp.AttachedEntity = ent.Owner;
        DirtyField(action.AsNullable(), nameof(ActionComponent.AttachedEntity));

        _container.Insert(spell, ent.Comp.Container);
    }

    /// <summary>
    /// Gets either all learned, or all active spells of the user
    /// </summary>
    /// <param name="ent">The entity</param>
    /// <param name="activeOnly">If true, it will only return the active spells. If false, the learned ones only (non-active)</param>
    /// <returns>A list containing spells based on if they are active or not</returns>
    public List<EntityUid> GetSpells(Entity<SpellsComponent?> ent, bool activeOnly)
    {
        if (!_spellsQuery.Resolve(ent.Owner, ref ent.Comp))
            return new();

        var spells = new List<EntityUid>();
        foreach (var spell in ent.Comp.Container.ContainedEntities)
        {
            if (!_spellQuery.TryComp(spell, out var spellComp))
                continue;

            if (spellComp.Active != activeOnly)
                continue;

            spells.Add(spell);
        }

        return spells;
    }

    /// <summary>
    /// Removes a spell from the user
    /// </summary>
    /// <param name="ent">The entity</param>
    /// <param name="spell">The spell prototype to remove</param>
    public void RemoveSpell(Entity<SpellsComponent?> ent, [ForbidLiteral] EntProtoId spell)
    {
        if (!_spellsQuery.Resolve(ent.Owner, ref ent.Comp) || GetSpell(ent, spell) is not { } spellEntity)
            return;

        RemoveSpell(ent, spellEntity);
    }

    /// <summary>
    /// Removes a spell from the user
    /// </summary>
    /// <param name="ent">The entity</param>
    /// <param name="spell">The spell to remove</param>
    public void RemoveSpell(Entity<SpellsComponent?> ent, EntityUid spell)
    {
        if (!_spellsQuery.Resolve(ent.Owner, ref ent.Comp) || !_container.Remove(spell,  ent.Comp.Container))
            return;

        PredictedQueueDel(spell);
    }

    /// <summary>
    /// Gets the first instance of a spell
    /// </summary>
    /// <param name="ent">The entity</param>
    /// <param name="spellProto">The spell prototype</param>
    /// <returns>The spell entity, null otherwise</returns>
    public EntityUid? GetSpell(Entity<SpellsComponent?> ent, [ForbidLiteral] EntProtoId spellProto)
    {
        if (!_spellsQuery.Resolve(ent.Owner, ref ent.Comp))
            return null;

        foreach (var spell in ent.Comp.Container.ContainedEntities)
        {
            if (Prototype(spell) is { } spellEntityProto && spellEntityProto == spellProto)
                return spell;
        }

        return null;
    }

    /// <summary>
    /// Moves the entity's selected spell index by 1
    /// </summary>
    /// <param name="ent">The entity</param>
    /// <param name="moveType">Do we want to move the index up, or down</param>
    public void MoveSelectedSpell(Entity<SpellsComponent?> ent, SpellMove moveType)
    {
        if (!_spellsQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        var currentSpell = ent.Comp.SelectedSpell;
        var activeSpells = GetSpells(ent, activeOnly: true);
        if (activeSpells.Count == 0)
            return;

        switch (moveType)
        {
            case SpellMove.Up:
            {
                // reset to 0 if index out of range, otherwise count up
                currentSpell = currentSpell + 1 >= activeSpells.Count ? 0 : currentSpell + 1;
                break;
            }
            case SpellMove.Down:
            {
                // reset to highest index if index out of range, otherwise count down
                currentSpell = currentSpell - 1 < 0 ? activeSpells.Count - 1 : currentSpell - 1;
                break;
            }
        }

        ent.Comp.SelectedSpell = currentSpell;
        Dirty(ent);

        UpdateUi((ent.Owner, ent.Comp));
    }

    /// <summary>
    /// Adds a slot to our memory
    /// </summary>
    /// <param name="ent">The entity</param>
    /// <param name="amount">How many slots to add</param>
    /// <param name="type">In which section to add the slots</param>
    public void AddSlot(Entity<SpellsComponent?> ent, int amount, SpellTransfer type)
    {
        if (type == SpellTransfer.Active)
        {
            ent.Comp.CurrentSlots = Math.Clamp(ent.Comp.CurrentSlots + amount, 0, 99); // 99 limit seems reasonable
            DirtyField(ent, nameof(SpellsComponent.CurrentSlots));
            return;
        }

        ent.Comp.MaxLearned = Math.Clamp(ent.Comp.MaxLearned + amount, 0, 99);
        DirtyField(ent, nameof(SpellsComponent.MaxLearned));
    }

    #endregion

    #region Virtual

    /// <summary>
    /// Refreshes the spell HUD slot, and optionally the spell window
    /// </summary>
    /// <param name="ent">The entity</param>
    /// <param name="refresh">If true, it will also refresh the window UI (if it's open)</param>
    protected virtual void UpdateUi(Entity<SpellsComponent> ent, bool refresh = false) { }

    #endregion

    private void MoveSelectedSpell(ICommonSession? session, SpellMove moveType)
    {
        if (session is not { } playerSession)
            return;

        if (playerSession.AttachedEntity is not { Valid: true } uid || !Exists(uid))
            return;

        MoveSelectedSpell(uid, moveType);
    }
}
