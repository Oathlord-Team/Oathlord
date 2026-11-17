using Content.Shared.EntityEffects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Oathlord.Shared.EntityEffects;

/// <summary>
/// Effect that plays a sound at the target's location
/// </summary>
public sealed partial class PlaySound : EntityEffectBase<PlaySound>
{
    /// <summary>
    /// The sound to play
    /// </summary>
    [DataField(required: true)]
    public SoundSpecifier Sound = default!;

    /// <summary>
    /// Play the sound at the position instead of parented to the target entity.
    /// Useful if the entity is deleted after.
    /// </summary>
    [DataField]
    public bool Positional = true;
}

public sealed partial class PlaySoundEffectSystem : EntityEffectSystem<TransformComponent, PlaySound>
{
    [Dependency] private SharedAudioSystem _audio = default!;

    protected override void Effect(Entity<TransformComponent> entity, ref EntityEffectEvent<PlaySound> args)
    {
        var predicted = args.Predicted;
        var sound = args.Effect.Sound;
        var user = args.User ?? entity.Owner;

        if (predicted)
        {
            if (args.Effect.Positional)
                _audio.PlayPredicted(sound, entity.Comp.Coordinates, user);
            else
                _audio.PlayPredicted(sound, entity, user);
            return;
        }

        if (args.Effect.Positional)
            _audio.PlayPvs(sound, entity.Comp.Coordinates);
        else
            _audio.PlayPvs(sound, entity);
    }
}

