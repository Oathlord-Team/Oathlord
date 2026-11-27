using System.Linq;
using Content.Oathlord.Shared.Economy.Components;
using Content.Shared.Stacks;
using Content.Shared.Storage;
using Content.Shared.Storage.Components;

namespace Content.Oathlord.Shared.Economy.Systems;

public sealed partial class EconomyValueSystem : EntitySystem
{
    [Dependency] private EntityQuery<EconomyValueComponent> _economyValueQuery = default!;

    /// <summary>
    /// Returns the value of the entity in the economy.
    /// </summary>
    public int GetValue(Entity<EconomyValueComponent?> ent)
    {
        if (!_economyValueQuery.Resolve(ent.Owner, ref ent.Comp))
            return 0;

        var ev = new GetEconomyValueEvent(ent.Comp.Value);
        RaiseLocalEvent(ent, ref ev);
        if (ev.Handled)
            return ev.NewValue;

        return ent.Comp.Value;
    }

    [SubscribeLocalEvent]
    private void OnGetStackValue(Entity<StackComponent> ent, ref GetEconomyValueEvent args)
    {
        // Stacked items are well... stacked. So we have to account them for the whole package
        args.NewValue = args.Current * ent.Comp.Count;
        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnGetStorageValue(Entity<StorageComponent> ent, ref GetEconomyValueEvent args)
    {
        // Storage entities like backpacks can include multiple items inside
        var nestedItemsValue = GetValues(ent.Comp.StoredItems.Keys.ToList());
        args.NewValue = nestedItemsValue + args.Current;

        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnGetEntityStorageValue(Entity<EntityStorageComponent> ent, ref GetEconomyValueEvent args)
    {
        // Same as above
        var nestedItemsValue = GetValues(ent.Comp.Contents.ContainedEntities.ToList());
        args.NewValue = nestedItemsValue + args.Current;

        args.Handled = true;
    }

    /// <summary>
    /// Helper to get the values of multiple entities
    /// </summary>
    /// <param name="entities">The entities to gather values against</param>
    /// <returns>The overall value of all the entities</returns>
    private int GetValues(List<EntityUid> entities)
    {
        var overallValue = 0;
        foreach (var item in entities)
        {
            // don't wanna spam errors
            if (!_economyValueQuery.TryComp(item, out var economyValue))
                continue;

            overallValue += GetValue((item, economyValue));
        }

        return overallValue;
    }
}

/// <summary>
/// Raised when trying to get the value of an entity in the economy.
/// </summary>
/// <param name="Current">The current value of the entity</param>
/// <param name="NewValue">The modified value</param>
/// <param name="Handled">If handled, the <see cref="NewValue"/> will be used for returning the entity's economic value</param>
[ByRefEvent]
public record struct GetEconomyValueEvent(int Current, int NewValue = 0, bool Handled = false);
