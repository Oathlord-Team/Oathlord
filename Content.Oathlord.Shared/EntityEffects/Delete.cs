using Content.Shared.EntityEffects;

namespace Content.Oathlord.Shared.EntityEffects;

/// <summary>
/// Effect that deletes the target entity, be careful with this...
/// </summary>
public sealed partial class Delete : EntityEffectBase<Delete>
{
    /// <summary>
    /// Whether to queue the deletion
    /// </summary>
    [DataField]
    public bool Queued;
}

public sealed class DeleteEffectSystem : EntityEffectSystem<MetaDataComponent, Delete>
{
    protected override void Effect(Entity<MetaDataComponent> ent, ref EntityEffectEvent<Delete> args)
    {
        if (TerminatingOrDeleted(ent, ent.Comp))
            return;

        var meta = ent.AsNullable();
        if (args.Effect.Queued)
            PredictedQueueDel(meta);
        else
            PredictedDel(meta);
    }
}
