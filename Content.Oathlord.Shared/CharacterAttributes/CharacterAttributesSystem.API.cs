using System.Linq;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Oathlord.Shared.CharacterAttributes;

public sealed partial class CharacterAttributesSystem
{
    /// <summary>
    /// Adds an attribute to the container of the entity
    /// </summary>
    /// <param name="ent">The entity</param>
    /// <param name="attribute">The attribute to add</param>
    public void AddAttribute(Entity<CharacterAttributesComponent?> ent, [ForbidLiteral] EntProtoId attribute)
    {
        if (!_characterAttrQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        if (!AllAttributes.Contains(attribute))
            return;

        foreach (var entity in ent.Comp.Container.ContainedEntities)
        {
            // Don't allow duplicate attributes to go in...
            if (Prototype(entity) is not { } proto || proto == attribute)
            {
                Log.Warning($"Tried to add an attribute {attribute.Id} that already " +
                            $"exists on the entity {ToPrettyString(ent)}");
                return;
            }
        }

        PredictedTrySpawnInContainer(
            protoName: attribute,
            containerUid: ent,
            containerId: CharacterAttributesComponent.ContainerId,
            uid: out _);
    }

    public void AdjustAttributes(Entity<CharacterAttributesComponent?> ent, Dictionary<EntityUid, int> toChange)
    {
        if (!_characterAttrQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        if (ent.Comp.Points == 0)
            return; // no points to give lol

        // todo: fix this shit

        var points = ent.Comp.Points;
        var wasted = 0;

        var attributes = GetAttributes(ent);
        foreach (var attribute in attributes)
        {
            if (!toChange.TryGetValue(attribute, out var value))
                continue;

            if (value <= 0) // no lol
                continue;

            if (wasted >= points) // we reached the limit
            {
                Log.Info("Wasted more than points, leaving");
                break;
            }

            AdjustAttribute(attribute, value);
            wasted += value;
        }

        ent.Comp.Points = wasted;
        Dirty(ent);
    }

    public void AdjustAttribute(Entity<CharacterAttributeComponent?> ent, int points)
    {
        if (!_attrQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        // Validate that the prototype is actually ours in case of malf client
        if (Prototype(ent) is not { } proto || !AllAttributes.Contains(proto))
            return;

        ent.Comp.CurrentPoints = Math.Clamp(ent.Comp.CurrentPoints + points, 0, 10); // todo cvar max?
        Dirty(ent);
    }

    public List<EntityUid> GetAttributes(Entity<CharacterAttributesComponent?> ent)
    {
        if (!_characterAttrQuery.Resolve(ent.Owner, ref ent.Comp))
            return new();

        if (!_container.TryGetContainer(ent, CharacterAttributesComponent.ContainerId, out var container) || container == null)
            return new();

        return container.ContainedEntities.ToList();
    }

    public int GetPoints(Entity<CharacterAttributesComponent?> ent)
    {
        if (!_characterAttrQuery.Resolve(ent.Owner, ref ent.Comp))
            return 0;

        return ent.Comp.Points;
    }
}

[Serializable, NetSerializable]
public sealed class ApplyCharacterAttributesEvent(Dictionary<NetEntity, int> changedPoints) : EntityEventArgs
{
    public Dictionary<NetEntity, int> ChangedPoints = changedPoints;
}
