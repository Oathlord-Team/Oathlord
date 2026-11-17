using Content.Oathlord.Shared.Spellcasting.Components;
using Content.Oathlord.Shared.Spellcasting.Systems;

namespace Content.Oathlord.Shared.CharacterAttributes.Attributes;

public sealed partial class IntelligenceAttributeSystem : EntitySystem
{
    [Dependency] private SpellcastingSystem _spellcasting = default!;

    [SubscribeLocalEvent]
    private void OnAttributeAdjusted(Entity<IntelligenceAttributeComponent> ent, ref AttributeAdjustedEvent args)
    {
        var user = args.User;
        var currentPoints = args.CurrentPoints;
        var previousPoints = args.PreviousPoints;

        // We need to calculate if the points added were negative or positive, so the loop can handle removing slots too.
        var sign = currentPoints >= previousPoints ? 1 : -1;
        for (var i = previousPoints; i != currentPoints; i += sign)
        {
            // 0modX will be 0, however that's fine if we are ONLY subtracting
            // This also ensures we add and undo the correct amount of slots.
            var check = sign > 0 ? i + 1 : i;

            if (check % ent.Comp.LearnedIncrease == 0)
                _spellcasting.AddSlot(user, ent.Comp.LearnedAmount * sign, SpellTransfer.Learned);

            if (check % ent.Comp.ActiveIncrease == 0)
                _spellcasting.AddSlot(user, ent.Comp.ActiveAmount * sign, SpellTransfer.Active);
        }
    }
}
