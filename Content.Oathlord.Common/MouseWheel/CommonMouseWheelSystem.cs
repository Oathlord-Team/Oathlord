using System.Numerics;

namespace Content.Oathlord.Common.MouseWheel;

public abstract partial class CommonMouseWheelSystem : EntitySystem
{
    public abstract void HandleMouseWheel(Vector2 delta);
}
