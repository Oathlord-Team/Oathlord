using Content.Oathlord.Common.Oaths;
using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;

namespace Content.Shared.Preferences;

public sealed partial class HumanoidCharacterProfile
{
    [DataField]
    public ProtoId<OathPrototype> Oath = HumanoidProfileSystem.DefaultOath;

    public HumanoidCharacterProfile WithOath(ProtoId<OathPrototype> oath)
    {
        return new(this) { Oath = oath };
    }

    private void EnsureValidOathlord(IPrototypeManager proto)
    {
        if (!proto.HasIndex(Oath))
            Oath = HumanoidProfileSystem.DefaultOath;
    }
}
