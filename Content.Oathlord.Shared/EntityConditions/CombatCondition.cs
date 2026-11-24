using Content.Shared.CombatMode;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.EntityConditions;

/// <summary>
/// Condition that checks whether we are in combat mode
/// </summary>
public sealed partial class CombatCondition : EntityConditionBase<CombatCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
        => string.Empty; // idc
}

public sealed partial class CombatConditionSystem : EntityConditionSystem<CombatModeComponent, CombatCondition>
{
    protected override void Condition(Entity<CombatModeComponent> entity, ref EntityConditionEvent<CombatCondition> args)
    {
        args.Result = entity.Comp.IsInCombatMode;
    }
}
