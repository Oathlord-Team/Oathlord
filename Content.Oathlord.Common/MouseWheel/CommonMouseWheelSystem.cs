using System.Numerics;

namespace Content.Oathlord.Common.MouseWheel;

public abstract partial class CommonMouseWheelSystem : EntitySystem
{
    /// <summary>
    /// Generic method that handles mouse wheel behaviours, and invokes their methods
    /// </summary>
    /// <param name="delta">The delta provided by the mouse wheel</param>
    public abstract void HandleMouseWheel(Vector2 delta);
}
