using Robust.Shared.Serialization;

namespace Content.Oathlord.Shared.Movement;

/// <summary>
/// Adjusts walk speed when scrolling
/// </summary>
public sealed partial class ScrollingMovementSpeed : EntitySystem
{
    [EventSubscription]
    private void OnScrollingWalkSpeed(RequestScrollingWalkSpeed msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } player)
            return;

        // todo: delay + make component
    }
}

[NetSerializable, Serializable]
public sealed class RequestScrollingWalkSpeed(float deltaY) : EntityEventArgs
{
    public float DeltaY = deltaY;
};
