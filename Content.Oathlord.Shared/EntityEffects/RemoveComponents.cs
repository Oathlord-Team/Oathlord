using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.EntityEffects;

/// <summary>
/// Effect that adds component on the entity
/// </summary>
public sealed partial class RemoveComponents : EntityEffectBase<RemoveComponents>
{
    /// <summary>
    /// The components to remove
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry Components;
}

public sealed class RemoveComponentsEffectSystem : EntityEffectSystem<MetaDataComponent, RemoveComponents>
{
    protected override void Effect(Entity<MetaDataComponent> ent, ref EntityEffectEvent<RemoveComponents> args)
    {
        EntityManager.RemoveComponents(ent, args.Effect.Components);
    }
}
