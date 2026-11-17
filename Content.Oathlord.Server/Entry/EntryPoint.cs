// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.ContentPack;

namespace Content.Oathlord.Server.Entry;

public sealed class EntryPoint : GameServer
{
    public override void Init()
    {
        base.Init();

        Dependencies.BuildGraph();
        Dependencies.InjectDependencies(this);
    }
};
