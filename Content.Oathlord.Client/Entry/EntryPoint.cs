using Robust.Shared.ContentPack;

namespace Content.Oathlord.Client.Entry;

public sealed class EntryPoint : GameClient
{
    public override void Init()
    {
        base.Init();

        Dependencies.BuildGraph();
        Dependencies.InjectDependencies(this);
    }
};
