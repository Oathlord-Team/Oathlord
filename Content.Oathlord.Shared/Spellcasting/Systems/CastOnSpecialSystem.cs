using Content.Oathlord.Shared.ItemSpecial;
using Content.Oathlord.Shared.Spellcasting.Components;

namespace Content.Oathlord.Shared.Spellcasting.Systems;

public sealed partial class CastOnSpecialSystem : EntitySystem
{
    [Dependency] private SpellcastingSystem _spellcasting = default!;

    [SubscribeLocalEvent]
    public void OnItemSpecial(Entity<CastOnSpecialComponent> ent, ref ItemSpecialEvent args)
    {
        _spellcasting.CastSpell(args.User, ent, args.Target, args.Coords);
    }
};
