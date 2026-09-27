using Content.Shared.FixedPoint;

namespace Content.Oathlord.Shared.Mana;

/// <summary>
/// Public API for anything mana-related.
/// Currently, it is barebones (no events etc) but will get expanded once more content gets added
/// </summary>
public abstract partial class ManaSystem : EntitySystem
{
    [Dependency] private EntityQuery<ManaUserComponent> _manaQuery = default!;

    #region  Public Api

    /// <summary>
    /// Returns the current mana of this entity
    /// </summary>
    public FixedPoint2 GetMana(Entity<ManaUserComponent?> ent)
    {
        if (!_manaQuery.Resolve(ent.Owner, ref ent.Comp))
            return 0;

        return ent.Comp.CurrentMana;
    }

    /// <summary>
    /// Returns the maximum mana this entity can have
    /// </summary>
    public FixedPoint2 GetMaxMana(Entity<ManaUserComponent?> ent)
    {
        if (!_manaQuery.Resolve(ent.Owner, ref ent.Comp))
            return 0;

        return ent.Comp.MaxMana;
    }

    /// <summary>
    /// Returns whether this entity can use mana
    /// </summary>
    public bool CanUseMana(Entity<ManaUserComponent?> ent)
    {
        if (!_manaQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        return ent.Comp.CanUse;
    }

    /// <summary>
    /// Adjusts the current mana of the entity
    /// </summary>
    /// <param name="amount">The amount to add</param>
    /// <param name="ent">The entity</param>>
    /// <param name="force">Whether to adjust mana without checking if the user can use mana</param>
    public void AdjustMana(Entity<ManaUserComponent?> ent, FixedPoint2 amount, bool force = false)
    {
        if (!_manaQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        if (force && !CanUseMana(ent))
            return;

        ent.Comp.CurrentMana = FixedPoint2.Clamp(ent.Comp.CurrentMana + amount, 0, ent.Comp.MaxMana);
        DirtyField(ent, nameof(ManaUserComponent.CurrentMana));

        UpdateHud((ent.Owner, ent.Comp));
    }

    #endregion

    protected virtual void UpdateHud(Entity<ManaUserComponent> ent) { }
}
