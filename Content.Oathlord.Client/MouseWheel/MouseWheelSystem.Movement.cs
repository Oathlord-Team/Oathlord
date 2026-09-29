using Content.Oathlord.Shared.Movement;

namespace Content.Oathlord.Client.MouseWheel;

public sealed partial class MouseWheelSystem
{
    private void OnMovement(Vector2 delta)
    {
        var ev = new RequestScrollingWalkSpeed(delta.Y);
        RaisePredictiveEvent(ev);
    }
}
