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
        if (args.CurrentPoints % ent.Comp.ActiveIncrease == 0)
        {
            _spellcasting.AddSlot(user, 1, SpellTransfer.Active);
        }

        if (args.CurrentPoints % ent.Comp.LearnedIncrease == 0)
        {
            _spellcasting.AddSlot(user, ent.Comp.LearnedAmount, SpellTransfer.Learned);
        }
    }
}
