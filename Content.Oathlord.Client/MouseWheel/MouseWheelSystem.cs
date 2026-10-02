using System.Linq;
using Content.Oathlord.Common.Input;
using Content.Oathlord.Common.MouseWheel;
using Robust.Client.GameObjects;
using Robust.Client.Input;
using Robust.Shared.Input;

namespace Content.Oathlord.Client.MouseWheel;

public sealed partial class MouseWheelSystem : CommonMouseWheelSystem
{
    [Dependency] private IInputManager _inputMan = default!;
    [Dependency] private InputSystem _input = default!;

    private readonly Dictionary<int, Action<Vector2>> _methods = new();

    public override void HandleMouseWheel(Vector2 delta)
    {
        _methods.Clear();
        AddMouseWheelBehaviour(OathlordKeyFunctions.MovementMod, OnMovement);

        var (priority, method) = _methods.MaxBy(x => x.Key);
        if (priority < 0)
            return;

        method(delta);
    }

    /// <summary>
    /// Adds a new behavior to the methods dictionary
    /// </summary>
    /// <param name="function">The bound key to add</param>
    /// <param name="action">The action that will be performed for this key</param>
    public void AddMouseWheelBehaviour(BoundKeyFunction function, Action<Vector2> action)
    {
        var prio = GetPriority(function);
        if (_methods.TryAdd(prio, action))
            return;

        Log.Warning($"Tried to add a key to the mouse wheel methods dictionary, but it already exists: {function.FunctionName}");
    }

    /// <summary>
    /// Returns the priority of the key, for mouse wheel behaviours
    /// </summary>
    public int GetPriority(BoundKeyFunction function)
    {
        if (!_inputMan.TryGetKeyBinding(function, out _))
            return 0;

        return _input.CmdStates.GetState(function) == BoundKeyState.Down ? 1 : -1;
    }
}


