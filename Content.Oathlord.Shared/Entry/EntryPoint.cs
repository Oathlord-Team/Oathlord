// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.ContentPack;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Entry;

public sealed partial class EntryPoint : GameShared
{
    [Dependency] private IPrototypeManager _proto = default!;

    public override void PreInit()
    {
        base.PreInit();

        Dependencies.InjectDependencies(this);
    }

    public override void Init()
    {
        base.Init();

        _proto.PartialDirectory(new("/Prototypes/_Oathlord/Partials"), 1);
    }
}
