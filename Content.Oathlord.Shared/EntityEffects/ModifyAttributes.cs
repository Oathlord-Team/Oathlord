using Content.Oathlord.Shared.CharacterAttributes;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.EntityEffects;

/// <summary>
/// Effect that modifies the attributes on an entity
/// </summary>
public sealed partial class ModifyAttributes : EntityEffectBase<ModifyAttributes>
{
    /// <summary>
    /// The attributes to adjust, and by how much.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<EntProtoId, int> Points = new();
}

public sealed partial class ModifyAttributesEffectSystem : EntityEffectSystem<CharacterAttributesComponent, ModifyAttributes>
{
    [Dependency] private CharacterAttributesSystem _attributes = default!;

    protected override void Effect(Entity<CharacterAttributesComponent> ent, ref EntityEffectEvent<ModifyAttributes> args)
    {
        _attributes.AdjustAttributes(ent.AsNullable(), args.Effect.Points);
    }
}
