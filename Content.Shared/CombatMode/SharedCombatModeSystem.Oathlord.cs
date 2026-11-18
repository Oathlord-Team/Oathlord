namespace Content.Shared.CombatMode;

public partial class SharedCombatModeSystem
{
    [Dependency] private EntityQuery<CombatModeComponent> _combatModeQuery = default!;

    public void SetToggle(Entity<CombatModeComponent?> ent, bool value)
    {
        if (!_combatModeQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.CanToggle = value;
        Dirty(ent);
    }
}
