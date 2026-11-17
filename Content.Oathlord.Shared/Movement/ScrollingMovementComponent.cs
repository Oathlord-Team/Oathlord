using Content.Shared.Destructible.Thresholds;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Oathlord.Shared.Movement;

/// <summary>
/// Component used on entities that can adjust their walking speed via mouse wheel scrolling
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class ScrollingMovementComponent : Component
{
    /// <summary>
    /// The maximum and minimum value <see cref="Current"/> can get
    /// </summary>
    [DataField]
    public MinMax WalkSpeedRange = new(0.1f, 1f);

    /// <summary>
    /// The current value used to multiply the walk speed modifier of the entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Current = 1f;

    /// <summary>
    /// Delays the next update of <see cref="Current"/> so it's not spammed by the user
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextDelay;
}
