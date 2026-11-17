using Content.Oathlord.Shared.Spellcasting.Components;
using Content.Oathlord.Shared.Spellcasting.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Spellcasting.Systems;

/// <summary>
/// Public api for <see cref="SpellComponent"/>
/// </summary>
public partial class SpellcastingSystem
{
    /// <summary>
    /// List of every spell prototype loaded in the game
    /// </summary>
    [ViewVariables]
    public List<EntProtoId> AllSpells = new();

    public void InitializeSpells()
    {
        LoadSpells();
    }

    [SubscribeLocalEvent]
    public void OnPrototypesReload(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<EntityPrototype>())
            return;

        LoadSpells();
    }

    #region Public Api

    /// <summary>
    /// Activates a spell, making it ready to be used
    /// </summary>
    public void SetActive(Entity<SpellComponent?> ent, bool active)
    {
        if (!_spellQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.Active = active;
        Dirty(ent);
    }

    /// <summary>
    /// Checks whether the spell is valid,
    /// usually that means it should have its <see cref="SpellComponent.Active"/> property set to true
    /// </summary>
    public bool IsValid(Entity<SpellComponent?> ent)
    {
        if (!_spellQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        return ent.Comp.Active;
    }

    /// <summary>
    /// Returns all spell types this spell has
    /// </summary>
    public List<ProtoId<SpellTypePrototype>> GetSpellTypes(Entity<SpellComponent?> ent)
    {
        if (!_spellQuery.Resolve(ent.Owner, ref ent.Comp))
            return new();

        return ent.Comp.Types;
    }

    #endregion

    private void LoadSpells()
    {
        AllSpells.Clear();
        var name = Factory.CompName<SpellComponent>();
        foreach (var proto in ProtoMan.EnumeratePrototypes<EntityPrototype>())
        {
            if (!proto.HasComp(name))
                continue;

            var id = proto.ID;
            AllSpells.Add(id);
        }
    }
}
