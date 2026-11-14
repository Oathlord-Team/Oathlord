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

    /// <summary>
    /// Adjusts the attributes on the entity
    /// </summary>
    /// <param name="ent">The entity containg the attribtues</param>
    /// <param name="toChange">A dictionary containing attribute entities as keys, and points to give as int</param>
    public void AdjustAttributes(Entity<CharacterAttributesComponent?> ent, Dictionary<EntityUid, int> toChange)
    {
        if (!_characterAttrQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        if (ent.Comp.Points == 0)
            return; // no points to give lol

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
                Log.Info($"Wasted more than {points}, leaving");
                break;
            }

            AdjustAttribute(attribute, value);
            wasted += value;
        }

        ent.Comp.Points = Math.Clamp(ent.Comp.Points - wasted, 0, 10);
        Dirty(ent);
    }

    /// <summary>
    /// Adjusts an attribute's given points.
    /// </summary>
    /// <param name="ent">The attribute</param>
    /// <param name="points">The points to adjust</param>
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

    /// <summary>
    /// Returns a list of all attributes we have on our character
    /// </summary>
    public List<EntityUid> GetAttributes(Entity<CharacterAttributesComponent?> ent)
    {
        if (!_characterAttrQuery.Resolve(ent.Owner, ref ent.Comp))
            return new();

        if (!_container.TryGetContainer(ent, CharacterAttributesComponent.ContainerId, out var container) || container == null)
            return new();

        return container.ContainedEntities.ToList();
    }

    /// <summary>
    /// Returns all points we have on our character
    /// </summary>
    public int GetPoints(Entity<CharacterAttributesComponent?> ent)
    {
        if (!_characterAttrQuery.Resolve(ent.Owner, ref ent.Comp))
            return 0;

        return ent.Comp.Points;
    }
}

/// <summary>
/// Raised from client to server, to apply character attributes.
/// </summary>
/// <param name="changedPoints">The points we changed</param>
[Serializable, NetSerializable]
public sealed class ApplyCharacterAttributesEvent(Dictionary<NetEntity, int> changedPoints) : EntityEventArgs
{
    public Dictionary<NetEntity, int> ChangedPoints = changedPoints;
}
