using Content.Oathlord.Client.Movement;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;

namespace Content.Oathlord.Client.UserInterface.Systems.ScrollingMovement;

[UsedImplicitly]
public sealed partial class ScrollingMovementController : UIController, IOnSystemChanged<ClientScrollingMovementSystem>
{
    private ScrollingMovementWidget? UI => UIManager.GetActiveUIWidgetOrNull<ScrollingMovementWidget>();

    public void OnSystemLoaded(ClientScrollingMovementSystem system)
    {
        system.OnUpdateMovement += OnUpdateMovement;
    }

    public void OnSystemUnloaded(ClientScrollingMovementSystem system)
    {
        system.OnUpdateMovement -= OnUpdateMovement;
    }

    private void OnUpdateMovement(object? sender, float value)
    {
        UI?.Blink(value);
    }
}

