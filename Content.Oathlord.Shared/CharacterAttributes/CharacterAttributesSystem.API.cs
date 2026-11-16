using System.Linq;
using Content.Shared.Database;
using Content.Shared.Popups;
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
                Log.Error($"Tried to add an attribute {attribute.Id} that already " +
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
    /// <param name="silent">Should we notify the user about the adjustments?</param>
    public void AdjustAttributes(Entity<CharacterAttributesComponent?> ent, Dictionary<EntityUid, int> toChange, bool silent = false)
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
                break;

            if (!TryAdjustAttribute(attribute, value, ent))
                continue;

            wasted += value;
        }

        ent.Comp.Points = Math.Clamp(ent.Comp.Points - wasted, 0, 10);
        Dirty(ent);

        if (!silent)
            _popup.PopupEntity("Your resolve increases...", ent, PopupType.SmallCaution);

        _admin.Add(LogType.CharacterAttribute, LogImpact.Low,
            $"{ent.Owner} wasted {wasted} points to increase: {toChange.ToString()} attributes. They now have {points} points left.");
    }

    /// <summary>
    /// Adjust attributes by prototype.
    /// </summary>
    /// <param name="ent">The entity containing the attributes</param>
    /// <param name="attributes">A dictionary holding attributes and points to adjust them by</param>
    /// <param name="silent">Whether to notify the user about this</param>
    public void AdjustAttributes(Entity<CharacterAttributesComponent?> ent, [ForbidLiteral] Dictionary<EntProtoId, int> attributes, bool silent = false)
    {
        if (!_characterAttrQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        var dict = new Dictionary<EntityUid, int>();
        var attributeList = GetAttributes(ent);
        foreach (var attribute in attributeList)
        {
            if (Prototype(attribute) is not { } proto || !attributes.TryGetValue(proto, out var value))
                continue;

            dict.Add(attribute, value);
        }

        AdjustAttributes(ent, dict, silent);
    }

    /// <summary>
    /// Adjusts an attribute's given points.
    /// </summary>
    /// <param name="ent">The attribute</param>
    /// <param name="points">The points to adjust</param>
    /// <param name="user">The attribute's owner (not container)</param>
    /// <returns>True if the adjustment was successful, false if nothing was adjusted</returns>
    public bool TryAdjustAttribute(Entity<CharacterAttributeComponent?> ent, int points, EntityUid user)
    {
        if (!_attrQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        // Validate that the prototype is actually ours in case of malf client
        if (Prototype(ent) is not { } proto || !AllAttributes.Contains(proto))
        {
            Log.Warning($"Invalid character attribute was provided by {ToPrettyString(user)}. The attribute {ToPrettyString(ent)} is not contained anywhere.");
            return false;
        }

        if (ent.Comp.CurrentPoints + points > ent.Comp.MaxPoints)
            return false;

        var prev = ent.Comp.CurrentPoints;
        ent.Comp.CurrentPoints = Math.Clamp(ent.Comp.CurrentPoints + points, 0, ent.Comp.MaxPoints);
        Dirty(ent);

        var ev = new AttributeAdjustedEvent(ent.Comp.CurrentPoints, prev, user);
        RaiseLocalEvent(ent, ref ev);
        return true;
    }

    /// <summary>
    /// Tries to adjust an attribute by prototype.
    /// </summary>
    public bool TryAdjustAttribute(Entity<CharacterAttributesComponent?> ent, [ForbidLiteral] EntProtoId attributeProto, int points)
    {
        if (!_characterAttrQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        var attributes = GetAttributes(ent);
        foreach (var attribute in attributes)
        {
            if (Prototype(attribute) is not { } proto)
                continue;

            if (proto != attributeProto)
                continue;

            TryAdjustAttribute(attribute, points, ent);
            return true;
        }

        return false;
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

    /// <summary>
    /// Returns all points we have on our attribute
    /// </summary>
    public int GetAttributePoints(Entity<CharacterAttributeComponent?> ent)
    {
        if (!_attrQuery.Resolve(ent.Owner, ref ent.Comp))
            return 0;

        return ent.Comp.CurrentPoints;
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

/// <summary>
/// Raised when an attributes points have been adjusted
/// </summary>
/// <param name="CurrentPoints">The current points we have</param>
/// <param name="PreviousPoints">The points we had before</param>
/// <param name="User">The entity holder of the attribute that was adjusted</param>
[ByRefEvent]
public record struct AttributeAdjustedEvent(int CurrentPoints, int PreviousPoints, EntityUid User);
