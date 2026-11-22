using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.EntityEffects;

/// <summary>
/// Effect that adds component on the entity
/// </summary>
public sealed partial class AddComponents : EntityEffectBase<AddComponents>
{
    /// <summary>
    /// The components to add
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry Components;
}

public sealed class AddComponentsEffectSystem : EntityEffectSystem<MetaDataComponent, AddComponents>
{
    protected override void Effect(Entity<MetaDataComponent> ent, ref EntityEffectEvent<AddComponents> args)
    {
        EntityManager.AddComponents(ent, args.Effect.Components);
    }
}
