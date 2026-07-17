using Robust.Shared;
using Robust.Shared.ContentPack;
using Robust.Shared.Configuration;

namespace Content.Oathlord.Client.Entry;

public sealed class EntryPoint : GameClient
{
    public override void Init()
    {
        base.Init();

        Dependencies.BuildGraph();
        Dependencies.InjectDependencies(this);

        var cfg = Dependencies.Resolve<IConfigurationManager>();
        cfg.SetCVar(CVars.InterfaceTheme, "OLDefaultTheme");
    }
};
