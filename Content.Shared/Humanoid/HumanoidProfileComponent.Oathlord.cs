using Content.Oathlord.Common.Oaths;
using Robust.Shared.Prototypes;

namespace Content.Shared.Humanoid;

public sealed partial class HumanoidProfileComponent
{
    [DataField]
    public ProtoId<OathPrototype> Oath = HumanoidProfileSystem.DefaultOath;
}
