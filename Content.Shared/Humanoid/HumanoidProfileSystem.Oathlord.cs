using Content.Oathlord.Common.Oaths;
using Robust.Shared.Prototypes;

namespace Content.Shared.Humanoid;

public sealed partial class HumanoidProfileSystem
{
    [Dependency] private CommonOathSystem _oath = default!;

    public static readonly ProtoId<OathPrototype> DefaultOath = "Oathless";

    public void SetOath(Entity<HumanoidProfileComponent> ent, ProtoId<OathPrototype> oath)
    {
        ent.Comp.Oath = oath;

        _oath.ApplyOath(ent, oath);
    }
}
