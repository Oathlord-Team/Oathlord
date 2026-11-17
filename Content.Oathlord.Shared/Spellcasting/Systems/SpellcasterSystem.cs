using Content.Oathlord.Shared.Spellcasting.Components;

namespace Content.Oathlord.Shared.Spellcasting.Systems;

/// <summary>
/// Public api for <see cref="SpellcasterComponent"/>
/// </summary>
public sealed partial class SpellcasterSystem : EntitySystem
{
    [Dependency] private SpellcastingSystem _spellcasting = default!;
    [Dependency] private EntityQuery<SpellcasterComponent> _spellcasterQuery = default!;

    /// <summary>
    /// Checks whether this entity can cast the specified spell
    /// </summary>
    /// <param name="ent">The item that will cast the spell</param>
    /// <param name="spell">The spell entity</param>
    /// <returns>True if it can cast the spell, false otherwise</returns>
    public bool CanCast(Entity<SpellcasterComponent?> ent, EntityUid spell)
    {
        if (!_spellcasterQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        var spellTypes = _spellcasting.GetSpellTypes(spell);
        if (spellTypes.Count == 0)
            return true; // can cast if there's no types specified

        // todo for discussion: some spells could have a requireAll variable instead of requiring 1:1 by default
        foreach (var spellType in spellTypes)
        {
            if (!ent.Comp.Types.Contains(spellType))
                return false;
        }

        return true;
    }
}
