using Content.Shared.EntityEffects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Oathlord.Shared.Movement;

/// <summary>
/// Component used on entities that get effects while sprinting
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class EffectsMovementComponent : Component
{
    /// <summary>
    /// Effects that are run on the user while sprinting
    /// </summary>
    [DataField(required: true)]
    public ProtoId<EntityEffectPrototype> Effect;

    /// <summary>
    /// Since <see cref="MoveEvent"/> gets called for every movement change,
    /// we must delay the effects for at least some milliseconds, so they don't get spammed
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.5f);

    /// <summary>
    /// Whether effects can be applied
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CanApply = true;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate;
}
