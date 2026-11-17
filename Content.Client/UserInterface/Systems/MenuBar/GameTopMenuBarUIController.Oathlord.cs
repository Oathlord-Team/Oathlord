using Content.Client.UserInterface.Systems.MenuBar.Widgets;

namespace Content.Client.UserInterface.Systems.MenuBar;

public sealed partial class GameTopMenuBarUIController
{
    public event Action<GameTopMenuBar>? OnLoad;
    public event Action<GameTopMenuBar>? OnUnload;

    public void RaiseOnLoad(GameTopMenuBar? gameTopMenuBar)
    {
        if (gameTopMenuBar is not { } bar)
            return;

        OnLoad?.Invoke(bar);
    }

    public void RaiseOnUnload(GameTopMenuBar? gameTopMenuBar)
    {
        if (gameTopMenuBar is not { } bar)
            return;

        OnUnload?.Invoke(bar);
    }
}
