using Content.Shared.EntityEffects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Oathlord.Shared.StatusEffects;

/// <summary>
/// Component that runs entity effects on the status effect owner based on a timespan.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class StatusEffectEffectsComponent : Component
{
    /// <summary>
    /// How often to run entity effects
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1f);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate;

    /// <summary>
    /// The entity effects to run
    /// </summary>
    [DataField(required: true)]
    public EntityEffect[] Effects = default!;
}
