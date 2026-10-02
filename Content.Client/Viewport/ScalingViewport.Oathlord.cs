using Content.Oathlord.Common.MouseWheel;
using Robust.Client.UserInterface;

namespace Content.Client.Viewport;

public sealed partial class ScalingViewport
{
    private CommonMouseWheelSystem? _mouseWheel;

    protected override void MouseWheel(GUIMouseWheelEventArgs args)
    {
        base.MouseWheel(args);

        var delta = args.Delta;
        if (args.Handled || MathHelper.CloseToPercent(0f, delta.Y))
            return;

        _mouseWheel ??= _entityManager.System<CommonMouseWheelSystem>();
        _mouseWheel.HandleMouseWheel(delta);
    }
}
