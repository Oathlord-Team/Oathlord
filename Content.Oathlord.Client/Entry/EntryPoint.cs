// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Oathlord.Client.Input;
using Robust.Client.Input;
using Robust.Shared.ContentPack;

namespace Content.Oathlord.Client.Entry;

public sealed partial class EntryPoint : GameClient
{
    [Dependency] private IInputManager _input = default!;

    public override void Init()
    {
        base.Init();

        Dependencies.BuildGraph();
        Dependencies.InjectDependencies(this);
    }

    public override void PostInit()
    {
        base.PostInit();

        OathlordInputContexts.SetupContexts(_input.Contexts);
    }
};
